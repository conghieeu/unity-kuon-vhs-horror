using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Editors;
using static UHFPS.Runtime.SaveGameManager;

namespace UHFPS.Editors
{
    public class SaveablesExplorer : EditorWindow
    {
        [Serializable]
        public abstract class ExplorerItem
        {
            public bool IsExpanded { get; set; }
        }

        [Serializable]
        public class ExSingleSaveable : ExplorerItem
        {
            public SaveablePair Reference { get; private set; }

            public ExSingleSaveable(SaveablePair pair)
            {
                Reference = pair;
                IsExpanded = false;
            }
        }

        [Serializable]
        public class ExRuntimeSaveable : ExplorerItem
        {
            public RuntimeSaveable Reference { get; private set; }
            public List<ExSingleSaveable> Saveables { get; private set; }
            public bool ListExpanded { get; set; }

            public ExRuntimeSaveable(RuntimeSaveable runtime)
            {
                Reference = runtime;
                Saveables = new();
                foreach (var pair in runtime.SaveablePairs)
                {
                    Saveables.Add(new ExSingleSaveable(pair));
                }

                IsExpanded = false;
            }
        }

        private GUIContent WarningIcon => EditorGUIUtility.IconContent("console.warnicon.sml");

        private List<ExSingleSaveable> exWorldSveables;
        private List<ExRuntimeSaveable> exRuntimeSaveables;
        private int selectedList;

        private SearchField searchField;
        private string searchString;
        private Vector2 scrollPosition;

        public void Show(IList<SaveablePair> worldSveables, IList<RuntimeSaveable> runtimeSaveables)
        {
            exWorldSveables = new();
            foreach (var item in worldSveables.OrderBy(x => x.Instance.GetType().Name))
            {
                exWorldSveables.Add(new(item));
            }

            exRuntimeSaveables = new();
            foreach (var item in runtimeSaveables.OrderBy(x => x.InstantiatedObject.name))
            {
                exRuntimeSaveables.Add(new(item));
            }

            searchField = new SearchField();
            searchString = string.Empty;
        }

        private void OnGUI()
        {
            Rect rect = position;
            rect.xMin += 5f;
            rect.xMax -= 5f;
            rect.yMin += 5f;
            rect.yMax -= 5f;
            rect.x = 5;
            rect.y = 5;

            GUILayout.BeginArea(rect);
            {
                DrawSaveableListSelector();
                EditorDrawing.SeparatorSpaced(5f);

                var searchRect = EditorGUILayout.GetControlRect();
                searchString = searchField.OnGUI(searchRect, searchString);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                {
                    using (new EditorDrawing.BorderBoxScope(false))
                    {
                        switch (selectedList)
                        {
                            case 0: DrawWorldSaveables(); break;
                            case 1: DrawRuntimeSaveables(); break;
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        private void DrawWorldSaveables()
        {
            if (exWorldSveables.Count > 0)
            {
                foreach (var pair in exWorldSveables.Where(x => MatchSearch(x.Reference.Instance != null ? x.Reference.Instance.gameObject : null, x.Reference.Token)))
                {
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {
                        Rect rect = EditorGUILayout.GetControlRect();

                        if (pair.Reference.Instance == null)
                        {
                            Rect warnRect = rect;
                            warnRect.xMin = warnRect.xMax - EditorGUIUtility.singleLineHeight;
                            warnRect.x -= 2f;
                            EditorGUI.LabelField(warnRect, WarningIcon);
                        }

                        if (pair.IsExpanded = EditorGUI.Foldout(rect, pair.IsExpanded, new GUIContent(pair.Reference.Token), true, EditorDrawing.Styles.miniBoldLabelFoldout))
                        {
                            if (pair.Reference.Instance == null)
                            {
                                EditorUtils.TrHelpIconText("The registered world saveable has been destroyed! Click the <b>Find Saveables</b> button.", MessageType.Warning, true);
                            }

                            EditorGUILayout.TextField(new GUIContent("Token"), pair.Reference.Token);
                            EditorGUILayout.ObjectField(new GUIContent("Instance"), pair.Reference.Instance, typeof(MonoBehaviour), true);
                        }
                    }
                }
            }
            else
            {
                EditorDrawing.RichHelpBox("There are no world saveables registered in the SaveGameManager. Click the <b>Find Saveables</b> button.", MessageType.Info);
            }
        }

        private void DrawRuntimeSaveables()
        {
            if (exRuntimeSaveables.Count > 0)
            {
                foreach (var pair in exRuntimeSaveables.Where(x => MatchSearch(x.Reference.InstantiatedObject, x.Reference.TokenGUID)))
                {
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {
                        Rect rect = EditorGUILayout.GetControlRect();

                        if (pair.Reference.InstantiatedObject == null)
                        {
                            Rect warnRect = rect;
                            warnRect.xMin = warnRect.xMax - EditorGUIUtility.singleLineHeight;
                            warnRect.x -= 2f;
                            EditorGUI.LabelField(warnRect, WarningIcon);
                        }

                        if (pair.IsExpanded = EditorGUI.Foldout(rect, pair.IsExpanded, new GUIContent(pair.Reference.TokenGUID), true, EditorDrawing.Styles.miniBoldLabelFoldout))
                        {
                            if (pair.Reference.InstantiatedObject == null)
                            {
                                EditorUtils.TrHelpIconText("The instantiated object has been destroyed outside SaveGameManager! Runtime Saveables should be removed using the <b>SaveGameManager.RemoveSaveable()</b> function.", MessageType.Warning, true);
                            }

                            EditorGUILayout.TextField(new GUIContent("Token"), pair.Reference.TokenGUID);
                            EditorGUILayout.ObjectField(new GUIContent("Object"), pair.Reference.InstantiatedObject, typeof(GameObject), true);

                            // draw runtime saveable pairs
                            if (pair.ListExpanded = EditorGUILayout.Foldout(pair.ListExpanded, new GUIContent("Saveable Pairs"), true, EditorDrawing.Styles.miniBoldLabelFoldout))
                            {
                                EditorGUI.indentLevel++;
                                foreach (var singlePair in pair.Saveables)
                                {
                                    DrawSinglePair(singlePair);
                                }
                                EditorGUI.indentLevel--;
                            }
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("You need to enter play mode and instantiate the runtime saveables in order to display them here.", MessageType.Info);
            }
        }

        private bool MatchSearch(GameObject obj, string token)
        {
            string objName = "";
            if(obj != null) objName = obj.name.ToLower();

            string tokenName = token.ToLower();
            string search = searchString.ToLower();
            return objName.Contains(search) || tokenName.Contains(search);
        }

        private void DrawSinglePair(ExSingleSaveable pair)
        {
            if (pair.IsExpanded = EditorGUILayout.Foldout(pair.IsExpanded, new GUIContent(pair.Reference.Token), true, EditorDrawing.Styles.miniBoldLabelFoldout))
            {
                EditorGUILayout.TextField(new GUIContent("Token"), pair.Reference.Token);
                EditorGUILayout.ObjectField(new GUIContent("Instance"), pair.Reference.Instance, typeof(MonoBehaviour), true);
            }
        }

        private void DrawSaveableListSelector()
        {
            GUIContent[] toolbarContent = {
                new GUIContent("World Saveables"),
                new GUIContent("Runtime Saveables")
            };

            GUIStyle toolbarButtons = new(GUI.skin.button)
            {
                fixedHeight = 0,
                fixedWidth = 125
            };

            // Calculate total toolbar width
            float buttonWidth = 125f;
            float toolbarWidth = buttonWidth * toolbarContent.Length;

            // Get centered position
            Rect toolbarRect = EditorGUILayout.GetControlRect(false, 25);
            toolbarRect.width = toolbarWidth;
            toolbarRect.x = (position.width - toolbarWidth) / 2;

            EditorGUI.BeginChangeCheck();
            selectedList = GUI.Toolbar(toolbarRect, selectedList, toolbarContent, toolbarButtons);
            if (EditorGUI.EndChangeCheck())
            {
                GUI.FocusControl(null);
                searchString = string.Empty;
            }
        }

    }
}