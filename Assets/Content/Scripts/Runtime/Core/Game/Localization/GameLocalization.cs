using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Scriptable;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/localization")]
    public class GameLocalization : Singleton<GameLocalization>
    {
        public GameLocaizationTable LocalizationTable;
        public int DefaultLanguage = 0;
        public bool ShowWarnings = true;

        public CompositeDisposable Disposables = new();

        private readonly BehaviorSubject<Unit> OnLanguageChange = new(Unit.Default);

        private int currentLanguageIndex;
        public int CurrentLanguageIndex => currentLanguageIndex;

        private IDictionary<string, string> _languageDict;
        public IDictionary<string, string> LanguageDict
        {
            get => _languageDict ??= GenerateLangDictionary(DefaultLanguage);
            private set => _languageDict = value;
        }

        /// <summary>
        /// Generates the Language Dictionary based on the specified localization asset.
        /// </summary>
        private IDictionary<string, string> GenerateLangDictionary(int language)
        {
            Dictionary<string, string> _dict = new();

            // Ensure the localization table is not null
            if (LocalizationTable == null || LocalizationTable.Languages.Count == 0)
            {
                Debug.LogError("Localization table is empty or null. Make sure that at least one language is added to the localization table and that it's not null.");
                return _dict;
            }

            // Ensure the provided language index is within bounds
            if (language < 0 || language >= LocalizationTable.Languages.Count)
            {
                Debug.LogError($"Invalid language index [{language}]. Make sure the default language is assigned to the correct language.");
                return _dict;
            }

            LocalizationLanguage currentLanguage = LocalizationTable.Languages[language];

            // Ensure the provided language is not null
            if (currentLanguage == null)
            {
                Debug.LogError("Selected language is null. Make sure that the selected language index is assigned to the localization table.");
                return _dict;
            }
            else
            {
                currentLanguageIndex = language;
                foreach (var langData in currentLanguage.Strings)
                {
                    if (string.IsNullOrEmpty(langData.Key) || string.IsNullOrEmpty(langData.Value))
                        continue;

                    string key = langData.Key.Replace(" ", "");
                    if (_dict.ContainsKey(key))
                    {
                        Debug.LogError($"Key with the same name has already been added. Key: {key}");
                        continue;
                    }

                    _dict.Add(key, langData.Value);
                }
            }

            return _dict;
        }

        public LocalizationLanguage GetDefaultLocalization()
        {
            int language = DefaultLanguage;

            if (LocalizationTable == null || LocalizationTable.Languages.Count == 0)
            {
                Debug.LogError("Localization table is empty or null.");
                return null;
            }

            // Ensure the provided language index is within bounds
            if (language < 0 || language >= LocalizationTable.Languages.Count)
            {
                Debug.LogWarning("Invalid language index. Falling back to first language.");
                language = 0; // Default to the first language
            }

            return LocalizationTable.Languages[language];
        }

        /// <summary>
        /// Observes a localized string for the given localization key.
        /// <br>Updates automatically when the language changes.</br>
        /// </summary>
        /// <param name="key">Localization key</param>
        /// <returns>An observable string representing the localized value.</returns>
        public IObservable<string> ObserveGloc(string key)
        {
            return OnLanguageChange
                .Select(_ => GetLocalizedString(key)); // Get updated value
        }

        /// <summary>
        /// Gets a localized string for the localization key.
        /// </summary>
        private string GetLocalizedString(string key)
        {
            if (LanguageDict.TryGetValue(key, out string text))
                return text;

            return $"[Missing {key}]"; // Fallback for missing entries
        }

        /// <summary>
        /// Updates the current language and regenerates the language dictionary.
        /// <br>Notifies all observers that the language has changed.</br>
        /// </summary>
        /// <param name="newLanguageIndex">The index of the new language in the table.</param>
        public void ChangeLanguage(int newLanguageIndex)
        {
            if (LocalizationTable == null || newLanguageIndex < 0 || newLanguageIndex >= LocalizationTable.Languages.Count)
            {
                Debug.LogError("Invalid language index.");
                return;
            }

            LanguageDict = GenerateLangDictionary(newLanguageIndex);
            OnLanguageChange.OnNext(Unit.Default); // Notify observers
        }
    }

    public static class GameLocalizationE
    {
        /// <summary>
        /// Get gloc and return a formatted string with the input glyph.
        /// </summary>
        /// <param name="format">Format in which to return the string. <br>Example: "{0} [gloc.key]"</br></param>
        /// <remarks>Useful if you want to get a formatted string in the Update() function.</remarks>
        public static string WithGloc(this InputManager.BindingPath bindingPath, string format)
        {
            Regex regex = new Regex(@"\[(.*?)\]");
            Match match = regex.Match(format);
            if (!match.Success) throw new ArgumentNullException("Could not find the gloc key in [] brackets.");

            string glocKey = regex.Match(format).Groups[1].Value;
            string newFormat = regex.Replace(format, "{1}");

            if (GameLocalization.Instance.LanguageDict.TryGetValue(glocKey, out string text))
            {
                string glyphFormat = string.Format(newFormat, "{0}", text);
                return bindingPath.Format(glyphFormat);
            }

            throw new NullReferenceException($"Could not find gloc with key '{glocKey}'");
        }

        /// <summary>
        /// Subscribe to listening for a binding path with localized string changes.
        /// </summary>
        /// <param name="key">Game localization key in format "section.key".</param>
        /// <param name="format">Format of the resulting text. Example: "{0} {1}"</param>
        /// <param name="onUpdate">Action when binding path or localization is changed and text is updated.</param>
        public static void SubscribeGlyphGloc(this InputManager.BindingPath bindingPath, string key, string format, Action<string> onUpdate)
        {
            string bindingGlyph = bindingPath.inputGlyph.GlyphPath;
            string glocText = key;

            // observe binding glyph changes
            bindingPath.ObserveGlyphPath(glyph =>
            {
                bindingGlyph = glyph;
                string formattedString = string.Format(format, bindingGlyph, glocText);
                onUpdate?.Invoke(formattedString);
            });

            // observe gloc string changes
            SubscribeGloc(key, text =>
            {
                glocText = text;
                string formattedString = string.Format(format, bindingGlyph, glocText);
                onUpdate?.Invoke(formattedString);
            });
        }

        /// <summary>
        /// Subscribe to listening for a binding path with localized string changes.
        /// </summary>
        /// <param name="format">Format of the resulting text. Example: "{0} [gloc.key]"</param>
        /// <param name="onUpdate">Action when binding path or localization is changed and text is updated.</param>
        public static void SubscribeGlyphGloc(this InputManager.BindingPath bindingPath, string format, Action<string> onUpdate)
        {
            Regex regex = new Regex(@"\[(.*?)\]");
            Match match = regex.Match(format);
            if (!match.Success) throw new ArgumentNullException("Could not find the gloc key in [] brackets.");

            string glocKey = regex.Match(format).Groups[1].Value;
            string newFormat = regex.Replace(format, "{1}");

            string bindingGlyph = bindingPath.inputGlyph.GlyphPath;
            string glocText = glocKey;

            // observe binding glyph changes
            bindingPath.ObserveGlyphPath(glyph =>
            {
                bindingGlyph = glyph;
                string formattedString = string.Format(newFormat, bindingGlyph, glocText);
                onUpdate?.Invoke(formattedString);
            });

            // observe gloc string changes
            SubscribeGloc(glocKey, text =>
            {
                glocText = text;
                string formattedString = string.Format(newFormat, bindingGlyph, glocText);
                onUpdate?.Invoke(formattedString);
            });
        }

        /// <summary>
        /// Subscribe to listening for a localized string changes. The result of the localized text may contain actions in the format "[action]" to subscribe to listen for changes to the action binding path. 
        /// </summary>
        /// <param name="key">Game localization key in format "section.key".</param>
        /// <param name="onUpdate">Action when localization is changed and text is updated.</param>
        /// <remarks>Useful if you have text that you want to localize, but you also want to display actions in it. For example: "Press [action1] or [action2] to do something."</remarks>
        public static void SubscribeGlocMany(this string key, Action<string> onUpdate, bool observeBinding = true)
        {
            ReactiveDisposable disposables = new();

            // observe gloc string changes
            SubscribeGloc(key, text =>
            {
                if (string.IsNullOrEmpty(text))
                    return;

                // dispose old subscribed binding changes
                disposables.Dispose();

                bool bindingPathSubscribed = false;
                Regex regex = new Regex(@"\[(.*?)\]");
                MatchCollection matches = regex.Matches(text);
                string[] bindingGlyphs = new string[matches.Count];
                string formatText = text;

                if (matches.Count > 0)
                {
                    var matchesArray = matches.ToArray();
                    foreach (Match match in matchesArray)
                    {
                        string group = match.Groups[0].Value;
                        string action = match.Groups[1].Value;

                        if (!InputManager.HasReference)
                        {
                            Debug.LogError("Reference to InputManager was not found!");
                            continue;
                        }

                        var bindingPath = InputManager.GetBindingPath(action);
                        if (bindingPath == null) continue;

                        int index = Array.IndexOf(matchesArray, match);
                        formatText = formatText.Replace(group, "{" + index + "}");

                        // observe binding path changes
                        if (observeBinding)
                        {
                            disposables.Add(bindingPath.GlyphPathObservable.Subscribe(glyphPath =>
                            {
                                bindingGlyphs[index] = glyphPath;
                                if (bindingPathSubscribed)
                                {
                                    string formattedString = string.Format(formatText, bindingGlyphs);
                                    onUpdate?.Invoke(formattedString);
                                }
                            }));
                        }
                        else
                        {
                            bindingGlyphs[index] = bindingPath.inputGlyph.GlyphPath;
                        }
                    }

                    bindingPathSubscribed = true;
                }

                string formattedString = string.Format(formatText, bindingGlyphs);
                onUpdate?.Invoke(formattedString);
            });
        }

        /// <summary>
        /// Subscribe to listening for a localized string changes.
        /// </summary>
        /// <param name="key">Game localization key in format "section.key"</param>
        /// <param name="onUpdate">Action when localization is changed and text is updated.</param>
        public static void SubscribeGloc(this string key, Action<string> onUpdate, bool updateWhenNull = true)
        {
#if UHFPS_LOCALIZATION
            if (!GameLocalization.HasReference || string.IsNullOrEmpty(key))
            {
                onUpdate?.Invoke(key);
                return;
            }

            GameLocalization localization = GameLocalization.Instance;
            CompositeDisposable disposables = localization.Disposables;

            if (localization.LanguageDict.ContainsKey(key))
            {
                disposables.Add(localization.ObserveGloc(key).Subscribe(text =>
                {
                    onUpdate?.Invoke(text);
                }));
            }
            else if(updateWhenNull)
            {
                if (localization.ShowWarnings)
                    Debug.LogWarning($"The localization key named \"{key}\" is not found in the dictionary. The key will be used as normal text. Consider inserting an asterisk (*) before the key name to prevent searching for the value and displaying this message.");

                onUpdate?.Invoke(key);
            }
#else
            onUpdate?.Invoke(key);
#endif
        }
    }
}