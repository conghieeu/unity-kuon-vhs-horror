using System.IO;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;
using UHFPS.Scriptable;
using ThunderWire.Editors;
using static UHFPS.Scriptable.GameLocaizationTable;
using static UHFPS.Scriptable.LocalizationLanguage;

namespace UHFPS.Editors
{
        public class LocalizationTableWindow : EditorWindow
    {
        private const float k_LanguagesWidth = 180f;
        private const float k_TableSheetWidth = 180f;
        private float Spacing => EditorGUIUtility.standardVerticalSpacing * 2;

        private GUIStyle miniLabelButton => new GUIStyle(EditorStyles.miniButton)
        {
            font = EditorStyles.miniBoldLabel.font,
            fontSize = EditorStyles.miniBoldLabel.fontSize
        };

        public class WindowSelection 
        {
            public TreeViewItem TreeViewItem;
        }

        public sealed class LanguageSelect : WindowSelection
        {
            public TempLanguageData Language;
        }

        public sealed class SectionSelect : WindowSelection
        {
            public SheetSectionTreeView Section;
        }

        public sealed class ItemSelect : WindowSelection
        {
            public SheetItemTreeView Item;
        }

        private GameLocaizationTable localizationTable;
        private LocalizationWindowData windowData;

        private SearchField searchField;
        private string searchString;
        private Vector2 scrollPosition;

        [SerializeField]
        private TreeViewState languagesTreeViewState;
        private LanguagesTreeView languagesTreeView;

        [SerializeField]
        private TreeViewState tableSheetTreeViewState;
        private TableSheetTreeView tableSheetTreeView;

        private WindowSelection selection = null;
        private bool globalExpanded = false;

        public void Show(GameLocaizationTable localizationTable)
        {
            this.localizationTable = localizationTable;
            searchField = new SearchField();
            InitializeTreeView();
        }

        private void InitializeTreeView()
        {
            // build window data
            LocalizationWindowUtility.BuildWindowData(localizationTable, out windowData);

            // set expanded status
            foreach (var section in windowData.TableSheet)
            {
                section.IsExpanded = globalExpanded;
            }

            // initialize languages tree view
            languagesTreeViewState = new TreeViewState();
            languagesTreeView = new(languagesTreeViewState, windowData)
            {
                OnLanguageSelect = (s) => selection = s
            };

            // initialize table sheet tree view
            tableSheetTreeViewState = new TreeViewState();
            tableSheetTreeView = new(tableSheetTreeViewState, windowData)
            {
                OnTableSheetSelect = (s) => selection = s
            };
        }

        private void OnGUI()
        {
            Rect toolbarRect = new(0, 0, position.width, 20f);
            GUI.Box(toolbarRect, GUIContent.none, EditorStyles.toolbar);

            // Define button size and spacing
            float buttonWidth = 100f;
            float spacing = 5f;

            // place buttons starting from the left
            Rect saveBtn = new(toolbarRect.xMax - buttonWidth - spacing, 0, buttonWidth, 20f);
            Rect importBtn = new(saveBtn.xMin - buttonWidth - spacing, 0, buttonWidth, 20f);
            Rect exportBtn = new(importBtn.xMin - buttonWidth - spacing, 0, buttonWidth, 20f);
            Rect importOldBtn = new(exportBtn.xMin - buttonWidth - spacing, 0, buttonWidth, 20f);

            // Import OLD button
            if (GUI.Button(importOldBtn, "Import OLD", EditorStyles.toolbarButton))
            {
                EditorWindow importer = GetWindow<LocalizationAssetImport>(true, "Import Game Localization Asset", true);
                importer.minSize = new Vector2(500, 185);
                importer.maxSize = new Vector2(500, 185);
                ((LocalizationAssetImport)importer).Show(windowData, localizationTable, () =>
                {
                    languagesTreeView.Reload();
                    tableSheetTreeView.Reload();
                });
            }

            // Export CSV button
            if (GUI.Button(exportBtn, "Export CSV", EditorStyles.toolbarButton))
            {
                string path = EditorUtility.SaveFilePanel("Export CSV", "", "Localization", "csv");
                LocalizationExporter.ExportLocalizationToCSV(windowData, path);
            }

            // Export CSV button
            if (GUI.Button(exportBtn, "Export CSV", EditorStyles.toolbarButton))
            {
                string path = EditorUtility.SaveFilePanel("Export CSV", "", "Localization", "csv");
                LocalizationExporter.ExportLocalizationToCSV(windowData, path);
            }

            // Import CSV button
            if (GUI.Button(importBtn, "Import CSV", EditorStyles.toolbarButton))
            {
                string path = EditorUtility.OpenFilePanel("Export CSV", "", "csv");
                LocalizationExporter.ImportLocalizationFromCSV(windowData, path);
            }

            // Save Asset button
            if (GUI.Button(saveBtn, "Save Asset", EditorStyles.toolbarButton))
            {
                BuildLocalizationTable();
                EditorUtility.SetDirty(localizationTable);
                AssetDatabase.SaveAssets();
            }

            // draw language three view
            Rect languagesRect = new Rect(5f, 25f, k_LanguagesWidth, position.height - 35f);
            languagesTreeView.OnGUI(languagesRect);

            // draw table sheet tree view
            float tableSheetStartX = languagesRect.xMax + 5f;
            Rect tableSheetRect = new Rect(tableSheetStartX, 25f, k_TableSheetWidth, position.height - 35f);
            tableSheetTreeView.OnGUI(tableSheetRect);

            // draw selection
            if (selection != null)
            {
                float inspectorStartX = tableSheetRect.xMax + 5f;
                Rect inspectorRect = new Rect(inspectorStartX, 25f, position.width - inspectorStartX - 5f, position.height - 30f);

                if (selection is LanguageSelect language)
                {
                    string title = language.Language.Entry.LanguageName;
                    GUIContent inspectorTitle = EditorGUIUtility.TrTextContentWithIcon($" INSPECTOR ({title})", "PrefabVariant On Icon");
                    EditorDrawing.DrawHeaderWithBorder(ref inspectorRect, inspectorTitle, 20f, false);

                    Rect inspectorViewRect = inspectorRect;
                    inspectorViewRect.y += Spacing;
                    inspectorViewRect.yMax -= Spacing;
                    inspectorViewRect.xMin += Spacing;
                    inspectorViewRect.xMax -= Spacing;

                    GUILayout.BeginArea(inspectorViewRect);
                    OnDrawLanguageInspector(language);
                    GUILayout.EndArea();
                }
                else if(selection is SectionSelect section)
                {
                    string title = section.Section.Name;
                    GUIContent inspectorTitle = EditorGUIUtility.TrTextContentWithIcon($" INSPECTOR ({title})", "PrefabVariant On Icon");
                    EditorDrawing.DrawHeaderWithBorder(ref inspectorRect, inspectorTitle, 20f, false);

                    Rect inspectorViewRect = inspectorRect;
                    inspectorViewRect.y += Spacing;
                    inspectorViewRect.yMax -= Spacing;
                    inspectorViewRect.xMin += Spacing;
                    inspectorViewRect.xMax -= Spacing;

                    GUILayout.BeginArea(inspectorViewRect);
                    OnDrawSectionInspector(section);
                    GUILayout.EndArea();
                }
                else if (selection is ItemSelect item)
                {
                    string title = item.Item.Key;
                    GUIContent inspectorTitle = EditorGUIUtility.TrTextContentWithIcon($" INSPECTOR ({title})", "PrefabVariant On Icon");
                    EditorDrawing.DrawHeaderWithBorder(ref inspectorRect, inspectorTitle, 20f, false);

                    Rect inspectorViewRect = inspectorRect;
                    inspectorViewRect.y += Spacing;
                    inspectorViewRect.yMax -= Spacing;
                    inspectorViewRect.xMin += Spacing;
                    inspectorViewRect.xMax -= Spacing;

                    GUILayout.BeginArea(inspectorViewRect);
                    OnDrawSectionItemInspector(item);
                    GUILayout.EndArea();
                }
            }
        }

        private void OnDrawSectionInspector(SectionSelect section)
        {
            // section name change
            EditorGUI.BeginChangeCheck();
            {
                section.Section.Name = EditorGUILayout.TextField("Name", section.Section.Name);
            }
            if (EditorGUI.EndChangeCheck())
            {
                section.TreeViewItem.displayName = section.Section.Name;
            }

            using (new EditorGUI.DisabledGroupScope(true))
            {
                int childerCount = section.TreeViewItem.children?.Count ?? 0;
                EditorGUILayout.IntField(new GUIContent("Keys"), childerCount);
            }

            EditorGUILayout.Space(2);
            EditorDrawing.Separator();
            EditorGUILayout.Space(1);

            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.LabelField("Id: " + section.Section.Id, EditorStyles.miniBoldLabel);
            }
        }

        private void OnDrawSectionItemInspector(ItemSelect item)
        {
            // item key change
            EditorGUI.BeginChangeCheck();
            {
                item.Item.Key = EditorGUILayout.TextField("Key", item.Item.Key);
            }
            if (EditorGUI.EndChangeCheck())
            {
                item.TreeViewItem.displayName = item.Item.Key;
            }

            EditorGUILayout.Space(2);
            EditorDrawing.Separator();
            EditorGUILayout.Space(1);

            using (new EditorGUI.DisabledGroupScope(true))
            {
                string parentName = item.Item.Parent.Name;
                string parentText = item.Item.Parent.Id + $" ({parentName})";
                EditorGUILayout.LabelField("Parent Id: " + parentText, EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("Id: " + item.Item.Id, EditorStyles.miniBoldLabel);
            }
        }

        private void OnDrawLanguageInspector(LanguageSelect selection)
        {
            var language = selection.Language;
            var entry = language.Entry;
            var treeView = selection.TreeViewItem;

            using (new EditorDrawing.BorderBoxScope(false))
            {
                // language name change
                Rect nameRect = EditorGUILayout.GetControlRect();
                EditorGUI.BeginChangeCheck();
                {
                    nameRect = EditorGUI.PrefixLabel(nameRect, new GUIContent("Name"));
                    nameRect.xMax -= EditorGUIUtility.singleLineHeight + 2f;
                    entry.LanguageName = EditorGUI.TextField(nameRect, entry.LanguageName);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    treeView.displayName = entry.LanguageName;
                }

                Rect renameAssetRect = nameRect;
                renameAssetRect.xMin = nameRect.xMax + 2f;
                renameAssetRect.width = EditorGUIUtility.singleLineHeight;

                using (new EditorGUI.DisabledGroupScope(entry.Asset == null))
                {
                    GUIContent editIcon = EditorGUIUtility.IconContent("editicon.sml", "Rename");
                    if (GUI.Button(renameAssetRect, editIcon, EditorStyles.iconButton))
                    {
                        string assetPath = AssetDatabase.GetAssetPath(entry.Asset);
                        string newName = "(Language) " + entry.LanguageName;
                        AssetDatabase.RenameAsset(assetPath, newName);
                    }
                }

                // language asset change
                EditorGUI.BeginChangeCheck();
                {
                    entry.Asset = (LocalizationLanguage)EditorGUILayout.ObjectField("Asset", entry.Asset, typeof(LocalizationLanguage), false);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    windowData.AssignLanguage(language, entry.Asset);
                    treeView.displayName = entry.Asset.LanguageName;
                }

                // draw create asset button if needed
                if(entry.Asset == null)
                {
                    if (GUILayout.Button("Create Asset"))
                    {
                        string tablePath = AssetDatabase.GetAssetPath(localizationTable);
                        if (!string.IsNullOrEmpty(tablePath))
                        {
                            string directoryPath = Path.GetDirectoryName(tablePath);

                            // Create a new instance of LocalizationLanguage
                            LocalizationLanguage newAsset = ScriptableObject.CreateInstance<LocalizationLanguage>();
                            string uniquePath = Path.Combine(directoryPath, "NewLocalizationLanguage.asset");
                            string assetPath = AssetDatabase.GenerateUniqueAssetPath(uniquePath);

                            // Save the asset
                            AssetDatabase.CreateAsset(newAsset, assetPath);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();

                            // Assign the created asset to the entry
                            entry.Asset = newAsset;

                            Debug.Log($"LocalizationLanguage asset created at: {assetPath}");
                        }
                        else
                        {
                            Debug.LogError("Unable to determine the path of the LocalizationTable ScriptableObject.");
                        }
                    }
                }
            }

            using (new EditorGUI.DisabledGroupScope(entry.Asset == null))
            {
                // Draw search field
                EditorGUILayout.Space();

                GUIContent expandText = new GUIContent("Expand");
                float expandWidth = miniLabelButton.CalcSize(expandText).x;

                var searchRect = EditorGUILayout.GetControlRect();
                searchRect.xMax -= (expandWidth + 2f);
                searchString = searchField.OnGUI(searchRect, searchString);

                Rect expandRect = new Rect(searchRect.xMax + 2f, searchRect.y, expandWidth, searchRect.height);
                expandRect.y -= 1f;

                using (new EditorDrawing.BackgroundColorScope("#F7E987"))
                {
                    if (GUI.Button(expandRect, expandText, miniLabelButton))
                    {
                        globalExpanded = !globalExpanded;
                        foreach (var section in language.TableSheet)
                        {
                            section.Reference.IsExpanded = globalExpanded;
                        }
                    }
                }

                if (entry.Asset != null)
                {
                    // Draw localization data
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                    {
                        foreach (var section in GetSearchResult(language, searchString))
                        {
                            DrawLocalizationKey(section);
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
                else
                {
                    EditorGUILayout.HelpBox("To begin editing localization data, you must first assign a localization asset.", MessageType.Warning);
                }
            }
        }

        private void DrawLocalizationKey(TempSheetSection section)
        {
            if (section.Items == null || section.Items.Count == 0)
                return;

            using (new EditorDrawing.BorderBoxScope(false))
            {
                string sectionName = section.Name.Replace(" ", "");
                section.Reference.IsExpanded = EditorGUILayout.Foldout(section.Reference.IsExpanded, new GUIContent(sectionName), true, EditorDrawing.Styles.miniBoldLabelFoldout);

                // Show section keys when expanded
                if (section.Reference.IsExpanded)
                {
                    foreach (var item in section.Items)
                    {
                        string keyName = item.Key.Replace(" ", "");
                        string key = sectionName + "." + keyName;

                        if (IsMultiline(item.Value))
                            key += " (Multiline)";

                        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                        {
                            // Display the expandable toggle
                            item.IsExpanded = EditorGUILayout.Foldout(item.IsExpanded, new GUIContent(key), true, EditorDrawing.Styles.miniBoldLabelFoldout);

                            if (item.IsExpanded)
                            {
                                // Show TextArea when expanded
                                float height = (EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight) * 3;
                                height += EditorGUIUtility.standardVerticalSpacing;

                                item.Scroll = EditorGUILayout.BeginScrollView(item.Scroll, GUILayout.Height(height));
                                item.Value = EditorGUILayout.TextArea(item.Value, GUILayout.ExpandHeight(true));
                                EditorGUILayout.EndScrollView();
                            }
                            else
                            {
                                // Show TextField when collapsed
                                item.Value = EditorGUILayout.TextField(item.Value);
                            }
                        }
                    }
                }
            }

            EditorGUILayout.Space(1f);
        }

        private IEnumerable<TempSheetSection> GetSearchResult(TempLanguageData languageData, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                List<TempSheetSection> searchResult = new();

                foreach (var section in languageData.TableSheet)
                {
                    List<TempSheetItem> sectionItems = new();
                    string sectionName = section.Name.Replace(" ", "");

                    foreach (var item in section.Items)
                    {
                        string keyName = item.Key.Replace(" ", "");
                        string key = sectionName + "." + keyName;

                        if (key.Contains(search))
                            sectionItems.Add(item);
                    }

                    searchResult.Add(new TempSheetSection()
                    {
                        Items = sectionItems,
                        Reference = section.Reference
                    });
                }

                return searchResult;
            }

            return languageData.TableSheet;
        }

        private bool IsMultiline(string text)
        {
            return text.Contains("\n") || text.Contains("\r");
        }

        private void BuildLocalizationTable()
        {
            // 1. build table sheet
            localizationTable.TableSheet = new();
            foreach (var section in windowData.TableSheet)
            {
                TableData tableData = new TableData(section.Name, section.Id);

                foreach (var item in section.Items)
                {
                    SheetItem sheetItem = new SheetItem(item.Key, item.Id);
                    tableData.SectionSheet.Add(sheetItem);
                }

                localizationTable.TableSheet.Add(tableData);
            }

            // 2. build table sheet for each language
            IList<LocalizationLanguage> languages = new List<LocalizationLanguage>();
            foreach (var language in windowData.Languages)
            {
                if (language.Entry.Asset == null)
                    continue;

                LocalizationLanguage asset = language.Entry.Asset;
                IList<LocalizationString> strings = new List<LocalizationString>();

                foreach (var section in language.TableSheet)
                {
                    string sectionKey = section.Name.Replace(" ", "");
                    
                    foreach (var item in section.Items)
                    {
                        string itemKey = item.Key.Replace(" ", "");
                        string key = sectionKey + "." + itemKey;

                        strings.Add(new()
                        {
                            SectionId = section.Id,
                            EntryId = item.Id,
                            Key = key,
                            Value = item.Value
                        });
                    }
                }

                // assign name and localization strings to language asset
                asset.LanguageName = language.Entry.LanguageName;
                asset.Strings = new(strings);

                languages.Add(asset);
                EditorUtility.SetDirty(asset);
            }

            // 3. assign languages to localization table
            localizationTable.Languages = new(languages);
        }
    }
}
