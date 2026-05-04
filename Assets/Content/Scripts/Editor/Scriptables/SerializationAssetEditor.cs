using UnityEngine;
using UnityEditor;
using UHFPS.Scriptable;
using UHFPS.Input;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(SerializationAsset))]
    public class SerializationAssetEditor : Editor
    {
        SerializationAsset Asset;
        Material UIMaterial;

        SerializedProperty LevelManagerScene;
        SerializedProperty MainMenuScene;

        SerializedProperty DataPath;
        SerializedProperty SavesPath;
        SerializedProperty ConfigPath;

        SerializedProperty InputsFilename;
        SerializedProperty OptionsFilename;

        SerializedProperty SaveFolderPrefix;
        SerializedProperty SaveInfoName;
        SerializedProperty SaveDataName;
        SerializedProperty SaveThumbnailName;
        SerializedProperty SaveExtension;

        SerializedProperty EncryptionKey;
        SerializedProperty EncryptSaves;
        SerializedProperty CreateThumbnails;
        SerializedProperty SingleSave;
        SerializedProperty UseSceneNames;
        SerializedProperty PreviousScenePersistency;

        private void OnEnable()
        {
            Asset = (SerializationAsset)target;
            UIMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

            LevelManagerScene = serializedObject.FindProperty("LevelManagerScene");
            MainMenuScene = serializedObject.FindProperty("MainMenuScene");

            DataPath = serializedObject.FindProperty("DataPath");
            SavesPath = serializedObject.FindProperty("SavesPath");
            ConfigPath = serializedObject.FindProperty("ConfigPath");

            InputsFilename = serializedObject.FindProperty("InputsFilename");
            OptionsFilename = serializedObject.FindProperty("OptionsFilename");

            SaveFolderPrefix = serializedObject.FindProperty("SaveFolderPrefix");
            SaveInfoName = serializedObject.FindProperty("SaveInfoName");
            SaveDataName = serializedObject.FindProperty("SaveDataName");
            SaveThumbnailName = serializedObject.FindProperty("SaveThumbnailName");
            SaveExtension = serializedObject.FindProperty("SaveExtension");

            EncryptionKey = serializedObject.FindProperty("EncryptionKey");
            EncryptSaves = serializedObject.FindProperty("EncryptSaves");
            CreateThumbnails = serializedObject.FindProperty("CreateThumbnails");
            SingleSave = serializedObject.FindProperty("SingleSave");
            UseSceneNames = serializedObject.FindProperty("UseSceneNames");
            PreviousScenePersistency = serializedObject.FindProperty("PreviousScenePersistency");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorDrawing.DrawInspectorHeader(new GUIContent("Serialization Asset"));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Scenes", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(LevelManagerScene);
            EditorGUILayout.PropertyField(MainMenuScene);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Paths", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(DataPath);
            EditorGUILayout.PropertyField(SavesPath);
            EditorGUILayout.PropertyField(ConfigPath);
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Filenames", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(InputsFilename);
            EditorGUILayout.PropertyField(OptionsFilename);
            EditorGUILayout.Space();

            string savesPath = Asset.GetSavesPath();
            EditorGUILayout.HelpBox("Saves Path: " + savesPath, MessageType.Info);
            EditorGUILayout.Space(2f);

            string configPath = Asset.GetConfigPath();
            EditorGUILayout.HelpBox("Config Path: " + configPath, MessageType.Info);
            EditorGUILayout.Space();

            using (new EditorDrawing.BorderBoxScope(new GUIContent("Save Preferences"), 18f, true))
            {
                EditorGUILayout.PropertyField(SaveFolderPrefix);
                EditorGUILayout.PropertyField(SaveInfoName);
                EditorGUILayout.PropertyField(SaveDataName);
                EditorGUILayout.PropertyField(SaveThumbnailName);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(SaveExtension);
                if (EditorGUI.EndChangeCheck())
                {
                    if (SaveExtension.stringValue.Length > 0)
                    {
                        if (SaveExtension.stringValue[0] != '.')
                            SaveExtension.stringValue = "." + SaveExtension.stringValue;
                    }
                    else
                    {
                        SaveExtension.stringValue = ".";
                    }
                }
            }

            EditorGUILayout.Space();
            using (new EditorDrawing.BorderBoxScope(new GUIContent("Save Encryption (Recommended)"), 18f, true))
            {
                Rect keyRect = EditorGUILayout.GetControlRect();
                keyRect.xMax -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(keyRect, EncryptionKey);

                Rect encryptRect = keyRect;
                encryptRect.xMin = keyRect.xMax + EditorGUIUtility.standardVerticalSpacing;
                encryptRect.xMax = encryptRect.xMin + EditorGUIUtility.singleLineHeight;

                GUIContent lockIcon = EditorGUIUtility.TrIconContent("AssemblyLock");
                using (new EditorGUI.DisabledGroupScope(string.IsNullOrEmpty(Asset.EncryptionKey)))
                {
                    if (GUI.Button(encryptRect, lockIcon, EditorStyles.iconButton))
                    {
                        Asset.EncryptKey();
                        GUI.FocusControl(null);
                        AssetDatabase.SaveAssetIfDirty(target);
                    }
                }

                EditorGUILayout.PropertyField(EncryptSaves);

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("The saved game file will be encrypted with an encryption key, so no one will be able to change the saved data.", MessageType.Info);
            }

            EditorGUILayout.Space();

            using (new EditorDrawing.BorderBoxScope(new GUIContent("Save Thumbnails")))
            {
                EditorGUILayout.PropertyField(CreateThumbnails);

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("The game will be saved along with a screenshot of the game, so you can view the saved game thumbnail in the game loading slot.", MessageType.Info);
            }

            EditorGUILayout.Space();

            using (new EditorDrawing.BorderBoxScope(new GUIContent("Single Save")))
            {
                EditorGUILayout.PropertyField(SingleSave);
                using (new EditorGUI.DisabledGroupScope(!SingleSave.boolValue))
                {
                    EditorGUILayout.PropertyField(UseSceneNames);
                }

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("The game will be saved in the same slot instead of being saved to a new file each time the game is saved.", MessageType.Info);

            }

            EditorGUILayout.Space();

            using (new EditorDrawing.BorderBoxScope(new GUIContent("Previous Scene Persistency")))
            {
                EditorGUILayout.PropertyField(PreviousScenePersistency);

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("The game will be saved with the option to return to the specified scene and load its world state. If another save will be loaded, it overwrites the player data from the saved state.", MessageType.Info);

            }

            EditorGUILayout.Space();

            using (new EditorDrawing.BorderBoxScope(new GUIContent("Serialization Structure Preview")))
            {
                string saveFolderName = SaveFolderPrefix.stringValue;
                string saveInfoName = SaveInfoName.stringValue;
                string saveDataName = SaveDataName.stringValue;
                string saveThumbnailName = SaveThumbnailName.stringValue;
                string saveExtension = SaveExtension.stringValue;

                Rect gameNameRect = EditorGUILayout.GetControlRect();
                GUIContent gameNameTitle = EditorGUIUtility.TrTextContentWithIcon(Application.productName, "Folder Icon");
                EditorGUI.LabelField(gameNameRect, gameNameTitle);

                // draw data folder
                GUIContent dataFolderTitle = EditorGUIUtility.TrTextContentWithIcon(DataPath.stringValue, "Folder Icon");
                Rect dataParentRect = DrawStructureItem(gameNameRect, dataFolderTitle);

                // draw saves folder
                GUIContent savesFolderTitle = EditorGUIUtility.TrTextContentWithIcon(SavesPath.stringValue, "Folder Icon");
                Rect savesParentRect = DrawStructureItem(dataParentRect, savesFolderTitle);

                // draw saves folder structure
                {
                    string saveFolder1 = saveFolderName;
                    string saveFolder2 = saveFolderName;

                    if (SingleSave.boolValue && UseSceneNames.boolValue)
                    {
                        saveFolder1 += "Level1";
                        saveFolder2 += "Level2";
                    }
                    else if (!SingleSave.boolValue || PreviousScenePersistency.boolValue)
                    {
                        saveFolder1 += "000";
                        saveFolder2 += "001";
                    }

                    saveThumbnailName += ".png";
                    saveInfoName += saveExtension;
                    saveDataName += saveExtension;

                    GUIContent saveFolder1Title = EditorGUIUtility.TrTextContentWithIcon(saveFolder1, "Folder Icon");
                    GUIContent saveFolder2Title = EditorGUIUtility.TrTextContentWithIcon(saveFolder2, "Folder Icon");

                    GUIContent saveInfoTitle = EditorGUIUtility.TrTextContentWithIcon(saveInfoName, "DefaultAsset Icon");
                    GUIContent saveDataTitle = EditorGUIUtility.TrTextContentWithIcon(saveDataName, "DefaultAsset Icon");
                    GUIContent saveThumbnailTitle = EditorGUIUtility.TrTextContentWithIcon(saveThumbnailName, "Texture Icon");

                    Rect save1Parent = DrawStructureItem(savesParentRect, saveFolder1Title);
                    DrawStructureItem(save1Parent, saveInfoTitle);
                    DrawStructureItem(save1Parent, saveDataTitle);
                    if(CreateThumbnails.boolValue) DrawStructureItem(save1Parent, saveThumbnailTitle);

                    if (!SingleSave.boolValue || SingleSave.boolValue && (UseSceneNames.boolValue || PreviousScenePersistency.boolValue))
                    {
                        Rect save2Parent = DrawStructureItem(savesParentRect, saveFolder2Title);
                        DrawStructureItem(save2Parent, saveInfoTitle);
                        DrawStructureItem(save2Parent, saveDataTitle);
                        if (CreateThumbnails.boolValue) DrawStructureItem(save2Parent, saveThumbnailTitle);
                    }
                }

                // draw config folder
                GUIContent configFolderTitle = EditorGUIUtility.TrTextContentWithIcon(ConfigPath.stringValue, "Folder Icon");
                Rect configParentRect = DrawStructureItem(dataParentRect, configFolderTitle);

                string inputsFilename = InputsFilename.stringValue + InputManager.EXTENSION;
                string optionsFilename = OptionsFilename.stringValue + OptionsManager.EXTENSION;

                GUIContent inputsFileTitle = EditorGUIUtility.TrTextContentWithIcon(inputsFilename, "TextAsset Icon");
                GUIContent optionsFileTitle = EditorGUIUtility.TrTextContentWithIcon(optionsFilename, "TextAsset Icon");

                DrawStructureItem(configParentRect, inputsFileTitle);
                DrawStructureItem(configParentRect, optionsFileTitle);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private Rect DrawStructureItem(Rect parentRect, GUIContent title)
        {
            Rect itemRect = EditorGUILayout.GetControlRect();
            itemRect.xMin = parentRect.xMin;

            DrawGLIndent(itemRect);
            itemRect.xMin += EditorGUIUtility.singleLineHeight + 2f;
            EditorGUI.LabelField(itemRect, title);

            return itemRect;
        }

        private void DrawGLIndent(Rect rect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                GUI.BeginClip(rect);
                {
                    GL.PushMatrix();
                    GL.LoadPixelMatrix();
                    UIMaterial.SetPass(0);
                    {
                        GL.Begin(GL.LINES);
                        {
                            GL.Vertex(new Vector2(10f, 0f));
                            GL.Vertex(new Vector2(10f, 9f));
                            GL.Vertex(new Vector2(10f, 9f));
                            GL.Vertex(new Vector2(18f, 9f));
                        }
                        GL.End();
                    }
                    GL.PopMatrix();
                }
                GUI.EndClip();
            }
        }
    }
}