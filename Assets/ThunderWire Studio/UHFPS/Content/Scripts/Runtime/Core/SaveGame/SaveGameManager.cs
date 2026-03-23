using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UHFPS.Runtime.Rendering;
using UHFPS.Scriptable;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/save-load-manager")]
    public class SaveGameManager : Singleton<SaveGameManager>
    {
        #region Structures
        [Serializable]
        public struct SaveablePair
        {
            public string Token;
            public MonoBehaviour Instance;

            public SaveablePair(string token, MonoBehaviour instance)
            {
                Token = token;
                Instance = instance;
            }
        }

        [Serializable]
        public struct RuntimeSaveable
        {
            public string TokenGUID;
            public GameObject InstantiatedObject;
            public SaveablePair[] SaveablePairs;
        }
        #endregion

        public const char TOKEN_SEPARATOR = '.';

        public enum LoadType
        {
            /// <summary>
            /// Normal scene loading.
            /// </summary>
            Normal,

            /// <summary>
            /// Load saved game from slot.
            /// </summary>
            LoadGameState,

            /// <summary>
            /// Load world state (Previous Scene Persistency).
            /// </summary>
            LoadWorldState,

            /// <summary>
            /// Load world last state (Previous Scene Persistency).
            /// </summary>
            LoadWorldLast,

            /// <summary>
            /// Load player data.
            /// </summary>
            LoadPlayer
        }

        public enum SaveType { Normal, Autosave, NextScene }

        /// <summary>
        /// Check if the game will be loaded (<see cref="GameLoadType"/> is not set to Normal).
        /// </summary>
        public static bool GameWillLoad => HasReference && GameLoadType != LoadType.Normal;

        /// <summary>
        /// Check if the game state of the level actually exists. Use with <see cref="GameWillLoad"/> to check if the game state will be actually loaded.
        /// </summary>
        public static bool GameStateExist => GameState != null;

        public static LoadType GameLoadType = LoadType.Normal;
        public static string LoadSceneName;
        public static string LoadFolderName;

        public ObjectReferences ObjectReferences;
        public CanvasGroup SavingIcon;
        public bool Debugging;

        public event Action<string> OnGameSaved;
        public event Action OnGameLoaded;

        public List<SaveablePair> worldSaveables = new();
        public List<RuntimeSaveable> runtimeSaveables = new();

        public static Dictionary<string, string> LastSceneSaves;
        private static JObject PlayerData;
        private static JObject GameState;
        private static float TimePlayed;

        private SaveType GameSaveType = SaveType.Normal;
        private Vector3 CustomSavePosition;
        private Vector2 CustomSaveRotation;
        private event Action OnWaitForSave;

        private string currentScene;
        private bool syncLoadingDone;
        private bool timerActive;
        private bool isSaved;

        private SpritesheetAnimation savingSpritesheet;
        private PlayerPresenceManager playerPresence;
        private ObjectiveManager objectiveManager;
        private Inventory inventory;

        public static SerializationAsset SerializationAsset
            => SerializationUtillity.SerializationAsset;

        private static SaveGameReader _SaveGameReader;
        public static SaveGameReader SaveGameReader
            => _SaveGameReader ??= new SaveGameReader(SerializationAsset);

        /// <summary>
        /// Shortcut to Level Manager Scene/Scene Loader
        /// </summary>
        public static string LMS => SerializationAsset.LevelManagerScene;

        /// <summary>
        /// Shortcut to MainMenu Scene
        /// </summary>
        public static string MM => SerializationAsset.MainMenuScene;

        private static string SavedGamePath
        {
            get
            {
                string savesPath = SerializationAsset.GetSavesPath();
                if (!Directory.Exists(savesPath))
                    Directory.CreateDirectory(savesPath);

                return savesPath;
            }
        }

        private void Awake()
        {
            if (SavingIcon != null) 
                savingSpritesheet = SavingIcon.GetComponent<SpritesheetAnimation>();

            playerPresence = GetComponent<PlayerPresenceManager>();
            objectiveManager = GetComponent<ObjectiveManager>();
            inventory = GetComponent<Inventory>();
        }

        private void Start()
        {
            bool loadFlag1 = GameLoadType == LoadType.LoadGameState;
            bool loadFlag2 = GameLoadType == LoadType.LoadWorldState;
            bool loadFlag3 = GameLoadType == LoadType.LoadPlayer;

            bool loadWorldType = loadFlag2 || GameLoadType == LoadType.LoadWorldLast;
            bool canLoadWorld = loadWorldType && SerializationAsset.PreviousScenePersistency;
            bool loadPlayer = !loadFlag1 && !canLoadWorld;

            if (Debugging)
            {
                if (loadFlag1)
                    Log("Attempting to load game state.");
                else if (loadWorldType)
                    Log("Attempting to load world state.");
            }

            // ================================
            // GAME/WORLD STATE LOADING
            // ================================
            if (loadFlag1 || canLoadWorld)
            {
                if(GameState != null)
                {
                    LoadGameState(GameState);
                    Log("Game state has been loaded.");
                }
                else if (loadFlag2)
                {
                    loadPlayer = true;
                    Log($"TLast world state does not exist. Loading is prevented.");
                }
            }

            // ================================
            // PLAYER DATA LOADING
            // ================================
            if(loadFlag3 || loadPlayer)
            {
                if(PlayerData != null)
                {
                    LoadPlayerData(PlayerData);
                    Log("Player data has been loaded.");
                    syncLoadingDone = true;
                }
            }

            currentScene = SceneManager.GetActiveScene().name;
            OnGameLoaded?.Invoke();
            timerActive = true;

            // clear loading cache data
            StartCoroutine(ClearAfterLoad());
        }

        private void Update()
        {
            if(timerActive) TimePlayed += Time.deltaTime;
        }

        IEnumerator ClearAfterLoad()
        {
            yield return new WaitUntil(() => syncLoadingDone);
            ClearLoadType();
        }

        private void Log(string message)
        {
            if (!Debugging)
                return;

            Debug.Log($"[SaveGameManager] {message}");
        }

        #region Exposed Methods
        /// <summary>
        /// Clear the load type for normal scene loading.
        /// <br>The game will not be loaded.</br>
        /// </summary>
        public static void ClearLoadType()
        {
            GameLoadType = LoadType.Normal;
            PlayerData = null;
            GameState = null;
        }

        /// <summary>
        /// Clear the load scene and folder name.
        /// </summary>
        public static void ClearLoadName()
        {
            LoadSceneName = string.Empty;
            LoadFolderName = string.Empty;
        }

        /// <summary>
        /// Set the load type to load the game state.
        /// <br>The game state and player data will be loaded from a saved game.</br>
        /// </summary>
        /// <param name="sceneName">The name of the scene to be loaded.</param>
        /// <param name="folderName">The name of the saved game folder.</param>
        public static void SetLoadGameState(string sceneName, string folderName)
        {
            GameLoadType = LoadType.LoadGameState;
            LoadSceneName = sceneName;
            LoadFolderName = folderName;
        }

        /// <summary>
        /// Set the load type to load the last game state of the world.
        /// <br>The game will be loaded from the last save of the scene. The player data will be transferred.</br>
        /// </summary>
        /// <param name="sceneName">The name of the scene to be loaded.</param>
        public static void SetLoadWorldState(string sceneName)
        {
            GameLoadType = LoadType.LoadWorldState;
            LoadSceneName = sceneName;
            LoadFolderName = string.Empty;
        }

        /// <summary>
        /// Set the load type to load the last game state of the current world.
        /// <br>The game will be loaded from the last save of the current scene. The player data will be loaded.</br>
        /// </summary>
        public static void SetLoadWorldLast()
        {
            GameLoadType = LoadType.LoadWorldLast;
            LoadSceneName = SceneManager.GetActiveScene().name;
            LoadFolderName = string.Empty;
        }

        /// <summary>
        /// Set the load type to load the player data only.
        /// </summary>
        /// <param name="sceneName">The name of the scene to be loaded.</param>
        public static void SetLoadPlayerData(string sceneName)
        {
            GameLoadType = LoadType.LoadPlayer;
            LoadSceneName = sceneName;
            LoadFolderName = string.Empty;
        }

        /// <summary>
        /// Save player data only.
        /// <br>The world state will not be saved.</br>
        /// </summary>
        public static void SavePlayer()
        {
            Instance.SetStaticPlayerData();
        }

        /// <summary>
        /// Save game state normally. 
        /// <br>The world state and player data will be saved.</br>
        /// </summary>
        public static void SaveGame(bool autosave)
        {
            Instance.GameSaveType = autosave ? SaveType.Autosave : SaveType.Normal;
            Instance.PrepareAndSaveGameState();
        }

        /// <summary>
        /// Save game state normally. 
        /// <br>The world state and player data will be saved.</br>
        /// </summary>
        /// <param name="onSaved">Event when the game is successfully saved.</param>
        public static void SaveGame(Action onSaved)
        {
            Instance.GameSaveType = SaveType.Normal;
            Instance.PrepareAndSaveGameState();
            Instance.OnWaitForSave += onSaved;
        }

        /// <summary>
        /// Save game state with custom player position and rotation. 
        /// <br>The world state and player data will be saved.</br>
        /// </summary>
        /// <remarks>If you need to have a different player position and rotation when returning to the previous scene.</remarks>
        /// <param name="onSaved">Event when the game is successfully saved.</param>
        public static void SaveGame(Vector3 position, Vector2 rotation, Action onSaved)
        {
            Instance.GameSaveType = SaveType.NextScene;
            Instance.CustomSavePosition = position;
            Instance.CustomSaveRotation = rotation;
            Instance.PrepareAndSaveGameState();
            Instance.OnWaitForSave += onSaved;
        }

        /// <summary>
        /// Instantiate Runtime Saveable.
        /// </summary>
        /// <remarks>
        /// The object is instantiated and added to the list of saveable objects so that it can be saved and loaded later. The object must be stored in the ObjectReferences list.
        /// </remarks>
        public static GameObject InstantiateSaveable(ObjectReference reference, Vector3 position, Vector3 rotation, string name = null)
        {
            GameObject instantiate = Instantiate(reference.Object, position, Quaternion.Euler(rotation));
            instantiate.name = name ?? reference.Object.name;
            Instance.AddRuntimeSaveable(instantiate, reference.GUID);
            return instantiate;
        }

        /// <summary>
        /// Instantiate Runtime Saveable.
        /// </summary>
        /// <remarks>
        /// The object is instantiated and added to the list of saveable objects so that it can be saved and loaded later. The object must be stored in the ObjectReferences list.
        /// </remarks>
        public static GameObject InstantiateSaveable(string referenceGUID, Vector3 position, Vector3 rotation, string name = null)
        {
            var reference = Instance.ObjectReferences.GetObjectReference(referenceGUID);
            if (reference.HasValue)
            {
                GameObject instantiate = Instantiate(reference?.Object, position, Quaternion.Euler(rotation));
                instantiate.name = name ?? reference?.Object.name;
                Instance.AddRuntimeSaveable(instantiate, referenceGUID);
                return instantiate;
            }

            return null;
        }

        /// <summary>
        /// Remove Runtime Saveable.
        /// </summary>
        public static void RemoveSaveable(GameObject obj)
        {
            Instance.runtimeSaveables.RemoveAll(x => x.InstantiatedObject == obj);
        }

        /// <summary>
        /// Set time played timer active state.
        /// </summary>
        public static void SetTimePlayedTimer(bool state)
        {
            Instance.timerActive = state;
        }
        #endregion

        #region SAVING GAME STATE
        IEnumerator ShowSavingIcon()
        {
            if (Instance.SavingIcon == null)
                yield return null;

            // show saving icon
            if (savingSpritesheet != null && !savingSpritesheet.PlayOnStart)
                savingSpritesheet.SetAnimationStatus(true);

            CanvasGroupFader.StartFadeInstance(Instance.SavingIcon, true, 3f);

            yield return new WaitForSeconds(2f);
            yield return new WaitUntil(() => isSaved);
            yield return CanvasGroupFader.StartFade(Instance.SavingIcon, false, 3f);

            // hide saving icon
            if (savingSpritesheet != null)
                savingSpritesheet.SetAnimationStatus(false);

            isSaved = false;
        }

        private async void PrepareAndSaveGameState()
        {
            // show saving icon
            isSaved = false;
            StopCoroutine(ShowSavingIcon());
            StartCoroutine(ShowSavingIcon());

            // create player state and data buffers
            StorableCollection worldStatePlayerData = new();

            // store player position and rotation
            if (SerializationAsset.PreviousScenePersistency && GameSaveType == SaveType.NextScene)
            {
                worldStatePlayerData.Add("position", CustomSavePosition.ToSaveable());
                worldStatePlayerData.Add("rotation", CustomSaveRotation.ToSaveable());
            }
            else
            {
                var (position, rotation) = playerPresence.GetPlayerTransform();
                worldStatePlayerData.Add("position", position.ToSaveable());
                worldStatePlayerData.Add("rotation", rotation.ToSaveable());
            }

            // add player data to player data buffer
            StorableCollection globalPlayerData = GetPlayerDataBuffer();
            StorableCollection localPlayerData = playerPresence.PlayerManager.OnCustomSave();
            StorableCollection playerData = new();

            playerData.Add("localData", localPlayerData);
            worldStatePlayerData.Add("localData", localPlayerData);

            playerData.Add("globalData", globalPlayerData);
            worldStatePlayerData.Add("globalData", globalPlayerData);

            // create saveable buffer with world data
            string saveId = GameTools.GetGuid();
            StorableCollection saveInfoData = new()
            {
                { "id", saveId },
                { "scene", currentScene },
                { "dateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                { "timePlayed", TimePlayed },
                { "saveType", (int)GameSaveType },
                { "data", "" },
                { "thumbnail", "" }
            };

            // add player data and world state data to saveables buffer
            StorableCollection saveablesBuffer = new()
            {
                { "id", saveId },
                { "playerData", worldStatePlayerData },
                { "worldState", GetWorldStateBuffer() }
            };

            // store player data to static variable
            PlayerData = JObject.FromObject(playerData);

            // get save folder name
            GetSaveFolder(out string saveFolderName);
            string saveFolderPath = Path.Combine(SavedGamePath, saveFolderName);

            // create save folder
            if (!Directory.Exists(saveFolderPath))
                Directory.CreateDirectory(saveFolderPath);

            // if thumbnail creation is enabled, take a screenshot and save it to the thumbnail file
            if (SerializationAsset.CreateThumbnails)
            {
                string thumbnailName = SerializationAsset.SaveThumbnailName + ".png";
                string thumbnailPath = Path.Combine(saveFolderPath, thumbnailName);
                saveInfoData["thumbnail"] = thumbnailName;
                CreateThumbnail(thumbnailPath);
            }

            // serialize save info and world state
            await SerializeGameState(saveInfoData, saveablesBuffer, saveFolderName, saveFolderPath);
        }

        private StorableCollection GetPlayerDataBuffer()
        {
            // Generate Player Data Buffer: Inventory, Objectives, Other stuff...
            StorableCollection buffer = new StorableCollection();

            buffer.Add("inventory", (inventory as ISaveableCustom).OnCustomSave());
            buffer.Add("objectives", (objectiveManager as ISaveableCustom).OnCustomSave());

            return buffer;
        }

        private StorableCollection GetWorldStateBuffer()
        {
            // Generate World State Buffer: World Saveables, Runtime Saveables
            StorableCollection saveablesBuffer = new StorableCollection();
            StorableCollection worldBuffer = new StorableCollection();
            StorableCollection runtimeBuffer = new StorableCollection();

            foreach (var saveable in worldSaveables)
            {
                if (saveable.Instance == null || string.IsNullOrEmpty(saveable.Token))
                    continue;

                var saveableData = ((ISaveable)saveable.Instance).OnSave();
                if(saveableData != null) worldBuffer.Add(saveable.Token, saveableData);
            }

            foreach (var saveable in runtimeSaveables)
            {
                if (saveable.InstantiatedObject == null || string.IsNullOrEmpty(saveable.TokenGUID))
                    continue;

                StorableCollection instantiateSaveables = new StorableCollection();
                foreach (var saveablePair in saveable.SaveablePairs)
                {
                    if (saveablePair.Instance == null || string.IsNullOrEmpty(saveablePair.Token))
                        continue;

                    var saveableData = ((ISaveable)saveablePair.Instance).OnSave();
                    if (saveableData != null)
                        instantiateSaveables.Add(saveablePair.Token, saveableData);
                }

                runtimeBuffer.Add(saveable.TokenGUID, instantiateSaveables);
            }

            saveablesBuffer.Add("worldSaveables", worldBuffer);
            saveablesBuffer.Add("runtimeSaveables", runtimeBuffer);
            return saveablesBuffer;
        }

        private void SetStaticPlayerData()
        {
            StorableCollection globalPlayerData = GetPlayerDataBuffer();
            StorableCollection localPlayerData = playerPresence.PlayerManager.OnCustomSave();
            StorableCollection playerDataBuffer = new StorableCollection
            {
                { "localData", localPlayerData },
                { "globalData", globalPlayerData }
            };

            JObject playerData = JObject.FromObject(playerDataBuffer);
            PlayerData = playerData;
        }

        private void GetSaveFolder(out string saveFolderName)
        {
            saveFolderName = SerializationAsset.SaveFolderPrefix;

            if (!SerializationAsset.SingleSave)
            {
                string[] directories = Directory.GetDirectories(SavedGamePath, $"{saveFolderName}*");
                saveFolderName += directories.Length.ToString("D3");
            }
            // if single save and use of scene names is enabled, the scene name is used as the save name
            else if (SerializationAsset.UseSceneNames)
            {
                saveFolderName += currentScene.Replace(" ", "");
            }
            // if single save and previous scene persistency is enabled
            else if (SerializationAsset.PreviousScenePersistency)
            {
                if (LastSceneSaves == null)
                {
                    // set save name to save prefix + 000
                    saveFolderName += "000";
                }
                else if (LastSceneSaves.ContainsKey(currentScene))
                {
                    // set save name to last scene save name
                    saveFolderName = LastSceneSaves[currentScene];
                }
                else
                {
                    // set save name to save prefix + count of saves
                    saveFolderName += LastSceneSaves.Count().ToString("D3");
                }
            }
        }

        private async Task SerializeGameState(StorableCollection saveInfo, StorableCollection worldState, string folderName, string folderPath)
        {
            string saveInfoFilename = SerializationAsset.SaveInfoName + SerializationAsset.SaveExtension;
            string saveDataFilename = SerializationAsset.SaveDataName + SerializationAsset.SaveExtension;
            saveInfo["data"] = saveDataFilename;

            // if previous scene persistency is enabled, store or update last scene save
            if (SerializationAsset.PreviousScenePersistency)
            {
                // if last scene saves dictionary is null, create a new instance
                if (LastSceneSaves == null) await LoadLastSceneSaves();

                // change the name of the last saved scene to a new one
                if (LastSceneSaves.ContainsKey(currentScene))
                {
                    LastSceneSaves[currentScene] = folderName;
                }
                else
                {
                    LastSceneSaves.Add(currentScene, folderName);
                }
            }

            // serialize save info to file
            string saveInfoPath = Path.Combine(folderPath, saveInfoFilename);
            await SerializeData(saveInfo, saveInfoPath);

            // serialize save data to file
            string saveDataPath = Path.Combine(folderPath, saveDataFilename);
            await SerializeData(worldState, saveDataPath);

            // show debug log
            Log($"The game state has been saved to the '{folderName}' folder.");

            // invoke game saved events
            OnGameSaved?.Invoke(folderName);
            OnWaitForSave?.Invoke();
            isSaved = true;
        }
        #endregion

        #region LOAD GAME STATE
        /// <summary>
        /// Try to Deserialize and Validate Game State
        /// </summary>
        public static async Task TryDeserializeGameStateAsync(string folderName)
        {
            // get saves path
            string savesPath = SerializationAsset.GetSavesPath();
            string saveFolderPath = Path.Combine(savesPath, folderName);

            // check if directory exists
            if (!Directory.Exists(saveFolderPath)) return;

            // deserialize saved game info
            var saveInfo = await SaveGameReader.ReadSave(folderName);

            // get data path and check if the file exists
            string dataFilepath = Path.Combine(saveFolderPath, saveInfo.Dataname);
            if (!File.Exists(dataFilepath)) throw new FileNotFoundException("Serialized file with saved data could not be found!");

            // deserialize game data state
            string gameStateJson = await DeserializeData(dataFilepath);
            JObject gameState = JObject.Parse(gameStateJson);

            // validate id of the save info and data
            if (saveInfo.Id != (string)gameState["id"])
                throw new DataException("Saved Info and the Saved Data do not match!");

            // assign time played
            TimePlayed = (float)saveInfo.TimePlayed.TotalSeconds;

            // assign deserialized game state
            GameState = gameState;
        }

        /// <summary>
        /// Try to Deserialize Last Scene Saves
        /// </summary>
        public static async Task LoadLastSceneSaves()
        {
            LastSceneSaves = new Dictionary<string, string>();

            SavedGameInfo[] savedGames = (await SaveGameReader.ReadSavesMeta())
                .GroupBy(x => x.Scene)
                .Select(s => s.OrderByDescending(x => x.TimeSaved).FirstOrDefault())
                .ToArray();

            foreach (var save in savedGames)
            {
                LastSceneSaves.Add(save.Scene, save.Foldername);
            }
        }

        /// <summary>
        /// Remove all saved games.
        /// </summary>
        public static async Task RemoveAllSaves()
        {
            LastSceneSaves?.Clear();
            await SaveGameReader.RemoveAllSaves();
        }

        private void LoadGameState(JObject gameState)
        {
            if (gameState != null)
            {
                // load world state
                if (gameState.ContainsKey("worldState"))
                {
                    JToken worldState = gameState["worldState"];
                    LoadSaveables(worldState);
                }

                // load player data
                if (gameState.ContainsKey("playerData"))
                {
                    // parse player data from game state
                    JObject playerData = JObject.FromObject(gameState["playerData"]);

                    // set player position and rotation from game state
                    Vector3 playerPos = playerData["position"].ToObject<Vector3>();
                    Vector2 playerRot = playerData["rotation"].ToObject<Vector2>();
                    playerPresence.SetPlayerTransform(playerPos, playerRot);

                    // define loading states
                    bool loadGameState = GameLoadType == LoadType.LoadGameState;
                    bool loadWorldState = GameLoadType == LoadType.LoadWorldState;
                    bool loadLastWorld = GameLoadType == LoadType.LoadWorldLast;

                    if (loadGameState || loadLastWorld || PlayerData == null)
                    {
                        // load player data from game state
                        LoadPlayerData(playerData);
                    }
                    else if(loadWorldState && PlayerData != null)
                    {
                        // load static player data
                        LoadPlayerData(PlayerData);
                    }
                }

                // notify that loading is finished
                syncLoadingDone = true;
            }
        }

        private void LoadSaveables(JToken worldState)
        {
            bool isTokenError = false;
            JToken worldSaveablesData = worldState["worldSaveables"];
            JToken runtimeSaveablesData = worldState["runtimeSaveables"];

            // iterate every world saveable
            foreach (var saveable in worldSaveables)
            {
                if (saveable.Instance == null || string.IsNullOrEmpty(saveable.Token))
                    continue;

                JToken token = worldSaveablesData[saveable.Token];
                if (token == null)
                {
                    Debug.LogError($"Could not find saveable with token '{saveable.Token}'.");
                    isTokenError = true;
                    continue;
                }
                ((ISaveable)saveable.Instance).OnLoad(token);
            }

            if (isTokenError)
            {
                Debug.LogError("[Token Error] Try to save your scene before loading game.");
                return;
            }

            // iterate every runtime saveable
            foreach (JProperty saveable in runtimeSaveablesData.Cast<JProperty>())
            {
                string tokenGUID = saveable.Name.Split('.')[0];
                var reference = ObjectReferences.GetObjectReference(tokenGUID);

                if (reference.HasValue)
                {
                    // instantiate saved object
                    GameObject instantiate = Instantiate(reference?.Object, Vector3.zero, Quaternion.identity);
                    instantiate.name = "Instance_" + reference?.Object.name;

                    // get saveables from instantiated object
                    SaveablePair[] saveablePairs = FindSaveablesInChildren(instantiate);

                    // add instantiated object to runtime saveables again
                    AddRuntimeSaveable(instantiate, tokenGUID, saveablePairs);

                    // iterate every saveable
                    foreach (JProperty saveableToken in saveable.Value.Cast<JProperty>())
                    {
                        // get saveable uniqueID and token
                        string uniqueID = saveableToken.Name.Split(TOKEN_SEPARATOR)[1];
                        JToken token = saveableToken.Value;
                        bool isUIDError = true;

                        // iterate every saveable pair in instantiated object
                        foreach (var saveablePair in saveablePairs)
                        {
                            // check if saveable uid is equal with uid in instantiated object
                            string uID = saveablePair.Token.Split(TOKEN_SEPARATOR)[1];
                            if (uniqueID == uID)
                            {
                                // load token
                                ((ISaveable)saveablePair.Instance).OnLoad(token);
                                isUIDError = false;
                            }
                        }

                        if (isUIDError) Debug.LogError($"[UniqueID Error] Could not find script with Unique ID: {uniqueID}.");
                    }
                }
            }
        }

        private void LoadPlayerData(JObject playerData)
        {
            // get local and global player data
            JToken localData = playerData["localData"];
            JToken globalData = playerData["globalData"];

            // load player local data
            playerPresence.PlayerManager.OnCustomLoad(localData);

            // load player global data
            LoadPlayerGlobalData(globalData);
        }

        private void LoadPlayerGlobalData(JToken globalData)
        {
            // load inventory
            JToken inventoryData = globalData["inventory"];
            (inventory as ISaveableCustom).OnCustomLoad(inventoryData);

            // load objectives
            JToken objectivesData = globalData["objectives"];
            (objectiveManager as ISaveableCustom).OnCustomLoad(objectivesData);

        }
        #endregion

        private void CreateThumbnail(string path)
        {
            ScreenshotFeature screenshot = ScreenshotFeature.Instance;
            screenshot.SaveScreenshotToDisk(path);
        }

        private async Task SerializeData(StorableCollection buffer, string path)
        {
            string json = JsonConvert.SerializeObject(buffer, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            await SerializableEncryptor.Encrypt(SerializationAsset, path, json);
        }

        private static async Task<string> DeserializeData(string path)
        {
            return await SerializableEncryptor.Decrypt(SerializationAsset, path);
        }

        private GameObject AddRuntimeSaveable(GameObject instantiate, string guid)
        {
            SaveablePair[] saveablePairs = FindSaveablesInChildren(instantiate);
            return AddRuntimeSaveable(instantiate, guid, saveablePairs);
        }

        private GameObject AddRuntimeSaveable(GameObject instantiate, string guid, SaveablePair[] saveablePairs)
        {
            int count = runtimeSaveables.Count(x => x.TokenGUID.Contains(guid));
            string newGuid = guid + $".id[{count}]";

            runtimeSaveables.Add(new RuntimeSaveable()
            {
                TokenGUID = newGuid,
                InstantiatedObject = instantiate,
                SaveablePairs = saveablePairs
            });

            return instantiate;
        }

        private SaveablePair[] FindSaveablesInChildren(GameObject root)
        {
            return (from mono in root.GetComponentsInChildren<MonoBehaviour>(true)
                    let type = mono.GetType()
                    where typeof(IRuntimeSaveable).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract
                    let saveable = mono as IRuntimeSaveable
                    let token = $"{type.Name}{TOKEN_SEPARATOR}{saveable.UniqueID}"
                    select new SaveablePair(token, mono)).ToArray();
        }
    }
}