using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace UHFPS.Scriptable
{
    public struct InputGlyph
    {
        public string GlyphPath;
        public Sprite GlyphSprite;
        public Vector2 GlyphScale;
    }

    [CreateAssetMenu(fileName = "InputSprites", menuName = "UHFPS/Input Sprites")]
    public class InputSpritesAsset : ScriptableObject
    {
        public const string FORMAT = "<sprite={0}>";

        public static string[] AllKeys
        {
            get
            {
                string[] keyboardKeys = Keyboard.current.allKeys.Select(x => x.path).ToArray();
                string[] mouseKeys = new string[]
                {
                    Mouse.current.leftButton.path,
                    Mouse.current.middleButton.path,
                    Mouse.current.rightButton.path,
                    Mouse.current.delta.path,
                    Mouse.current.scroll.path,
                };

                return keyboardKeys.Concat(mouseKeys).ToArray();
            }
        }
        
        [Serializable]
        public sealed class GlyphKeysPair
        {
            public TMP_SpriteGlyph Glyph;
            public string[] MappedKeys;
            public Vector2 Scale = Vector2.one;
        }

        public TMP_SpriteAsset SpriteAsset;
        public GlyphKeysPair[] GlyphMap;

        /// <summary>
        /// Get glyph path for the specified binding path.
        /// </summary>
        /// <returns>Format: &lt;sprite=0&gt;</returns>
        public string GetGlyphPath(string bindingPath)
        {
            int glyphIndex = GetGlyphIndex(bindingPath);
            return glyphIndex >= 0 
                ? string.Format(FORMAT, glyphIndex)
                : string.Empty;
        }

        /// <summary>
        /// Get glyph sprite for the specified binding path.
        /// </summary>
        public Sprite GetGlyphSprite(string bindingPath)
        {
            Sprite glyphSprite = null;

            foreach (var map in GlyphMap)
            {
                foreach (var key in map.MappedKeys)
                {
                    if (!key.Equals("invalid"))
                    {
                        string mappedBindingPath = ToBindingPath(key);
                        if (mappedBindingPath.Equals(bindingPath))
                            return map.Glyph.sprite;
                    }
                    else
                    {
                        glyphSprite = map.Glyph.sprite;
                    }
                }
            }

            return glyphSprite;
        }

        /// <summary>
        /// Get input glyph for the specified binding path.
        /// </summary>
        public InputGlyph GetInputGlyph(string bindingPath)
        {
            InputGlyph inputGlyph = new();

            foreach (var map in GlyphMap)
            {
                foreach (var key in map.MappedKeys)
                {
                    int index = (int)map.Glyph.index;

                    if (!key.Equals("invalid"))
                    {
                        string mappedBindingPath = ToBindingPath(key);
                        if (mappedBindingPath.Equals(bindingPath))
                        {
                            inputGlyph.GlyphPath = string.Format(FORMAT, index);
                            inputGlyph.GlyphSprite = map.Glyph.sprite;
                            inputGlyph.GlyphScale = map.Scale;
                            return inputGlyph;
                        }
                    }
                    else
                    {
                        inputGlyph.GlyphPath = string.Format(FORMAT, index);
                        inputGlyph.GlyphSprite = map.Glyph.sprite;
                        inputGlyph.GlyphScale = map.Scale;
                    }
                }
            }

            return inputGlyph;
        }

        private int GetGlyphIndex(string bindingPath)
        {
            int glyphIndex = -1;

            foreach (var map in GlyphMap)
            {
                foreach (var key in map.MappedKeys)
                {
                    if (!key.Equals("invalid"))
                    {
                        string mappedBindingPath = ToBindingPath(key);
                        if (mappedBindingPath.Equals(bindingPath))
                            return (int)map.Glyph.index;
                    }
                    else
                    {
                        glyphIndex = (int)map.Glyph.index;
                    }
                }
            }

            return glyphIndex;
        }

        private string ToBindingPath(string controlPath)
        {
            string[] components = controlPath[1..].Split('/');
            return $"<{components[0]}>/" + string.Join("/", components[1..]);
        }
    }
}