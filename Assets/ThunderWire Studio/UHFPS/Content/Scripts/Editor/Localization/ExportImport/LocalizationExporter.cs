using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using UnityEngine;
using CsvHelper;

namespace UHFPS.Editors
{
    public static class LocalizationExporter
    {
        public static void ExportLocalizationToCSV(LocalizationWindowData windowData, string filePath)
        {
            if (windowData == null || windowData.Languages.Count == 0)
            {
                Debug.LogError("Localization window data is empty.");
                return;
            }

            // Collect all keys from the TableSheet.
            var allKeys = new List<string>();
            foreach (var section in windowData.TableSheet)
            {
                foreach (var item in section.Items)
                {
                    string key = $"{section.Id}:{item.Id}";
                    allKeys.Add(key);
                }
            }

            // Build a language-specific lookup for translations.
            var languageLookups = windowData.Languages.ToDictionary(
                lang => lang.Entry.LanguageName,
                lang => lang.TableSheet.SelectMany(section => section.Items)
                                       .ToDictionary(item => $"{item.Parent.Id}:{item.Id}", item => item.Value)
            );

            // Write the CSV file.
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteField("Key");
                windowData.Languages.ForEach(x => csv.WriteField(x.Entry.LanguageName));
                csv.NextRecord();

                // Write each translation row.
                foreach (var key in allKeys)
                {
                    csv.WriteField(key);
                    foreach (var lang in windowData.Languages)
                    {
                        var translations = languageLookups[lang.Entry.LanguageName];
                        string value = translations.ContainsKey(key) ? translations[key] : "";
                        csv.WriteField(value);
                    }
                    csv.NextRecord();
                }
            }

            Debug.Log($"Localization exported successfully to: {filePath}");
        }

        public static void ImportLocalizationFromCSV(LocalizationWindowData windowData, string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"CSV file not found: {filePath}");
                return;
            }

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Read the header.
                csv.Read();
                csv.ReadHeader();
                var headerNames = csv.HeaderRecord;
                var languageNames = headerNames.Skip(1).ToList();

                // Map the existing languages from windowData.
                var existingLanguages = windowData.Languages
                    .Where(l => languageNames.Contains(l.Entry.LanguageName))
                    .ToDictionary(l => l.Entry.LanguageName);

                if (existingLanguages.Count == 0)
                {
                    Debug.LogError("No matching languages in the Localization Window Data.");
                    return;
                }

                // Process each record in the CSV.
                while (csv.Read())
                {
                    string key = csv.GetField("Key");
                    string[] keyParts = key.Split(':');

                    // Get sectionId and itemId
                    if (keyParts.Length != 2 ||
                        !int.TryParse(keyParts[0], out int sectionId) ||
                        !int.TryParse(keyParts[1], out int itemId))
                    {
                        Debug.LogWarning($"Invalid key format: {key}");
                        continue;
                    }

                    // Update translations for each language
                    foreach (var lang in existingLanguages)
                    {
                        // Get the localized value for this language
                        string value = csv.GetField(lang.Key);

                        // Find the matching section and item to update.
                        var matchingSection = lang.Value.TableSheet.FirstOrDefault(sec => sec.Id == sectionId);
                        var matchingItem = matchingSection?.Items.FirstOrDefault(item => item.Id == itemId);
                        if (matchingItem != null) matchingItem.Value = value;
                    }
                }
            }

            Debug.Log("Localization CSV imported successfully into existing languages.");
        }
    }
}
