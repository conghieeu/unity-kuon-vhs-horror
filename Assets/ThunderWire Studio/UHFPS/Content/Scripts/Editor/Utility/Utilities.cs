using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using Object = UnityEngine.Object;

namespace UHFPS.Editors
{
    public class Utilities 
    {
        public const string ROOT_PATH = "UHFPS";
        public const string TOOLS_PATH = "Tools/" + ROOT_PATH;
        public const string GAMEMANAGER_PATH = "Setup/GAMEMANAGER";
        public const string PLAYER_PATH = "Setup/HEROPLAYER";

        [MenuItem(TOOLS_PATH + "/Setup/Add HEROPLAYER")]
        static void SetupScene()
        {
            // find GameManager and Player in scene
            GameObject _gameManager = GameObject.Find(GAMEMANAGER_PATH.Split("/").Last());
            GameObject _player = GameObject.Find(PLAYER_PATH.Split("/").Last());

            if(_gameManager || _player)
            {
                Debug.LogError("GameManager or Player already exists on the scene! To add Player and GameManager to the scene again, use Reload Setup.");
                return;
            }

            // load GameManager and Player prefab from Resources
            GameObject gameManagerPrefab = Resources.Load<GameObject>(GAMEMANAGER_PATH);
            GameObject playerPrefab = Resources.Load<GameObject>(PLAYER_PATH);

            // add GameManager and Player to scene
            GameObject gameManager = PrefabUtility.InstantiatePrefab(gameManagerPrefab) as GameObject;
            GameObject player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;

            // unpack prefab
            PrefabUtility.UnpackPrefabInstance(gameManager, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            PrefabUtility.UnpackPrefabInstance(player, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

            // get required components
            Camera mainCameraBrain = gameManager.GetComponentInChildren<Camera>();
            PlayerPresenceManager playerPresence = gameManager.GetComponent<PlayerPresenceManager>();
            PlayerManager playerManager = player.GetComponent<PlayerManager>();

            // assign missing references
            player.transform.position = new Vector3(0, 0, 0);
            playerManager.MainCamera = mainCameraBrain;
            playerPresence.Player = player;
        }

        [MenuItem(TOOLS_PATH + "/Setup/Reload Setup")]
        static void ReloadSetup()
        {
            // find GameManager and Player in scene
            GameObject gameManager = GameObject.Find(GAMEMANAGER_PATH.Split("/").Last());
            GameObject player = GameObject.Find(PLAYER_PATH.Split("/").Last());

            Vector3 lastPlayerPos = Vector3.zero;
            Quaternion lastPlayerRot = Quaternion.identity;

            // get last player position, rotation and destroy player
            if (player != null)
            {
                player.transform.GetPositionAndRotation(out lastPlayerPos, out lastPlayerRot);
                Object.DestroyImmediate(player);
            }

            // destroy GameManager
            if (gameManager != null) Object.DestroyImmediate(gameManager);

            // load GameManager and Player prefab from Resources
            GameObject gameManagerPrefab = Resources.Load<GameObject>(GAMEMANAGER_PATH);
            GameObject playerPrefab = Resources.Load<GameObject>(PLAYER_PATH);

            // add GameManager and Player to scene
            gameManager = PrefabUtility.InstantiatePrefab(gameManagerPrefab) as GameObject;
            player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;

            // unpack prefab
            PrefabUtility.UnpackPrefabInstance(gameManager, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            PrefabUtility.UnpackPrefabInstance(player, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

            // get required components
            Camera mainCameraBrain = gameManager.GetComponentInChildren<Camera>();
            PlayerPresenceManager playerPresence = gameManager.GetComponent<PlayerPresenceManager>();
            PlayerManager playerManager = player.GetComponent<PlayerManager>();

            // assign missing references
            player.transform.SetLocalPositionAndRotation(lastPlayerPos, lastPlayerRot);
            playerManager.MainCamera = mainCameraBrain;
            playerPresence.Player = player;
        }

        [MenuItem(TOOLS_PATH + "/Setup/Fix Missing")]
        static void FixSetup()
        {
            // find GameManager and Player in scene
            GameObject gameManager = GameObject.Find(GAMEMANAGER_PATH.Split("/").Last());
            GameObject player = GameObject.Find(PLAYER_PATH.Split("/").Last());
            bool playerMissing = player == null;

            // check if Game Manager is missing
            if (gameManager == null)
            {
                GameObject gameManagerPrefab = Resources.Load<GameObject>(GAMEMANAGER_PATH);
                gameManager = PrefabUtility.InstantiatePrefab(gameManagerPrefab) as GameObject;
                PrefabUtility.UnpackPrefabInstance(gameManager, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }

            // check if Player is missing
            if(player == null)
            {
                GameObject playerPrefab = Resources.Load<GameObject>(PLAYER_PATH);
                player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
                PrefabUtility.UnpackPrefabInstance(player, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }

            // get required components
            Camera mainCameraBrain = gameManager.GetComponentInChildren<Camera>();
            PlayerPresenceManager playerPresence = gameManager.GetComponent<PlayerPresenceManager>();
            PlayerManager playerManager = player.GetComponent<PlayerManager>();

            // assign missing references
            if(playerMissing) 
                player.transform.position = new Vector3(0, 0, 0);

            playerManager.MainCamera = mainCameraBrain;
            playerPresence.Player = player;
        }

        [MenuItem("Tools/UHFPS/Utilities/Select GameObjects With Missing Scripts")]
        static void SelectGameObjects()
        {
            //Get the current scene and all top-level GameObjects in the scene hierarchy
            Scene currentScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = currentScene.GetRootGameObjects();

            List<Object> objectsWithDeadLinks = new List<Object>();
            foreach (GameObject g in rootObjects)
            {
                //Get all components on the GameObject, then loop through them 
                Component[] components = g.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    Component currentComponent = components[i];

                    //If the component is null, that means it's a missing script!
                    if (currentComponent == null)
                    {
                        //Add the sinner to our naughty-list
                        objectsWithDeadLinks.Add(g);
                        Selection.activeGameObject = g;
                        Debug.Log(g + " has a missing script!");
                        break;
                    }
                }
            }
            if (objectsWithDeadLinks.Count > 0)
            {
                //Set the selection in the editor
                Selection.objects = objectsWithDeadLinks.ToArray();
            }
            else
            {
                Debug.Log("No GameObjects in '" + currentScene.name + "' have missing scripts! Yay!");
            }
        }

        [MenuItem("Tools/UHFPS/Utilities/Find Missing Scripts In Project")]
        static void FindMissingScriptsInProjectMenuItem()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                foreach (Component component in prefab.GetComponentsInChildren<Component>())
                {
                    if (component == null)
                    {
                        Debug.Log("Prefab found with missing script " + assetPath, prefab);
                        break;
                    }
                }
            }
        }

        [MenuItem("Tools/UHFPS/Utilities/Assign GString Normal Text")]
        static void AssignGStringNormalText()
        {
            if (!GameLocalization.HasReference)
                throw new NullReferenceException("GameLocalization is not found in scene!");

            GameLocalization gloc = GameLocalization.Instance;
            string[] guids = AssetDatabase.FindAssets("t:Prefab");

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                // Instantiate a temporary prefab instance in the scene
                GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if (prefabInstance == null) continue;

                // Get all MonoBehaviour scripts attached to the prefab instance and its children
                MonoBehaviour[] components = prefabInstance.GetComponentsInChildren<MonoBehaviour>(true);
                bool updated = false;

                foreach (MonoBehaviour component in components)
                {
                    if (component == null)
                        continue;

                    Type type = component.GetType();
                    FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

                    Type gStringType = typeof(GString);
                    foreach (FieldInfo field in fields)
                    {
                        if (field.FieldType == gStringType)
                        {
                            GString gString = (GString)field.GetValue(component);
                            if (string.IsNullOrEmpty(gString.GlocText))
                                continue;

                            if (gloc.LanguageDict.TryGetValue(gString.GlocText, out string text))
                            {
                                FieldInfo textField = gStringType.GetField("NormalText", BindingFlags.Public | BindingFlags.Instance);
                                if (textField != null)
                                {
                                    // Update the NormalText property value
                                    textField.SetValue(gString, text);
                                    updated = true;
                                }
                            }
                        }
                    }
                }

                // Apply the changes to the prefab and destroy the temporary instance
                if (updated)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabInstance, assetPath);
                    Debug.Log($"Updated GString in '{prefab.name}' ({assetPath})");
                }

                Object.DestroyImmediate(prefabInstance);
            }
        }
    }
}