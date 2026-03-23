using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UHFPS.Scriptable;

namespace UHFPS.Editors
{
    public sealed class LanguageEntry
    {
        public string LanguageName;
        public LocalizationLanguage Asset;

        public LanguageEntry() { }

        public LanguageEntry(LocalizationLanguage asset)
        {
            LanguageName = asset.LanguageName;
            Asset = asset;
        }
    }

    public class SheetSectionTreeView
    {
        public int Id;
        public string Name;

        public SheetSectionTreeView()
        {
            Id = Random.Range(10000000, 99999999);
            Name = LocalizationWindowUtility.NULL;
        }
    }

    public class SheetItemTreeView
    {
        public int Id;
        public string Key;
        public SheetSectionTreeView Parent;

        public SheetItemTreeView()
        {
            Id = Random.Range(10000000, 99999999);
            Key = LocalizationWindowUtility.NULL;
        }
    }

    public sealed class TempSheetItem
    {
        // Reference to the original item (shared instance)
        public SheetItemTreeView Reference;

        // Language-specific localization value
        public string Value;

        public int Id => Reference.Id;
        public string Key
        {
            get => Reference.Key;
            set => Reference.Key = value;
        }

        public SheetSectionTreeView Parent => Reference.Parent;
        public Vector2 Scroll { get; set; }
        public bool IsExpanded { get; set; }
    }

    public sealed class TempSheetSection
    {
        // Reference to the original section (shared instance)
        public SheetSectionData Reference;

        // Language-specific items
        public List<TempSheetItem> Items = new();

        public int Id => Reference.Id;
        public string Name
        {
            get => Reference.Name;
            set => Reference.Name = value;
        }
    }

    public sealed class SheetSectionData : SheetSectionTreeView
    {
        public List<SheetItemTreeView> Items = new();
        public bool IsExpanded { get; set; }
    }

    public sealed class TempLanguageData
    {
        public LanguageEntry Entry;
        public List<TempSheetSection> TableSheet = new();
    }

    public sealed class LocalizationWindowData
    {
        public List<TempLanguageData> Languages = new();
        public List<SheetSectionData> TableSheet = new();

        public int LanguageCount = 0;
        public int SectionCount = 0;
        public int EntryCount = 0;
    }

    public static class LocalizationWindowUtility
    {
        public const string NULL = "null";

        public static void BuildWindowData(GameLocaizationTable table, out LocalizationWindowData data)
        {
            LocalizationWindowData windowData = new();

            int languages = 0;
            int sections = 0;
            int entries = 0;

            // 1. Build the main table structure
            foreach (var tableData in table.TableSheet)
            {
                SheetSectionData sectionData = new()
                {
                    Id = tableData.Id,
                    Name = tableData.SectionName,
                    Items = new List<SheetItemTreeView>()
                };

                foreach (var item in tableData.SectionSheet)
                {
                    sectionData.Items.Add(new()
                    {
                        Id = item.Id,
                        Key = item.Key,
                        Parent = sectionData
                    });
                    entries++;
                }

                windowData.TableSheet.Add(sectionData);
                sections++;
            }

            windowData.SectionCount = sections;
            windowData.EntryCount = entries;

            // 2. Build language-specific data that references the same sections and items
            foreach (var lang in table.Languages)
            {
                TempLanguageData langData = new()
                {
                    Entry = new(lang)
                };

                // Assign language name from asset
                if (lang != null)
                {
                    string name = lang.LanguageName;
                    langData.Entry.LanguageName = name;
                }

                foreach (var globalSection in windowData.TableSheet)
                {
                    int _sectionId = globalSection.Id;

                    // Create a TempSheetSection reference and assign shared section reference
                    TempSheetSection tempSection = new()
                    {
                        Reference = globalSection
                    };

                    // For each item in the global section, we create a TempSheetItem that references it
                    foreach (var globalItem in globalSection.Items)
                    {
                        int _entryId = globalItem.Id;
                        string value = NULL;

                        if (lang != null)
                        {
                            foreach (var item in lang.Strings)
                            {
                                // Try match the localized string from language asset
                                if(item.SectionId == _sectionId && item.EntryId == _entryId)
                                {
                                    value = item.Value;
                                    break;
                                }
                            }
                        }

                        // Add SheetItem to section items list and assign shared item reference
                        tempSection.Items.Add(new()
                        {
                            Reference = globalItem,
                            Value = value
                        });
                    }

                    langData.TableSheet.Add(tempSection);
                }

                windowData.Languages.Add(langData);
                languages++;
            }

            windowData.LanguageCount = languages;
            data = windowData;
        }

        /// <summary>
        /// Assign a language asset to an existing TempLanguageData.
        /// </summary>
        public static void AssignLanguage(this LocalizationWindowData data, TempLanguageData languageData, LocalizationLanguage asset)
        {
            // Assign the asset to the language entry
            languageData.Entry.Asset = asset;

            if (asset == null)
            {
                // If no asset is assigned, clear all values
                foreach (var tempSection in languageData.TableSheet)
                {
                    foreach (var tempItem in tempSection.Items)
                    {
                        tempItem.Value = string.Empty;
                    }
                }
                return;
            }
            else
            {
                // Assign language name from asset
                languageData.Entry.LanguageName = asset.LanguageName;
            }

            // If we have a valid asset, we try to match each TempSheetItem to a corresponding LocalizationString
            foreach (var tempSection in languageData.TableSheet)
            {
                int sectionId = tempSection.Reference.Id;

                foreach (var tempItem in tempSection.Items)
                {
                    int entryId = tempItem.Reference.Id;
                    string value = string.Empty;

                    foreach (var item in asset.Strings)
                    {
                        // Try match the localized string from language asset
                        if (item.SectionId == sectionId && item.EntryId == entryId)
                        {
                            value = item.Value;
                            break;
                        }
                    }

                    // Assign the localization value
                    tempItem.Value = value;
                }
            }
        }

        /// <summary>
        /// Add a new language.
        /// </summary>
        public static void AddLanguage(this LocalizationWindowData data, string languageName, LocalizationLanguage asset, bool withIndex = true)
        {
            if (withIndex)
            {
                int languageIndex = ++data.LanguageCount;
                languageName += " " + languageIndex;
            }

            var newLanguageEntry = new LanguageEntry()
            {
                LanguageName = languageName,
                Asset = asset
            };

            TempLanguageData newLangData = new()
            {
                Entry = newLanguageEntry,
                TableSheet = new List<TempSheetSection>()
            };

            // Reference existing sections and items
            foreach (var sectionData in data.TableSheet)
            {
                TempSheetSection tempSection = new()
                {
                    Reference = sectionData,
                    Items = new List<TempSheetItem>()
                };

                foreach (var globalItem in sectionData.Items)
                {
                    tempSection.Items.Add(new()
                    {
                        Reference = globalItem,
                        Value = string.Empty
                    });
                }

                newLangData.TableSheet.Add(tempSection);
            }

            data.Languages.Add(newLangData);
        }

        /// <summary>
        /// Add a new section to the global TableSheet.
        /// </summary>
        public static SheetSectionData AddSection(this LocalizationWindowData data, string sectionName, bool withIndex = true)
        {
            if (withIndex)
            {
                int sectionIndex = ++data.SectionCount;
                sectionName += " " + sectionIndex;
            }

            SheetSectionData newSection = new()
            {
                Name = sectionName,
                Items = new List<SheetItemTreeView>()
            };

            data.TableSheet.Add(newSection);

            // Add corresponding section to each language
            foreach (var lang in data.Languages)
            {
                lang.TableSheet.Add(new()
                {
                    Reference = newSection,
                    Items = new List<TempSheetItem>()
                });
            }

            return newSection;
        }

        /// <summary>
        /// Remove a section from the global TableSheet by reference.
        /// </summary>
        public static void RemoveSection(this LocalizationWindowData data, SheetSectionTreeView section)
        {
            // Remove section
            int sectionIndex = data.TableSheet.FindIndex(x => x.Id == section.Id);
            if (sectionIndex != -1) data.TableSheet.RemoveAt(sectionIndex);

            // Remove the corresponding section from each language
            foreach (var lang in data.Languages)
            {
                var tempSection = lang.TableSheet.FirstOrDefault(ts => ts.Reference == section);
                if (tempSection != null) lang.TableSheet.Remove(tempSection);
            }
        }

        /// <summary>
        /// Add a new item to a given section.
        /// </summary>
        public static SheetItemTreeView AddItem(this LocalizationWindowData data, SheetSectionData section, string key, bool withIndex = true)
        {
            if (withIndex)
            {
                int keyIndex = ++data.EntryCount;
                key += " " + keyIndex;
            }

            SheetItemTreeView newItem = new()
            {
                Key = key,
                Parent = section
            };

            section.Items.Add(newItem);

            // Add corresponding item to each language
            foreach (var lang in data.Languages)
            {
                var tempSection = lang.TableSheet.FirstOrDefault(ts => ts.Reference == section);
                if (tempSection != null)
                {
                    tempSection.Items.Add(new()
                    {
                        Reference = newItem,
                        Value = string.Empty
                    });
                }
            }

            data.EntryCount++;
            return newItem;
        }

        /// <summary>
        /// Remove an item by reference from the given section.
        /// </summary>
        public static void RemoveItem(this LocalizationWindowData data, SheetSectionTreeView section, SheetItemTreeView item)
        {
            // Remove item from section
            int sectionIndex = data.TableSheet.FindIndex(x => x.Id == section.Id);
            if (sectionIndex != -1) data.TableSheet[sectionIndex].Items.Remove(item);

            // Remove corresponding item from each language
            foreach (var lang in data.Languages)
            {
                var tempSection = lang.TableSheet.FirstOrDefault(ts => ts.Reference == section);
                if (tempSection != null)
                {
                    var tempItem = tempSection.Items.FirstOrDefault(ti => ti.Reference == item);
                    if (tempItem != null) tempSection.Items.Remove(tempItem);
                }
            }
        }

        /// <summary>
        /// Moves an item within the same section to a new position.
        /// </summary>
        public static void OnMoveItemWithinSection(this LocalizationWindowData data, SheetSectionTreeView section, SheetItemTreeView item, int position)
        {
            // Find the corresponding SheetSectionData in data
            var sectionData = data.TableSheet.FirstOrDefault(s => s.Id == section.Id);
            if (sectionData == null)
                return;

            int oldIndex = sectionData.Items.IndexOf(item);
            if (oldIndex < 0)
                return;

            // Clamp position
            int insertTo = position > oldIndex ? position - 1 : position;
            insertTo = Mathf.Clamp(insertTo, 0, sectionData.Items.Count);

            if (oldIndex == insertTo)
                return;

            sectionData.Items.RemoveAt(oldIndex);
            sectionData.Items.Insert(insertTo, item);

            // Update in each language
            foreach (var lang in data.Languages)
            {
                // Find the corresponding TempSheetSection
                var tempSection = lang.TableSheet.FirstOrDefault(ts => ts.Reference == sectionData);
                if (tempSection == null) continue;

                int langOldIndex = tempSection.Items.FindIndex(i => i.Reference == item);
                if (langOldIndex < 0) continue;

                // Clamp position
                int insertToLang = position > langOldIndex ? position - 1 : position;
                insertToLang = Mathf.Clamp(insertToLang, 0, tempSection.Items.Count);
                if (langOldIndex == position) continue;

                var tempItem = tempSection.Items[langOldIndex];
                tempSection.Items.RemoveAt(langOldIndex);
                tempSection.Items.Insert(insertToLang, tempItem);
            }
        }

        /// <summary>
        /// Moves an item from one section to another section, inserting it at a specific position.
        /// </summary>
        public static void OnMoveItemToSectionAt(this LocalizationWindowData data, SheetSectionTreeView parent, SheetSectionTreeView section, SheetItemTreeView item, int position)
        {
            // Find both parent and target sections
            var parentSection = data.TableSheet.FirstOrDefault(s => s.Id == parent.Id);
            var targetSection = data.TableSheet.FirstOrDefault(s => s.Id == section.Id);

            if (parentSection == null || targetSection == null)
                return;

            // Remove from parent
            if (!parentSection.Items.Remove(item))
                return;

            // Insert into target at specified position
            position = Mathf.Max(0, Mathf.Min(position, targetSection.Items.Count));

            item.Parent = targetSection;
            targetSection.Items.Insert(position, item);

            // Update in each language
            foreach (var lang in data.Languages)
            {
                var langParentSection = lang.TableSheet.FirstOrDefault(ts => ts.Reference == parentSection);
                var langTargetSection = lang.TableSheet.FirstOrDefault(ts => ts.Reference == targetSection);

                if (langParentSection == null || langTargetSection == null)
                    continue;

                // Remove the corresponding temp item from the parent section
                var tempItem = langParentSection.Items.FirstOrDefault(i => i.Reference == item);
                if (tempItem == null) continue;

                langParentSection.Items.Remove(tempItem);

                // Insert into target section
                position = Mathf.Max(0, Mathf.Min(position, langTargetSection.Items.Count));
                langTargetSection.Items.Insert(position, tempItem);
            }
        }

        /// <summary>
        /// Moves an item from one section to another section, adding it to the end.
        /// </summary>
        public static void OnMoveItemToSection(this LocalizationWindowData data, SheetSectionTreeView parent, SheetSectionTreeView section, SheetItemTreeView item)
        {
            // Find both parent and target sections
            var parentSection = data.TableSheet.FirstOrDefault(s => s.Id == parent.Id);
            var targetSection = data.TableSheet.FirstOrDefault(s => s.Id == section.Id);

            if (parentSection == null || targetSection == null)
                return;

            // Remove from parent
            if (!parentSection.Items.Remove(item))
                return;

            item.Parent = targetSection;
            targetSection.Items.Add(item);

            // Update in each language
            foreach (var lang in data.Languages)
            {
                var langParentSection = lang.TableSheet.FirstOrDefault(ts => ts.Reference == parentSection);
                var langTargetSection = lang.TableSheet.FirstOrDefault(ts => ts.Reference == targetSection);

                if (langParentSection == null || langTargetSection == null)
                    continue;

                // Remove the corresponding temp item from the parent section
                var tempItem = langParentSection.Items.FirstOrDefault(i => i.Reference == item);
                if (tempItem == null)  continue;

                langParentSection.Items.Remove(tempItem);
                langTargetSection.Items.Add(tempItem);
            }
        }

        /// <summary>
        /// Moves a section to a new position in the TableSheet list.
        /// </summary>
        public static void OnMoveSection(this LocalizationWindowData data, SheetSectionTreeView section, int position)
        {
            var sectionData = data.TableSheet.FirstOrDefault(s => s.Id == section.Id);
            if (sectionData == null)
                return;

            int oldIndex = data.TableSheet.IndexOf(sectionData);
            if (oldIndex < 0)
                return;

            // Clamp position
            int insertTo = position > oldIndex ? position - 1 : position;
            insertTo = Mathf.Clamp(insertTo, 0, sectionData.Items.Count);

            if (oldIndex == insertTo)
                return;

            data.TableSheet.RemoveAt(oldIndex);
            data.TableSheet.Insert(insertTo, sectionData);

            // Update in each language
            foreach (var lang in data.Languages)
            {
                var langSection = lang.TableSheet.FirstOrDefault(ts => ts.Reference == sectionData);
                if (langSection == null) continue;

                int langOldIndex = lang.TableSheet.IndexOf(langSection);
                if (langOldIndex < 0) continue;

                // Clamp position
                int insertToLang = position > langOldIndex ? position - 1 : position;
                insertToLang = Mathf.Clamp(insertTo, 0, lang.TableSheet.Count);

                if (langOldIndex == position)
                    continue;

                lang.TableSheet.RemoveAt(langOldIndex);
                lang.TableSheet.Insert(insertToLang, langSection);
            }
        }
    }
}