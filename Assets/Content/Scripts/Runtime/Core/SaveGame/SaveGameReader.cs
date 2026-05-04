using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UHFPS.Scriptable;

namespace UHFPS.Runtime
{
    public struct SavedGameInfo
    {
        public string Id;
        public string Scene;
        public DateTime TimeSaved;
        public TimeSpan TimePlayed;
        public bool IsAutosave;
        public string Foldername;
        public string Dataname;
        public Texture2D Thumbnail;
    }

    public class SaveGameReader
    {
        private readonly SerializationAsset serializationAsset;

        public SaveGameReader(SerializationAsset serializationAsset)
        {
            this.serializationAsset = serializationAsset;
        }

        /// <summary>
        /// Get information about all saved games.
        /// </summary>
        public async Task<SavedGameInfo[]> ReadAllSaves()
        {
            string savesPath = serializationAsset.GetSavesPath();
            string saveFolderPrefix = serializationAsset.SaveFolderPrefix;
            string saveInfoPrefix = serializationAsset.SaveInfoName;
            string saveExtension = serializationAsset.SaveExtension;
            IList<SavedGameInfo> saveInfos = new List<SavedGameInfo>();

            if (Directory.Exists(savesPath))
            {
                string[] directories = Directory.GetDirectories(savesPath, $"{saveFolderPrefix}*");
                foreach (var directoryPath in directories)
                {
                    string saveInfoPath = Path.Combine(directoryPath, saveInfoPrefix + saveExtension);

                    // check if file exists
                    if (!File.Exists(saveInfoPath))
                        continue;

                    // decrypt saved data
                    string json = await SerializableEncryptor.Decrypt(serializationAsset, saveInfoPath);

                    // parse json from string
                    JObject saveInfo = JObject.Parse(json);
                    string thumbnailName = (string)saveInfo["thumbnail"];
                    Texture2D thumbnail = null;

                    // load thumbnail
                    if (!string.IsNullOrEmpty(thumbnailName))
                    {
                        string thumbnailPath = Path.Combine(directoryPath, thumbnailName);
                        if (File.Exists(thumbnailPath))
                        {
                            byte[] thumbnailBytes = await File.ReadAllBytesAsync(thumbnailPath);
                            thumbnail = new Texture2D(2, 2);
                            thumbnail.LoadImage(thumbnailBytes);
                        }
                    }

                    // finally add saved game info struct
                    saveInfos.Add(new SavedGameInfo()
                    {
                        Id = (string)saveInfo["id"],
                        Scene = (string)saveInfo["scene"],
                        TimeSaved = saveInfo["dateTime"].ToObject<DateTime>(),
                        TimePlayed = TimeSpan.FromSeconds((float)saveInfo["timePlayed"]),
                        IsAutosave = (int)saveInfo["saveType"] == 1,
                        Foldername = Path.GetFileName(directoryPath),
                        Dataname = (string)saveInfo["data"],
                        Thumbnail = thumbnail
                    });
                }

                // reorder save infos by time saved
                return saveInfos.OrderByDescending(x => x.TimeSaved).ToArray();
            }

            return new SavedGameInfo[0];
        }

        /// <summary>
        /// Get info about the saved game of the specified save.
        /// </summary>
        public async Task<SavedGameInfo> ReadSave(string folderName)
        {
            string savesPath = serializationAsset.GetSavesPath();
            string saveInfoPrefix = serializationAsset.SaveInfoName;
            string saveExtension = serializationAsset.SaveExtension;
            string saveFolderPath = Path.Combine(savesPath, folderName);

            if (Directory.Exists(savesPath) && Directory.Exists(saveFolderPath))
            {
                string saveInfoPath = Path.Combine(saveFolderPath, saveInfoPrefix + saveExtension);
                if (File.Exists(saveInfoPath))
                {
                    // decrypt saved data
                    string json = await SerializableEncryptor.Decrypt(serializationAsset, saveInfoPath);

                    // parse json from string
                    JObject saveInfo = JObject.Parse(json);
                    string thumbnailName = (string)saveInfo["thumbnail"];
                    Texture2D thumbnail = null;

                    // load thumbnail
                    if (!string.IsNullOrEmpty(thumbnailName))
                    {
                        string thumbnailPath = Path.Combine(saveFolderPath, thumbnailName);
                        if (File.Exists(thumbnailPath))
                        {
                            byte[] thumbnailBytes = await File.ReadAllBytesAsync(thumbnailPath);
                            thumbnail = new Texture2D(2, 2);
                            thumbnail.LoadImage(thumbnailBytes);
                        }
                    }

                    // finally add saved game info struct
                    return new SavedGameInfo()
                    {
                        Id = (string)saveInfo["id"],
                        Scene = (string)saveInfo["scene"],
                        TimeSaved = saveInfo["dateTime"].ToObject<DateTime>(),
                        TimePlayed = TimeSpan.FromSeconds((float)saveInfo["timePlayed"]),
                        IsAutosave = (int)saveInfo["saveType"] == 1,
                        Dataname = (string)saveInfo["data"],
                        Thumbnail = thumbnail
                    };
                }
            }

            return new SavedGameInfo();
        }


        /// <summary>
        /// Get meta information about all saved games.
        /// </summary>
        public async Task<SavedGameInfo[]> ReadSavesMeta()
        {
            string savesPath = serializationAsset.GetSavesPath();
            string saveFolderPrefix = serializationAsset.SaveFolderPrefix;
            string saveInfoPrefix = serializationAsset.SaveInfoName;
            string saveExtension = serializationAsset.SaveExtension;
            IList<SavedGameInfo> saveInfos = new List<SavedGameInfo>();

            if (Directory.Exists(savesPath))
            {
                string[] directories = Directory.GetDirectories(savesPath, $"{saveFolderPrefix}*");
                foreach (var directoryPath in directories)
                {
                    string saveInfoPath = Path.Combine(directoryPath, saveInfoPrefix + saveExtension);

                    // check if file exists
                    if (!File.Exists(saveInfoPath))
                        continue;

                    // decrypt saved data
                    string json = await SerializableEncryptor.Decrypt(serializationAsset, saveInfoPath);

                    // parse json from string
                    JObject saveInfo = JObject.Parse(json);

                    // finally add saved game info struct
                    saveInfos.Add(new SavedGameInfo()
                    {
                        Id = (string)saveInfo["id"],
                        Scene = (string)saveInfo["scene"],
                        TimeSaved = saveInfo["dateTime"].ToObject<DateTime>(),
                        TimePlayed = TimeSpan.FromSeconds((float)saveInfo["timePlayed"]),
                        IsAutosave = (int)saveInfo["saveType"] == 1,
                        Foldername = Path.GetFileName(directoryPath),
                        Dataname = (string)saveInfo["data"],
                    });
                }

                return saveInfos.OrderByDescending(x => x.TimeSaved).ToArray();
            }

            return new SavedGameInfo[0];
        }

        /// <summary>
        /// Remove all saved games.
        /// </summary>
        public async Task RemoveAllSaves()
        {
            string savesPath = serializationAsset.GetSavesPath();
            string saveFolderPrefix = serializationAsset.SaveFolderPrefix;

            if (Directory.Exists(savesPath))
            {
                string[] directories = Directory.GetDirectories(savesPath, $"{saveFolderPrefix}*");
                if (directories.Length > 0)
                {
                    Task[] deleteTasks = new Task[directories.Length];
                    for (int i = 0; i < directories.Length; i++)
                    {
                        string directoryPath = directories[i];
                        deleteTasks[i] = Task.Run(() => Directory.Delete(directoryPath, true));
                    }
                    await Task.WhenAll(deleteTasks);
                }
            }
        }

        /// <summary>
        /// Remove single saved game.
        /// </summary>
        public async Task RemoveSave(string folderName)
        {
            string savesPath = serializationAsset.GetSavesPath();
            string saveFolderPath = Path.Combine(savesPath, folderName);
            if (Directory.Exists(savesPath) && Directory.Exists(saveFolderPath))
            {
                await Task.Run(() => Directory.Delete(saveFolderPath, true));
            }
        }
    }
}