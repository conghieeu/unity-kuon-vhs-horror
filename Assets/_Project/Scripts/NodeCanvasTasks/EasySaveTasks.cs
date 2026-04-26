using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.EasySave
{
    [Category("Extensions/EasySave/Utility")]
    public class ES3FileExists : ConditionTask
    {
        public BBParameter<string> path = "SaveFile.es3";
        protected override bool OnCheck() {
            return ES3.FileExists(path.value);
        }
    }

    [Category("Extensions/EasySave/Utility")]
    public class ES3KeyExists : ConditionTask
    {
        public BBParameter<string> key;
        protected override bool OnCheck() {
            return ES3.KeyExists(key.value);
        }
    }

    [Category("Extensions/EasySave/Utility")]
    public class ES3DeleteFile : ActionTask
    {
        public BBParameter<string> path = "SaveFile.es3";
        protected override void OnExecute() {
            ES3.DeleteFile(path.value);
            EndAction();
        }
    }

    [Category("Extensions/EasySave/Utility")]
    public class ES3DeleteKey : ActionTask
    {
        public BBParameter<string> key;
        protected override void OnExecute() {
            ES3.DeleteKey(key.value);
            EndAction();
        }
    }

    [Category("Extensions/EasySave/Utility")]
    public class ES3RenameFile : ActionTask
    {
        public BBParameter<string> path = "SaveFile.es3";
        public BBParameter<string> newPath;
        protected override void OnExecute() {
            ES3.RenameFile(path.value, newPath.value);
            EndAction();
        }
    }

    ///----------------------------------------------------------------------------------------------

#if !DISABLE_WEB

    public abstract class ES3CloudAction : ActionTask
    {
        public BBParameter<string> url = "http://www.myserver.com/ES3Cloud.php";
        public BBParameter<string> apiKey;
        public BBParameter<string> path = "SaveFile.es3";
        protected ES3Cloud cloud;
        protected override void OnUpdate() { if ( cloud.isDone ) { EndAction(true); } }
    }

    [Category("Extensions/EasySave/Web")]
    public class ES3CloudSync : ES3CloudAction
    {
        protected override void OnExecute() {
            cloud = new ES3Cloud(url.value, apiKey.value);
            StartCoroutine(cloud.Sync(path.value));
            if ( cloud.isError ) {
                Error(cloud.errorCode + ":" + cloud.error);
                EndAction(false);
            }
        }
    }

    [Category("Extensions/EasySave/Web")]
    public class ES3CloudUpload : ES3CloudAction
    {
        public BBParameter<string> user;
        public BBParameter<string> password;
        protected override void OnExecute() {
            cloud = new ES3Cloud(url.value, apiKey.value);
            StartCoroutine(cloud.UploadFile(path.value, user.value, password.value));
            if ( cloud.isError ) {
                Error(cloud.errorCode + ":" + cloud.error);
                EndAction(false);
            }
        }
    }

    [Category("Extensions/EasySave/Web")]
    public class ES3CloudDownload : ES3CloudAction
    {
        public BBParameter<string> user;
        public BBParameter<string> password;
        protected override void OnExecute() {
            cloud = new ES3Cloud(url.value, apiKey.value);
            StartCoroutine(cloud.DownloadFile(path.value, user.value, password.value));
            if ( cloud.isError ) {
                Error(cloud.errorCode + ":" + cloud.error);
                EndAction(false);
            }
        }
    }

    [Category("Extensions/EasySave/Web")]
    public class ES3CloudDelete : ES3CloudAction
    {
        public BBParameter<string> user;
        public BBParameter<string> password;
        protected override void OnExecute() {
            cloud = new ES3Cloud(url.value, apiKey.value);
            StartCoroutine(cloud.DeleteFile(path.value, user.value, password.value));
            if ( cloud.isError ) {
                Error(cloud.errorCode + ":" + cloud.error);
                EndAction(false);
            }
        }
    }

#endif

    ///----------------------------------------------------------------------------------------------

    [Category("Extensions/EasySave/Saving")]
    public class ES3Save<T> : ActionTask
    {
        public BBParameter<string> key;
        public BBParameter<T> data;
        public BBParameter<string> filePath = "SaveFile.es3";
        public ES3.EncryptionType encryptionType = ES3.EncryptionType.None;
        public string encryptionPassword = "password";
        public ES3.CompressionType compressionType = ES3.CompressionType.None;

        protected override void OnExecute() {
            var settings = new ES3Settings(filePath.value, encryptionType, encryptionPassword);
            settings.compressionType = compressionType;
            try {
                ES3.Save(key.value, data.value, settings);
                EndAction();
            }
            catch (System.FormatException) {
                ES3.DeleteFile(settings);
                try {
                    ES3.Save(key.value, data.value, settings);
                    EndAction();
                }
                catch (System.Exception e) {
                    Error("ES3.Save failed after deleting corrupt file: " + e.Message);
                    EndAction(false);
                }
            }
            catch (System.Exception e) {
                Error("ES3.Save failed: " + e.Message);
                EndAction(false);
            }
        }
    }

    [Category("Extensions/EasySave/Saving")]
    public class ES3SaveImage : ActionTask
    {
        public BBParameter<Texture2D> image;
        public BBParameter<string> path = "SaveFile.es3";
        protected override void OnExecute() {
            ES3.SaveImage(image.value, path.value);
            EndAction();
        }
    }

    [Category("Extensions/EasySave/Loading")]
    public class ES3Load<T> : ActionTask
    {
        public BBParameter<string> key;
        [BlackboardOnly]
        public BBParameter<T> loadedData;
        public T defaultValue;
        protected override void OnExecute() {
            loadedData.value = ES3.Load<T>(key.value, defaultValue);
            EndAction();
        }
    }

    [Category("Extensions/EasySave/Loading")]
    public class ES3LoadInto<T> : ActionTask where T : class
    {
        public BBParameter<string> key;
        [BlackboardOnly]
        public BBParameter<T> target;
        protected override void OnExecute() {
            ES3.LoadInto<T>(key.value, target.value);
            EndAction();
        }
    }

    [Category("Extensions/EasySave/Loading")]
    public class ES3LoadAudio : ActionTask
    {
        public BBParameter<string> path = "SaveFile.es3";
        public AudioType audioType;
        [BlackboardOnly]
        public BBParameter<AudioClip> loadedData;
        protected override void OnExecute() {
            loadedData.value = ES3.LoadAudio(path.value, audioType);
        }
    }

    [Category("Extensions/EasySave/Loading")]
    public class ES3LoadImage : ActionTask
    {
        public BBParameter<string> path = "SaveFile.es3";
        [BlackboardOnly]
        public BBParameter<Texture2D> loadedData;
        protected override void OnExecute() {
            loadedData.value = ES3.LoadImage(path.value);
        }
    }
}