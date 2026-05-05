using System.IO;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

namespace UHFPS.Scriptable
{
    [CreateAssetMenu(fileName = "SerializationAsset", menuName = "UHFPS/Serialization Asset")]
    public class SerializationAsset : ScriptableObject
    {
        public string LevelManagerScene = "LevelManager";
        public string MainMenuScene = "MainMenu";

        public string DataPath = "Data";
        public string SavesPath = "SavedGame";
        public string ConfigPath = "Config";

        public string InputsFilename = "Inputs";
        public string OptionsFilename = "GameOptions";

        public string SaveFolderPrefix = "Save_";
        public string SaveInfoName = "Save";
        public string SaveDataName = "Data";
        public string SaveThumbnailName = "Thumbnail";
        public string SaveExtension = ".bin";

        public string EncryptionKey;
        public bool EncryptSaves;
        public bool CreateThumbnails;
        public bool SingleSave;
        public bool UseSceneNames;
        public bool PreviousScenePersistency;

        public string GetSavesPath()
        {
            return Path.Combine(Application.dataPath, DataPath, SavesPath).Replace('\\', '/');
        }

        public string GetConfigPath()
        {
            return Path.Combine(Application.dataPath, DataPath, ConfigPath).Replace('\\', '/');
        }

        public void EncryptKey()
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] input = Encoding.ASCII.GetBytes(EncryptionKey);
                byte[] hash = md5.ComputeHash(input);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }

                EncryptionKey = sb.ToString();
            }
        }
    }
}