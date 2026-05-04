using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(SaveGameManager))]
    public class SaveGameManagerEditor : InspectorEditor<SaveGameManager>
    {
        private string saveFolderName;
        private bool debugExpanded;

        public override void OnEnable()
        {
            base.OnEnable();
            saveFolderName = "Save";
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Save Game Manager"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                DrawPropertiesExcluding(serializedObject, "m_Script", "worldSaveables", "runtimeSaveables");

                GUIStyle headerStyle = new(EditorStyles.boldLabel)
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new(5, 0, 0, 0)
                };

                headerStyle.normal.textColor = Color.white;

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    Rect searcherHeader = EditorGUILayout.GetControlRect(false, 25f);
                    ColorUtility.TryParseHtmlString("#272727", out Color color);

                    EditorGUI.DrawRect(searcherHeader, color);
                    EditorGUI.LabelField(searcherHeader, "Saveables Searcher".ToUpper(), headerStyle);

                    using (new EditorDrawing.IconSizeScope(15))
                    {
                        EditorUtils.TrIconText($"World Saveables: {Target.worldSaveables.Count}", MessageType.Info, EditorStyles.miniBoldLabel);
                        EditorUtils.TrIconText($"Runtime Saveables: {Target.runtimeSaveables.Count}", MessageType.Info, EditorStyles.miniBoldLabel);
                    }
                    EditorGUILayout.Space();

                    using (new EditorDrawing.BackgroundColorScope("#F7E987"))
                    {
                        if (GUILayout.Button("Find Saveables", GUILayout.Height(25)))
                        {
                            System.Diagnostics.Stopwatch stopwatch = new();
                            stopwatch.Start();

                            // Find all MonoBehaviours in the scene.
                            MonoBehaviour[] monos = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                            // Get the current list of saveables.
                            var existingSaveables = Target.worldSaveables ?? null;
                            var updatedSaveables = new List<SaveGameManager.SaveablePair>();

                            foreach (var mono in monos)
                            {
                                var type = mono.GetType();
                                if (typeof(ISaveable).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                                {
                                    string token = string.Empty;

                                    // Loop through the existing saveables to match the existing token.
                                    if (existingSaveables != null)
                                    {
                                        foreach (var pair in existingSaveables)
                                        {
                                            if (pair.Instance == mono)
                                            {
                                                // Reuse the token.
                                                token = pair.Token;
                                                break;
                                            }
                                        }
                                    }

                                    // Generate a new token if the existing was not found.
                                    if (string.IsNullOrEmpty(token))
                                        token = $"{type.Name}{SaveGameManager.TOKEN_SEPARATOR}{GUID.Generate()}";

                                    updatedSaveables.Add(new SaveGameManager.SaveablePair(token, mono));
                                }
                            }

                            // Replace the stored list with the updated one.
                            Target.worldSaveables = updatedSaveables;
                            stopwatch.Stop();

                            EditorUtility.SetDirty(target);
                            Debug.Log($"<color=yellow>[Saveables Searcher]</color> Found {updatedSaveables.Count} world saveables in {stopwatch.ElapsedMilliseconds}ms. <color=red>SAVE YOUR SCENE!</color>");
                        }

                    }

                    float buttonSize = EditorGUIUtility.singleLineHeight;
                    float spacing = 2f; // Spacing between buttons

                    // Reset Button
                    Rect resetButtonRect = searcherHeader;
                    resetButtonRect.xMin = resetButtonRect.xMax - buttonSize;
                    resetButtonRect.height = buttonSize;
                    resetButtonRect.y += 4f;
                    resetButtonRect.x -= 5f;

                    GUIContent resetButtonIcon = EditorGUIUtility.TrIconContent("Refresh", "Reset Saveables");
                    if (GUI.Button(resetButtonRect, resetButtonIcon, EditorStyles.iconButton))
                    {
                        Properties["worldSaveables"].arraySize = 0;
                        Properties["runtimeSaveables"].arraySize = 0;
                        serializedObject.ApplyModifiedProperties();
                    }

                    // Saveables Explorer Button
                    Rect openExplorerRect = resetButtonRect;
                    openExplorerRect.x -= buttonSize + spacing;
                    GUIContent openExplorerIcon = EditorGUIUtility.TrIconContent("Package Manager", "Open Saveables Explorer");

                    if (GUI.Button(openExplorerRect, openExplorerIcon, EditorStyles.iconButton))
                    {
                        EditorWindow explorer = EditorWindow.GetWindow<SaveablesExplorer>(true, "Saveables Explorer", true);
                        explorer.minSize = new Vector2(600, 500);
                        ((SaveablesExplorer)explorer).Show(Target.worldSaveables, Target.runtimeSaveables);
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Functions Debug (Runtime Only)"), ref debugExpanded))
                {
                    using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
                    {
                        Rect saveGameRect = EditorGUILayout.GetControlRect();
                        Rect saveGameBtn = EditorGUI.PrefixLabel(saveGameRect, new GUIContent("Save Game"));
                        if (GUI.Button(saveGameBtn, new GUIContent("Save")))
                        {
                            SaveGameManager.SaveGame(false);
                        }

                        Rect loadGameRect = EditorGUILayout.GetControlRect();
                        Rect loadGameBtn = EditorGUI.PrefixLabel(loadGameRect, new GUIContent("Load Game"));

                        Rect loadGameText = loadGameBtn;
                        loadGameText.xMax *= 0.8f;
                        loadGameBtn.xMin = loadGameText.xMax + 2f;

                        saveFolderName = EditorGUI.TextField(loadGameText, saveFolderName);
                        if (GUI.Button(loadGameBtn, new GUIContent("Load")))
                        {
                            if (!string.IsNullOrEmpty(saveFolderName))
                            {
                                SaveGameManager.GameLoadType = SaveGameManager.LoadType.LoadGameState;
                                SaveGameManager.LoadFolderName = saveFolderName;

                                string sceneName = SceneManager.GetActiveScene().name;
                                SaveGameManager.LoadSceneName = sceneName;
                                SceneManager.LoadScene(SaveGameManager.LMS);
                            }
                        }
                    }

                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private Texture2D MakeBackgroundTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D backgroundTexture = new Texture2D(width, height);

            backgroundTexture.SetPixels(pixels);
            backgroundTexture.Apply();

            return backgroundTexture;
        }
    }
}