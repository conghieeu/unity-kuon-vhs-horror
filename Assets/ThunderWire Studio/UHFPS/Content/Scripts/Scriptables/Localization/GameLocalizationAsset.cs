using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UHFPS.Scriptable
{
    public class GameLocalizationAsset : ScriptableObject
    {
        [Serializable]
        public struct Localization
        {
            public string Key;
            [TextArea(1, 6)]
            public string Text;
        }

        [Serializable]
        public struct LocalizationSection
        {
            public string Section;
            public List<Localization> Localizations;
        }

        public string LanguageName;
        public List<LocalizationSection> Localizations = new();

        public void RemoveSection(string section)
        {
            for (int i = 0; i < Localizations.Count; i++)
            {
                if (Localizations[i].Section == section)
                {
                    Localizations.RemoveAt(i);
                    break;
                }
            }
        }

        public void AddSectionKey(string key, string text)
        {
            var split = key.Split('.');
            string section = split[0];
            string newKey = string.Join(".", split.Skip(1));

            if(!Localizations.Any(x => x.Section == section))
            {
                Localizations.Add(new()
                {
                    Section = section,
                    Localizations = new()
                    {
                        new()
                        {
                            Key = newKey,
                            Text = text
                        }
                    }
                });
            }
            else
            {
                foreach (var localizationSection in Localizations)
                {
                    if(localizationSection.Section == section)
                    {
                        if (!localizationSection.Localizations.Any(x => x.Key == newKey))
                        {
                            localizationSection.Localizations.Add(new()
                            {
                                Key = newKey,
                                Text = text
                            });

                            break;
                        }
                        else
                        {
                            throw new ArgumentException($"Key '{newKey}' already exists in the '{localizationSection.Section}' section!");
                        }
                    }
                }
            }
        }
    }
}