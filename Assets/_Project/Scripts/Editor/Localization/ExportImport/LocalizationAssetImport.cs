using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UHFPS.Scriptable;
using static UHFPS.Scriptable.LocalizationLanguage;

namespace UHFPS.Editors
{
    public class LocalizationAssetImport : EditorWindow
    {
        private LocalizationWindowData windowData;
        private GameLocaizationTable locaizationTable;

        private GameLocalizationAsset localizationAsset;
        private Action onImported;

        public void Show(LocalizationWindowData windowData, GameLocaizationTable locaizationTable, Action onImported = null)
        {
            this.windowData = windowData;
            this.locaizationTable = locaizationTable;
            this.onImported = onImported;
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
                EditorGUILayout.HelpBox("With this tool you can import the localization language from the old Game Localization Asset. It will import all sections and keys from the old asset and assign the localization data.", MessageType.Info);

                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    localizationAsset = (GameLocalizationAsset)EditorGUILayout.ObjectField(new GUIContent("Game Localization Asset"), localizationAsset, typeof(GameLocalizationAsset), false);

                    EditorGUILayout.Space();
                    using (new EditorGUI.DisabledGroupScope(localizationAsset == null))
                    {
                        if (GUILayout.Button(new GUIContent("Import Asset"), GUILayout.Height(25f)))
                        {
                            ImportAsset();
                        }
                    }
                }
            }
            GUILayout.EndArea();
        }

        private void ImportAsset()
        {
            if (localizationAsset == null)
            {
                Debug.LogError("Localization Asset is null.");
                return;
            }

            if (windowData == null)
            {
                Debug.LogError("windowData is not assigned.");
                return;
            }

            // Create the new LocalizationLanguage asset
            LocalizationLanguage newLanguageAsset = CreateInstance<LocalizationLanguage>();
            newLanguageAsset.LanguageName = localizationAsset.LanguageName;
            newLanguageAsset.Strings = new List<LocalizationString>();

            // Add newly created language asset to windowData
            LocalizationWindowUtility.AddLanguage(windowData, localizationAsset.LanguageName, newLanguageAsset, false);

            // Process each section from the old asset.
            for (int i = 0; i < localizationAsset.Localizations.Count; i++)
            {
                GameLocalizationAsset.LocalizationSection oldSection = localizationAsset.Localizations[i];
                SheetSectionData existingSection = null;

                // Find an existing section in windowData.TableSheet.
                for (int j = 0; j < windowData.TableSheet.Count; j++)
                {
                    SheetSectionData section = windowData.TableSheet[j];
                    if (!string.IsNullOrEmpty(section.Name) && section.Name.StartsWith(oldSection.Section))
                    {
                        existingSection = section;
                        break;
                    }
                }

                // If not found, add a new section.
                if (existingSection == null)
                {
                    existingSection = LocalizationWindowUtility.AddSection(windowData, oldSection.Section, false);
                    Debug.Log("Added new section: " + existingSection.Name);
                }

                // Process each localization entry (key) in the old section.
                for (int k = 0; k < oldSection.Localizations.Count; k++)
                {
                    GameLocalizationAsset.Localization oldLoc = oldSection.Localizations[k];
                    SheetItemTreeView existingItem = null;

                    // Check if the key exists.
                    for (int m = 0; m < existingSection.Items.Count; m++)
                    {
                        if (existingSection.Items[m].Key == oldLoc.Key)
                        {
                            existingItem = existingSection.Items[m];
                            break;
                        }
                    }

                    if (existingItem == null)
                    {
                        // Create the new key.
                        existingItem = new SheetItemTreeView
                        {
                            Key = oldLoc.Key,
                            Parent = existingSection
                        };

                        existingSection.Items.Add(existingItem);
                        Debug.Log("Added new key: " + oldLoc.Key + " to section: " + existingSection.Name);

                        // For every language, add a corresponding TempSheetItem.
                        for (int n = 0; n < windowData.Languages.Count; n++)
                        {
                            TempLanguageData langData = windowData.Languages[n];
                            TempSheetSection tempSection = null;

                            for (int p = 0; p < langData.TableSheet.Count; p++)
                            {
                                if (langData.TableSheet[p].Reference == existingSection)
                                {
                                    tempSection = langData.TableSheet[p];
                                    break;
                                }
                            }

                            if (tempSection != null)
                            {
                                tempSection.Items.Add(new TempSheetItem
                                {
                                    Reference = existingItem,
                                    Value = string.Empty
                                });
                            }
                        }
                    }
                }
            }

            // Populate the new LocalizationLanguage asset with localization strings.
            for (int i = 0; i < windowData.TableSheet.Count; i++)
            {
                SheetSectionData section = windowData.TableSheet[i];
                bool foundMatchingSection = false;

                // Match old section entry with the new section data
                GameLocalizationAsset.LocalizationSection matchingOldSection = new();
                for (int j = 0; j < localizationAsset.Localizations.Count; j++)
                {
                    GameLocalizationAsset.LocalizationSection tempOldSection = localizationAsset.Localizations[j];
                    if (!string.IsNullOrEmpty(section.Name) && section.Name.StartsWith(tempOldSection.Section))
                    {
                        matchingOldSection = tempOldSection;
                        foundMatchingSection = true;
                        break;
                    }
                }

                if (foundMatchingSection)
                {
                    for (int k = 0; k < section.Items.Count; k++)
                    {
                        SheetItemTreeView item = section.Items[k];
                        string value = string.Empty;

                        for (int l = 0; l < matchingOldSection.Localizations.Count; l++)
                        {
                            GameLocalizationAsset.Localization loc = matchingOldSection.Localizations[l];
                            if (loc.Key == item.Key)
                            {
                                value = loc.Text;
                                break;
                            }
                        }

                        // Populate language strings from the old localization data
                        string key = (section.Name + "." + item.Key).Replace(" ", "");
                        newLanguageAsset.Strings.Add(new LocalizationString
                        {
                            SectionId = section.Id,
                            EntryId = item.Id,
                            Key = key,
                            Value = value
                        });
                    }
                }
            }

            // Save the new LocalizationLanguage asset to the project.
            string pathToTable = AssetDatabase.GetAssetPath(locaizationTable);
            string directoryPath = Path.GetDirectoryName(pathToTable);
            string assetName = $"({newLanguageAsset.LanguageName}) New Language.asset";
            string assetPath = Path.Combine(directoryPath, assetName).Replace("\\", "/");

            AssetDatabase.CreateAsset(newLanguageAsset, assetPath);
            AssetDatabase.SaveAssets();

            onImported?.Invoke();
            Debug.Log("Import completed.");
        }
    }
}