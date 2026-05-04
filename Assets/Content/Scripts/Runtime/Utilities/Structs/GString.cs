using System;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using UHFPS.Input;
using UnityEngine;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Represents a string that can be localized (GameLocalization) or normal.
    /// </summary>
    [Serializable]
    public sealed class GString
    {
        public const char EXCLUDE_CHAR = '*';

        public string GlocText;
        public string NormalText;

        private Subject<string> OnTextChange = new();

        public GString(string text)
        {
            OnTextChange = new();
            NormalText = text;
        }

        public GString(string gloc, string text)
        {
            OnTextChange = new();
            GlocText = gloc;
            NormalText = text;
        }

        public GString(GString copy)
        {
            OnTextChange = new();
            GlocText = copy.GlocText;
            NormalText = copy.NormalText;
        }

        public string Value
        {
            get
            {
                if(string.IsNullOrEmpty(NormalText)) 
                    return GlocText;

                return NormalText;
            }

            set
            {
                NormalText = value;
            }
        }

        public static implicit operator string(GString gstring)
        {
            if(gstring == null || string.IsNullOrEmpty(gstring.Value))
                return string.Empty;

            return gstring.Value;
        }

        public static implicit operator GString(string value)
        {
            GString result = new(value);
            return result;
        }

        /// <summary>
        /// Subscribe to listening for a localized string changes.
        /// </summary>
        public void SubscribeGloc(Action<string> onUpdate = null)
        {
            if (string.IsNullOrEmpty(Value))
                return;

#if UHFPS_LOCALIZATION
            if (GlocText.Length > 0 && GlocText[0] == EXCLUDE_CHAR)
            {
                NormalText = GlocText[1..];
                onUpdate?.Invoke(NormalText);

                OnTextChange ??= new();
                OnTextChange?.OnNext(NormalText);
                return;
            }

            GlocText.SubscribeGloc(text =>
            {
                NormalText = text;
                onUpdate?.Invoke(text);

                OnTextChange ??= new();
                OnTextChange?.OnNext(text);
            });
#endif
        }

        /// <summary>
        /// Subscribe to listening for a localized string changes. The result of the localized text may contain actions in the format "[action]" to subscribe to listen for changes to the action binding path.
        /// </summary>
        public void SubscribeGlocMany(Action<string> onUpdate = null)
        {
            if (string.IsNullOrEmpty(Value))
                return;

#if UHFPS_LOCALIZATION
            if (GlocText.Length > 0 && GlocText[0] == EXCLUDE_CHAR)
            {
                NormalText = GlocText[1..];
                onUpdate?.Invoke(NormalText);

                OnTextChange ??= new();
                OnTextChange?.OnNext(NormalText);
                return;
            }

            GlocText.SubscribeGlocMany(text =>
            {
                NormalText = text;
                onUpdate?.Invoke(text);

                OnTextChange ??= new();
                OnTextChange?.OnNext(text);
            });
#endif
        }

        /// <summary>
        /// Subscribe to listening for a binding path changes.
        /// </summary>
        public void ObserveBindingPath()
        {
            Regex regex = new Regex(@"\[(.*?)\]");
            Match match = regex.Match(NormalText);

            if (match.Success)
            {
                string actionName = match.Groups[1].Value;
                string text = NormalText;

                InputManagerE.ObserveGlyphPath(actionName, 0, glyph =>
                {
                    NormalText = regex.Replace(text, glyph);
                });
            }
        }

        public IDisposable ObserveText(Action<string> onUpdate = null)
        {
            OnTextChange ??= new();
            return OnTextChange.Subscribe(text => onUpdate?.Invoke(text));
        }

        public override string ToString() => Value;
    }
}