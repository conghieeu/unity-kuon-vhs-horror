using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using ThunderWire.Attributes;
using Object = UnityEngine.Object;
using UnityEditorInternal;

namespace ThunderWire.Editors
{
    public class MultiToolbarItem
    {
        public GUIContent content;
        public SerializedProperty property;

        public MultiToolbarItem(GUIContent content, SerializedProperty toggleProperty)
        {
            this.content = content;
            property = toggleProperty;
        }
    }

    public static class EditorDrawing
    {
        public static class Styles
        {
            public static Color? labelColor = null;

            public static GUIStyle borderBoxHeaderStyle
            {
                get
                {
                    GUIStyle style = new();
                    style.margin = new RectOffset(3, 3, 2, 2);
                    style.padding = new RectOffset(5, 5, 2, 5);
                    return style;
                }
            }

            public static GUIStyle borderBoxStyle
            {
                get
                {
                    GUIStyle style = new();
                    style.margin = new RectOffset(3, 3, 2, 2);
                    style.padding = new RectOffset(5, 5, 5, 5);
                    return style;
                }
            }

            public static GUIStyle miniBoldLabelMiddleLeft
            {
                get
                {
                    GUIStyle style = new(EditorStyles.miniBoldLabel);
                    style.alignment = TextAnchor.MiddleLeft;

                    if (labelColor.HasValue)
                        style.normal.textColor = labelColor.Value;

                    return style;
                }
            }

            public static GUIStyle boldLabelMiddleLeft
            {
                get
                {
                    GUIStyle style = new(EditorStyles.boldLabel);
                    style.alignment = TextAnchor.MiddleLeft;

                    if (labelColor.HasValue)
                        style.normal.textColor = labelColor.Value;

                    return style;
                }
            }

            public static GUIStyle miniBoldLabelCenter
            {
                get
                {
                    GUIStyle style = new(EditorStyles.miniBoldLabel);
                    style.alignment = TextAnchor.MiddleLeft;

                    if (labelColor.HasValue)
                        style.normal.textColor = labelColor.Value;

                    return style;
                }
            }

            public static GUIStyle miniBoldLabelFoldout
            {
                get
                {
                    GUIStyle style = new(EditorStyles.foldout);
                    style.font = EditorStyles.miniBoldLabel.font;
                    style.fontStyle = EditorStyles.miniBoldLabel.fontStyle;
                    style.fontSize = EditorStyles.miniBoldLabel.fontSize;
                    return style;
                }
            }

            public static GUIStyle miniBoldButton
            {
                get
                {
                    GUIStyle style = new(GUI.skin.button);
                    style.font = EditorStyles.miniBoldLabel.font;
                    style.fontStyle = EditorStyles.miniBoldLabel.fontStyle;
                    style.fontSize = EditorStyles.miniBoldLabel.fontSize;
                    return style;
                }
            }

            public static GUIStyle RichLabel
            {
                get => new(EditorStyles.label)
                {
                    richText = true
                };
            }

            public static GUIStyle RichHelpBox
            {
                get => new(EditorStyles.helpBox)
                {
                    richText = true
                };
            }

            public static Texture2D TransparentCheckerTexture
            {
                get
                {
                    if (EditorGUIUtility.isProSkin)
                    {
                        return EditorGUIUtility.LoadRequired("Previews/Textures/textureCheckerDark.png") as Texture2D;
                    }

                    return EditorGUIUtility.LoadRequired("Previews/Textures/textureChecker.png") as Texture2D;
                }
            }
        }

        public static GUIStyle CenterStyle(GUIStyle style)
        {
            return new GUIStyle(style)
            {
                alignment = TextAnchor.MiddleCenter
            };
        }

        public static GUIStyle CenterStyleLeft(GUIStyle style)
        {
            return new GUIStyle(style)
            {
                alignment = TextAnchor.MiddleLeft
            };
        }

        public class BorderBoxScope : GUI.Scope
        {
            public BorderBoxScope(bool roundedBox = true)
            {
                BeginBorderLayout(roundedBox);
            }

            public BorderBoxScope(GUIContent title, float headerHeight = 18f, bool roundedBox = true)
            {
                BeginHeaderBorderLayout(title, headerHeight, roundedBox);
            }

            protected override void CloseScope()
            {
                EndBorderHeaderLayout();
            }
        }

        public class ToggleBorderBoxScope : GUI.Scope
        {
            private readonly bool disableContent;

            public ToggleBorderBoxScope(GUIContent title, SerializedProperty toggle, float headerHeight = 18f, bool roundedBox = true, bool disableContent = true)
            {
                this.disableContent = disableContent;
                toggle.boolValue = BeginToggleBorderLayout(title, toggle.boolValue, headerHeight, roundedBox);
                if (disableContent) GUI.enabled = toggle.boolValue;
            }

            protected override void CloseScope()
            {
                if (disableContent) GUI.enabled = true;
                EndBorderHeaderLayout();
            }
        }

        public class IconSizeScope : GUI.Scope
        {
            private Vector2 prevIconSize;

            public IconSizeScope(Vector2 iconSize)
            {
                prevIconSize = EditorGUIUtility.GetIconSize();
                EditorGUIUtility.SetIconSize(iconSize);
            }

            public IconSizeScope(float iconSize)
            {
                prevIconSize = EditorGUIUtility.GetIconSize();
                EditorGUIUtility.SetIconSize(new Vector2(iconSize, iconSize));
            }

            public IconSizeScope(float x, float y)
            {
                prevIconSize = EditorGUIUtility.GetIconSize();
                EditorGUIUtility.SetIconSize(new Vector2(x, y));
            }

            protected override void CloseScope()
            {
                EditorGUIUtility.SetIconSize(prevIconSize);
            }
        }

        public class BackgroundColorScope : GUI.Scope
        {
            private Color prevColor;

            public BackgroundColorScope(Color backgroundColor)
            {
                prevColor = GUI.backgroundColor;
                GUI.backgroundColor = backgroundColor;
            }

            public BackgroundColorScope(string htmlColor)
            {
                prevColor = GUI.backgroundColor;
                if(ColorUtility.TryParseHtmlString(htmlColor, out Color bgColor))
                    GUI.backgroundColor = bgColor;
            }

            protected override void CloseScope()
            {
                GUI.backgroundColor = prevColor;
            }
        }

        /// <summary>
        /// Set custom icon size.
        /// </summary>
        public static Vector2 SetIconSize(float iconSize)
        {
            Vector2 prevIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(iconSize, iconSize));
            return prevIconSize;
        }

        /// <summary>
        /// Reset custom icon size.
        /// </summary>
        public static void ResetIconSize()
        {
            EditorGUIUtility.SetIconSize(Vector2.zero);
        }

        /// <summary>
        /// Set the header label text color.
        /// </summary>
        public static void SetLabelColor(Color color)
        {
            Styles.labelColor = color;
        }

        /// <summary>
        /// Set the header label text color.
        /// </summary>
        public static void SetLabelColor(string htmlColor)
        {
            if(ColorUtility.TryParseHtmlString(htmlColor, out Color color))
                Styles.labelColor = color;
        }

        /// <summary>
        /// Reset the header label text color.
        /// </summary>
        public static void ResetLabelColor()
        {
            Styles.labelColor = null;
        }

        /// <summary>
        /// Get GUIContent with specified icon.
        /// </summary>
        public static GUIContent IconContent(string iconName, float iconSize = 16f)
        {
            SetIconSize(iconSize);
            return EditorGUIUtility.TrIconContent(iconName);
        }

        /// <summary>
        /// Get GUIContent with specified icon and text.
        /// </summary>
        public static GUIContent IconTextContent(string text, string iconName, float iconSize = 16f)
        {
            SetIconSize(iconSize);
            return EditorGUIUtility.TrTextContentWithIcon(" " + text, iconName);
        }

        /// <summary>
        /// Get GUIContent with specified icon and text.
        /// </summary>
        public static GUIContent IconTextContent(string text, Texture2D texture, float iconSize = 16f)
        {
            SetIconSize(iconSize);
            return EditorGUIUtility.TrTextContentWithIcon(" " + text, texture);
        }

        /// <summary>
        /// Classic HelpBox but with Rich Text.
        /// </summary>
        public static void RichHelpBox(string message, MessageType type)
        {
            GUIContent content = new(message);
            content.image = GetHelpIcon(type);
            EditorGUILayout.LabelField(GUIContent.none, content, Styles.RichHelpBox);
        }

        private static Texture2D GetHelpIcon(MessageType type)
        {
            Type editorGUIUtilityType = typeof(EditorGUIUtility);
            MethodInfo method = editorGUIUtilityType.GetMethod("GetHelpIcon",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            if (method != null)
            {
                return method.Invoke(null, new object[] { type }) as Texture2D;
            }

            Debug.LogError("GetHelpIcon method not found!");
            return null;
        }

        /// <summary>
        /// Draw custom border box.
        /// </summary>
        public static void DrawBorderBox(Rect rect, RectOffset border, Color color)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Color orgColor = GUI.color;

            GUI.color *= color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, border.top), EditorGUIUtility.whiteTexture); //top
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - border.bottom, rect.width, border.bottom), EditorGUIUtility.whiteTexture); //bottom
            GUI.DrawTexture(new Rect(rect.x, rect.y + border.left, border.left, rect.height - 2 * border.left), EditorGUIUtility.whiteTexture); //left
            GUI.DrawTexture(new Rect(rect.xMax - border.right, rect.y + border.right, border.right, rect.height - 2 * border.right), EditorGUIUtility.whiteTexture); //right

            GUI.color = orgColor;
        }

        /// <summary>
        /// Draw the heading at the top of the inspector.
        /// </summary>
        public static void DrawInspectorHeader(GUIContent title, Object script = null)
        {
            GUIStyle headerStyle = new(EditorStyles.boldLabel)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter
            };

            headerStyle.normal.textColor = Color.white;
            title.text = title.text.ToUpper();

            using (new IconSizeScope(13))
            {
                Rect rect = GUILayoutUtility.GetRect(1, 30);
                ColorUtility.TryParseHtmlString("#181818", out Color color);

                EditorGUI.DrawRect(rect, color);
                //EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), Color.white.Alpha(0.4f));
                //EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), Color.white.Alpha(0.4f));
                //DrawCorners(rect, Vector2.one * 5, 1, Color.white.Alpha(0.75f));

                Texture2D mask = Resources.Load<Texture2D>("EditorIcons/mask");
                GUI.DrawTexture(new Rect(rect.x, rect.yMax - 1, rect.width, 1), mask);

                if (script != null)
                {
                    MonoScript monoScript = null;

                    if (script is MonoBehaviour monoBehaviour)
                        monoScript = MonoScript.FromMonoBehaviour(monoBehaviour);
                    else if (script is ScriptableObject scriptableObject)
                        monoScript = MonoScript.FromScriptableObject(scriptableObject);

                    DocsAttribute docsAttribute = script.GetType().GetCustomAttribute<DocsAttribute>(true);
                    Event e = Event.current;

                    Rect pingRect = rect;
                    Rect docsIconRect = rect;
                    Rect saveableIconRect = rect;

                    docsIconRect.xMin = docsIconRect.xMax - EditorGUIUtility.singleLineHeight - 3f;
                    docsIconRect.y += 7f;
                    docsIconRect.width = EditorGUIUtility.singleLineHeight;
                    docsIconRect.height = EditorGUIUtility.singleLineHeight;

                    saveableIconRect.xMin = saveableIconRect.xMax - (EditorGUIUtility.singleLineHeight * 2) - 3f;
                    saveableIconRect.y += 6f;
                    saveableIconRect.width = EditorGUIUtility.singleLineHeight;
                    saveableIconRect.height = EditorGUIUtility.singleLineHeight;

                    if (docsAttribute != null) pingRect.xMax = saveableIconRect.xMin - 2f;

                    if (pingRect.Contains(e.mousePosition))
                    {
                        if (e.type == EventType.MouseDown && e.button == 0)
                        {
                            EditorGUIUtility.PingObject(monoScript);
                        }
                    }

                    // draw docs icon link
                    if (docsAttribute != null)
                    {
                        using (new IconSizeScope(16))
                        {
                            Texture2D docsTex = Resources.Load<Texture2D>("EditorIcons/manual");
                            GUIContent docsIcon = EditorGUIUtility.TrIconContent(docsTex, "Documentation");

                            if (GUI.Button(docsIconRect, docsIcon, EditorStyles.iconButton))
                            {
                                Application.OpenURL(docsAttribute.docsLink);
                            }
                        }
                    }
                    else
                    {
                        saveableIconRect = docsIconRect;
                        saveableIconRect.y -= 1f;
                    }

                    // draw saveable icon
                    bool flag1 = typeof(UHFPS.Runtime.ISaveable).IsAssignableFrom(script.GetType());
                    bool flag2 = typeof(UHFPS.Runtime.IRuntimeSaveable).IsAssignableFrom(script.GetType());
                    bool flag3 = typeof(UHFPS.Runtime.ISaveableCustom).IsAssignableFrom(script.GetType());

                    if (flag1 || flag2 || flag3)
                    {
                        GUIContent saveableIcon = EditorGUIUtility.TrIconContent("CacheServerConnected", "Script is Saveable");
                        EditorGUI.LabelField(saveableIconRect, saveableIcon);
                    }
                }

                EditorGUI.LabelField(rect, title, headerStyle);
            }
        }

        private static void DrawCorners(Rect rect, Vector2 cornerSize, Color cornerColor)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, cornerSize.x, cornerSize.y), cornerColor);

            // Draw top-right corner
            EditorGUI.DrawRect(new Rect(rect.xMax - cornerSize.x, rect.y, cornerSize.x, cornerSize.y), cornerColor);

            // Draw bottom-left corner
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - cornerSize.y, cornerSize.x, cornerSize.y), cornerColor);

            // Draw bottom-right corner
            EditorGUI.DrawRect(new Rect(rect.xMax - cornerSize.x, rect.yMax - cornerSize.y, cornerSize.x, cornerSize.y), cornerColor);
        }

        private static void DrawCorners(Rect rect, Vector2 cornerSize, float thickness, Color cornerColor)
        {
            // Draw top-left corner
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, cornerSize.x, thickness), cornerColor); // Horizontal part of L
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, cornerSize.y), cornerColor); // Vertical part of L

            // Draw top-right corner
            EditorGUI.DrawRect(new Rect(rect.xMax - cornerSize.x, rect.y, cornerSize.x, thickness), cornerColor); // Horizontal part of L
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, cornerSize.y), cornerColor); // Vertical part of L

            // Draw bottom-left corner
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, cornerSize.x, thickness), cornerColor); // Horizontal part of L
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - cornerSize.y, thickness, cornerSize.y), cornerColor); // Vertical part of L

            // Draw bottom-right corner
            EditorGUI.DrawRect(new Rect(rect.xMax - cornerSize.x, rect.yMax - thickness, cornerSize.x, thickness), cornerColor); // Horizontal part of L
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.yMax - cornerSize.y, thickness, cornerSize.y), cornerColor); // Vertical part of L
        }


        /// <summary>
        /// Draw classic prefix label.
        /// </summary>
        public static void DrawPrefixLabel(string title, string text, GUIStyle style)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(title);
            EditorGUILayout.LabelField(text, style);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw min-max slider from MinMax property.
        /// </summary>
        public static void DrawMinMaxSlider(SerializedProperty minMaxProperty, float minLimit, float maxLimit)
        {
            SerializedProperty minProp = minMaxProperty.FindPropertyRelative("min");
            SerializedProperty maxProp = minMaxProperty.FindPropertyRelative("max");

            float minValue = minProp.floatValue;
            float maxValue = maxProp.floatValue;
            EditorGUILayout.MinMaxSlider(minMaxProperty.displayName, ref minValue, ref maxValue, minLimit, maxLimit);

            minProp.floatValue = minValue;
            maxProp.floatValue = maxValue;
        }

        /// <summary>
        /// Draw a property field with a label after the field.
        /// </summary>
        public static void DrawPropertyAndLabel(SerializedProperty property, GUIContent content)
        {
            DrawPropertyAndLabel(property, content, EditorStyles.boldLabel);
        }

        /// <summary>
        /// Draw a property field with a label after the field.
        /// </summary>
        public static void DrawPropertyAndLabel(SerializedProperty property, GUIContent content, GUIStyle labelStyle)
        {
            Rect rect = EditorGUILayout.GetControlRect();

            float labelWidth = labelStyle.CalcSize(content).x;
            rect.xMax -= labelWidth + 2f;

            EditorGUI.PropertyField(rect, property);

            rect.xMin = rect.xMax + 2f;
            rect.xMax += labelWidth + 2f;
            EditorGUI.LabelField(rect, content, labelStyle);
        }

        /// <summary>
        /// Draw horizontal separator.
        /// </summary>
        public static void Separator(int height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        /// <summary>
        /// Draw horizontal (spaced) separator.
        /// </summary>
        public static void SeparatorSpaced(float space, int height = 1)
        {
            EditorGUILayout.Space(space);
            Separator(height);
            EditorGUILayout.Space(space);
        }

        /// <summary>
        /// Draw a large sprite selection similar to the EditorGUI.ObjectField sprite selection.
        /// <br>But with the correct, not stretched image.</br>
        /// </summary>
        public static Object DrawLargeSpriteSelector(Rect rect, Object obj)
        {
            Rect borderRect = rect;
            borderRect.height -= 1;
            borderRect.width -= 1;
            GUI.Box(borderRect, GUIContent.none, new GUIStyle("HelpBox"));

            Rect iconRect = rect;
            iconRect.height -= 10;
            iconRect.width -= 10;
            iconRect.x += 5f;
            iconRect.y += 5f;

            DrawTransparentTextureWithChecker(iconRect, obj);

            Rect mageSelectRect = rect;
            mageSelectRect.yMax -= 15f;

            Event e = Event.current;
            if (mageSelectRect.Contains(e.mousePosition))
            {
                if (e.type == EventType.MouseDown && e.button == 0 && obj != null)
                {
                    EditorGUIUtility.PingObject(obj);
                    e.Use();
                }
            }

            Rect buttonRect = rect;
            buttonRect.height = 15f;
            buttonRect.xMin = rect.width - rect.width * 0.7f;
            buttonRect.y = rect.height - 16f;
            buttonRect.x -= 1f;

            if (GUI.Button(buttonRect, "Select", EditorStyles.objectFieldThumb))
            {
                EditorGUIUtility.ShowObjectPicker<Sprite>(obj, false, "", GUIUtility.GetControlID(FocusType.Passive));
            }
            else if (Event.current.commandName == "ObjectSelectorUpdated")
            {
                return EditorGUIUtility.GetObjectPickerObject();
            }

            return obj;
        }

        /// <summary>
        /// Draw classic object field with picker.
        /// </summary>
        public static bool ObjectField(Rect rect, GUIContent text, GUIContent tooltip = null)
        {
            using (new IconSizeScope(12f))
            {
                GUI.Box(rect, text, EditorStyles.objectField);

                GUIStyle buttonStyle = new GUIStyle("ObjectFieldButton") { richText = true };
                Rect buttonRect = buttonStyle.margin.Remove(new Rect(rect.xMax - 19, rect.y, 19, rect.height));

                return GUI.Button(buttonRect, tooltip ?? new GUIContent(), buttonStyle);
            }
        }

        /// <summary>
        /// Draw a multi selection toolbar.
        /// </summary>
        public static void DrawMultiToolbar(Rect rect, MultiToolbarItem[] contents, float buttonWidth = 100) 
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            string name = style.name;

            GUIStyle midStyle = GUI.skin.FindStyle(name + "mid") ?? style;
            GUIStyle firstStyle = GUI.skin.FindStyle(name + "left") ?? midStyle;
            GUIStyle lastStyle = GUI.skin.FindStyle(name + "right") ?? midStyle;

            Rect toolbarRect = rect;
            toolbarRect.width = buttonWidth;

            for (int i = 0; i < contents.Length; i++)
            {
                if(i == 0)
                {
                    // first
                    contents[i].property.boolValue = GUI.Toggle(toolbarRect, contents[i].property.boolValue, contents[i].content, firstStyle);
                    toolbarRect.x += buttonWidth;
                }
                else if(i == contents.Length - 1)
                {
                    // last
                    contents[i].property.boolValue = GUI.Toggle(toolbarRect, contents[i].property.boolValue, contents[i].content, lastStyle);
                    toolbarRect.x += buttonWidth;
                }
                else
                {
                    // mid
                    contents[i].property.boolValue = GUI.Toggle(toolbarRect, contents[i].property.boolValue, contents[i].content, midStyle);
                    toolbarRect.x += buttonWidth;
                }
            }
        }

        /// <summary>
        /// Draw multi selection popup menu.
        /// </summary>
        public static void DrawMultiSelectPopup(Rect rect, string[] content, string[] selected, Action<string[]> onItemSelect, int maxDisplay = 3)
        {
            GenericMenu menu = new GenericMenu();
            List<string> selectedArr = new List<string>(selected);

            for (int i = 0; i < content.Length; i++)
            {
                string name = content[i];
                bool on = selected.Any(x => x.Equals(name));

                menu.AddItem(new GUIContent(name), on, data =>
                {
                    if (selectedArr.Contains(name))
                        selectedArr.Remove(name);
                    else selectedArr.Add(name);
                    onItemSelect?.Invoke(selectedArr.ToArray());
                },
                name);
            }

            string popupTitle = "Nothing";
            if (selected.Length > 0)
            {
                popupTitle = string.Join(", ", selected.Take(maxDisplay));

                if (selected.Length > maxDisplay)
                    popupTitle += " ...";
            }

            if (GUI.Button(rect, popupTitle, EditorStyles.popup))
            {
                menu.DropDown(rect);
            }
        }

        /// <summary>
        /// Draw multi selection popup menu.
        /// </summary>
        public static void DrawMultiSelectPopup(Rect rect, GUIContent title, string[] content, string[] selected, Action<string[]> onItemSelect, int maxDisplay = 3)
        {
            GenericMenu menu = new GenericMenu();
            GUIContent guiTitle = new GUIContent(title);
            List<string> selectedArr = new List<string>(selected);

            for (int i = 0; i < content.Length; i++)
            {
                string name = content[i];
                bool on = selected.Any(x => x.Equals(name));

                menu.AddItem(new GUIContent(name), on, data =>
                {
                    if (selectedArr.Contains(name))
                        selectedArr.Remove(name);
                    else selectedArr.Add(name);
                    onItemSelect?.Invoke(selectedArr.ToArray());
                },
                name);
            }

            if (selected.Length > 0)
            {
                guiTitle.text = string.Join(", ", selected.Take(maxDisplay));

                if (selected.Length > maxDisplay)
                    guiTitle.text += " ...";
            }

            if (GUI.Button(rect, guiTitle, EditorStyles.popup))
            {
                menu.DropDown(rect);
            }
        }

        /// <summary>
        /// Draw string selection popup menu.
        /// </summary>
        public static void DrawStringSelectPopup(Rect rect, string[] content, string selected, Action<string> onItemSelect)
        {
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < content.Length; i++)
            {
                string name = content[i];
                bool on = name.Equals(selected);

                menu.AddItem(new GUIContent(name), on, data =>
                {
                    string selection = (string)data;
                    string result = selected;

                    if (selected != selection)
                        result = selection;

                    onItemSelect?.Invoke(result);
                },
                name);
            }

            string popupTitle = string.IsNullOrEmpty(selected) ? "Nothing" : selected;
            if (GUI.Button(rect, popupTitle, EditorStyles.popup))
            {
                menu.DropDown(rect);
            }
        }

        /// <summary>
        /// Draw string selection popup menu.
        /// </summary>
        public static void DrawStringSelectPopup(Rect rect, GUIContent title, string[] content, string selected, Action<string> onItemSelect)
        {
            GenericMenu menu = new GenericMenu();
            GUIContent guiTitle = new GUIContent(title);

            for (int i = 0; i < content.Length; i++)
            {
                string name = content[i];
                bool on = name.Equals(selected);

                menu.AddItem(new GUIContent(name), on, data =>
                {
                    string selection = (string)data;
                    string result = selected;

                    if (selected != selection)
                        result = selection;

                    onItemSelect?.Invoke(result);
                },
                name);
            }

            if (!string.IsNullOrEmpty(selected))
                guiTitle.text = selected;

            if (GUI.Button(rect, guiTitle, EditorStyles.popup))
            {
                menu.DropDown(rect);
            }
        }

        /// <summary>
        /// Draw string selection popup menu.
        /// </summary>
        public static void DrawStringSelectPopup(GUIContent label, GUIContent title, string[] content, string selected, Action<string> onItemSelect)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, label);
            DrawStringSelectPopup(rect, title, content, selected, onItemSelect);
        }

        /// <summary>
        /// Draw string selection popup menu.
        /// </summary>
        public static void DrawStringSelectPopup(GUIContent title, string[] content, string selected, Action<string> onItemSelect)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            DrawStringSelectPopup(rect, title, content, selected, onItemSelect);
        }

        /// <summary>
        /// Draw custom class property list.
        /// </summary>
        public static void DrawList(SerializedProperty listProperty, GUIContent title, float headerHeight = 18f, bool roundedBox = true)
        {
            if(BeginFoldoutBorderLayout(listProperty, title, headerHeight, roundedBox))
            {
                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    SerializedProperty listItem = listProperty.GetArrayElementAtIndex(i);
                    var properties = GetAllProperties(listItem);
                    string elementName = "Element " + i;

                    var firstProperty = properties.First();
                    if(firstProperty.Value.propertyType == SerializedPropertyType.String)
                    {
                        string stringValue = firstProperty.Value.stringValue;
                        if (!string.IsNullOrEmpty(stringValue)) elementName = stringValue;
                    }

                    if (BeginFoldoutBorderLayout(listItem, new GUIContent(elementName), out Rect itemHeaderRect))
                    {
                        properties.DrawAll(true);
                        EndBorderHeaderLayout();
                    }

                    GUIContent minus = EditorGUIUtility.TrIconContent("Toolbar Minus");
                    Rect minusRect = itemHeaderRect;
                    minusRect.xMin = minusRect.xMax - EditorGUIUtility.singleLineHeight;
                    minusRect.x -= 3f;
                    minusRect.y += 3f;

                    if (GUI.Button(minusRect, minus, EditorStyles.iconButton))
                    {
                        listProperty.DeleteArrayElementAtIndex(i);
                    }
                }

                if (listProperty.arraySize > 0)
                    EditorGUILayout.Space(2f);

                if (GUILayout.Button("Add"))
                {
                    listProperty.arraySize++;
                }

                EndBorderHeaderLayout();
            }
        }

        public static int BeginDrawCustomList(SerializedProperty listProperty, GUIContent title)
        {
            int arraySize = listProperty.arraySize;

            EditorGUILayout.BeginVertical(GUI.skin.box);

            Rect labelRect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);

            Rect countLabelRect = labelRect;
            countLabelRect.xMin = countLabelRect.xMax - 25f;

            GUI.enabled = false;
            EditorGUI.IntField(countLabelRect, arraySize);
            GUI.enabled = true;

            EditorGUILayout.Space(2f);

            return arraySize;
        }

        public static void EndDrawCustomList(GUIContent buttonTitle, bool buttonEnabled, Action onClick)
        {
            EditorGUILayout.Space(2f);
            Separator();
            EditorGUILayout.Space(2f);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                float width = GUI.skin.button.CalcSize(buttonTitle).x + 10f;
                Rect moduleButtonRect = EditorGUILayout.GetControlRect(GUILayout.Width(width), GUILayout.Height(20f));

                using (new EditorGUI.DisabledGroupScope(Application.isPlaying || !buttonEnabled))
                {
                    if (GUI.Button(moduleButtonRect, buttonTitle))
                    {
                        onClick?.Invoke();
                    }
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Setup reorderable list for easy drawing.
        /// </summary>
        public static void SetupReorderableList(ReorderableList reorderableList)
        {
            SerializedProperty properties = reorderableList.serializedProperty;

            reorderableList.elementHeightCallback += (int index) =>
            {
                SerializedProperty element = properties.GetArrayElementAtIndex(index);

                int fieldCount = element.CountProperty();
                if (element.isExpanded) return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (fieldCount + 1);

                return EditorGUIUtility.singleLineHeight;
            };

            reorderableList.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = properties.GetArrayElementAtIndex(index);

                rect.y += 1f;
                rect.height = EditorGUIUtility.singleLineHeight;
                Rect propertyRect = new(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                propertyRect.xMin += 12f;

                EditorGUI.PropertyField(propertyRect, element);

                if (element.isExpanded)
                {
                    EditorGUI.indentLevel++;

                    SerializedProperty childProperty = element.Copy();
                    SerializedProperty endProperty = element.GetEndProperty();

                    childProperty.NextVisible(true);
                    while (!SerializedProperty.EqualContents(childProperty, endProperty))
                    {
                        rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), childProperty);
                        childProperty.NextVisible(false);
                    }

                    EditorGUI.indentLevel--;
                }
            };
        }


        /// <summary>
        /// Get count of properties.
        /// </summary>
        public static int CountProperty(this SerializedProperty property)
        {
            SerializedProperty copy = property.Copy();
            SerializedProperty endProperty = property.GetEndProperty();
            int count = 0;

            copy.NextVisible(true);
            while (!SerializedProperty.EqualContents(copy, endProperty))
            {
                count++;
                copy.NextVisible(false);
            }

            return count;
        }

        /// <summary>
        /// Draw header by specified rect.
        /// </summary>
        public static void DrawHeader(Rect headerRect, GUIContent title, GUIStyle labelStyle = null)
        {
            Color headerColor = new Color(0.1f, 0.1f, 0.1f, 0.4f);
            EditorGUI.DrawRect(headerRect, headerColor);

            Rect labelRect = new Rect(headerRect.x + 4f, headerRect.y - 1f, headerRect.width - 4f, headerRect.height);
            EditorGUI.LabelField(labelRect, title, labelStyle ?? Styles.miniBoldLabelCenter);
        }

        /// <summary>
        /// Draw toggle header by specified rect.
        /// </summary>
        public static bool DrawToggleHeader(Rect headerRect, GUIContent title, bool toggle)
        {
            Color headerColor = new Color(0.1f, 0.1f, 0.1f, 0.4f);
            EditorGUI.DrawRect(headerRect, headerColor);

            Rect toggleRect = new Rect(headerRect.x + 4f, headerRect.y, EditorGUIUtility.singleLineHeight, headerRect.height);
            toggle = GUI.Toggle(toggleRect, toggle, new GUIContent("", "Enabled"), EditorStyles.toggle);

            Rect labelRect = new Rect(toggleRect.xMax, headerRect.y - 1f, headerRect.width - toggleRect.xMax, headerRect.height);
            EditorGUI.LabelField(labelRect, title, Styles.miniBoldLabelCenter);

            return toggle;
        }

        /// <summary>
        /// Draw foldout header by specified rect.
        /// </summary>
        public static bool DrawFoldoutHeader(Rect headerRect, GUIContent title, bool expanded)
        {
            // Constants
            Color headerColor = new Color(0.1f, 0.1f, 0.1f, 0.4f);
            float singleLineHeight = EditorGUIUtility.singleLineHeight;

            // Draw header background
            EditorGUI.DrawRect(headerRect, headerColor);

            // Define and draw foldout toggle
            Rect foldoutRect = new Rect(headerRect.x + 4f, headerRect.y, singleLineHeight, headerRect.height);
            GUI.Toggle(foldoutRect, expanded, GUIContent.none, EditorStyles.foldout);

            // Define and draw title label
            Rect labelRect = new Rect(foldoutRect.xMax, headerRect.y - 1f, headerRect.width - foldoutRect.xMax + 4f, headerRect.height);
            EditorGUI.LabelField(labelRect, title, Styles.miniBoldLabelCenter);

            // Handle mouse events for foldout interaction
            headerRect.xMax -= singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            Event e = Event.current;
            if (headerRect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
            {
                expanded = !expanded;
                e.Use();
            }

            return expanded;
        }

        /// <summary>
        /// Draw foldout header by specified rect.
        /// </summary>
        public static bool DrawFoldoutHeader(Rect headerRect, GUIContent title, float minusWidth, bool expanded)
        {
            // Constants
            Color headerColor = new Color(0.1f, 0.1f, 0.1f, 0.4f);
            float singleLineHeight = EditorGUIUtility.singleLineHeight;

            // Draw header background
            EditorGUI.DrawRect(headerRect, headerColor);

            // Draw foldout
            Rect foldoutRect = new Rect(headerRect.x + 4f, headerRect.y, singleLineHeight, headerRect.height);
            GUI.Toggle(foldoutRect, expanded, GUIContent.none, EditorStyles.foldout);

            // Draw title
            Rect labelRect = new Rect(foldoutRect.xMax, headerRect.y - 1f, headerRect.width - foldoutRect.xMax, headerRect.height);
            EditorGUI.LabelField(labelRect, title, Styles.miniBoldLabelCenter);

            // Handle mouse events
            headerRect.xMax -= singleLineHeight + EditorGUIUtility.standardVerticalSpacing + minusWidth;
            Event e = Event.current;
            if (headerRect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
            {
                expanded = !expanded;
                e.Use();
            }

            return expanded;
        }

        /// <summary>
        /// Draw foldout toggle header by specified rect.
        /// </summary>
        public static void DrawFoldoutToggleHeader(Rect headerRect, GUIContent title, ref bool expanded, ref bool toggle, bool canExpand = true)
        {
            // Constants
            float singleLineHeight = EditorGUIUtility.singleLineHeight;
            Color headerColor = new Color(0.1f, 0.1f, 0.1f, 0.4f);

            // Draw header background
            EditorGUI.DrawRect(headerRect, headerColor);

            // Set up initial positions
            Rect foldoutRect = headerRect;
            foldoutRect.width = singleLineHeight;
            foldoutRect.x += 4f;

            // If expandable, draw foldout and adjust for toggle position
            if (canExpand)
            {
                GUI.Toggle(foldoutRect, expanded, GUIContent.none, EditorStyles.foldout);
                foldoutRect.x += singleLineHeight;
            }

            // Draw toggle
            Rect toggleRect = new Rect(foldoutRect.x, headerRect.y, singleLineHeight, headerRect.height);
            toggle = GUI.Toggle(toggleRect, toggle, new GUIContent("", "Enabled"), EditorStyles.toggle);

            // Draw title
            Rect labelRect = new Rect(toggleRect.xMax, headerRect.y - 1f, headerRect.width - toggleRect.xMax, headerRect.height);
            EditorGUI.LabelField(labelRect, title, Styles.miniBoldLabelCenter);

            // Handle events
            headerRect.xMax -= singleLineHeight + 2f;
            Event e = Event.current;
            if (canExpand && headerRect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
            {
                expanded = !expanded;
                e.Use();
            }
        }

        /// <summary>
        /// Draw header with border.
        /// </summary>
        public static Rect DrawHeaderWithBorder(ref Rect rect, GUIContent title, float headerHeight = 18f, bool roundedBox = true)
        {
            GUI.Box(rect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
            rect.x += 1;
            rect.y += 1;
            rect.height -= 1;
            rect.width -= 2;

            Rect headerRect = rect;
            headerRect.height = headerHeight + EditorGUIUtility.standardVerticalSpacing;

            rect.y += headerRect.height;
            rect.height -= headerRect.height;

            Rect titleRect = headerRect;
            titleRect.x += 2f;

            using (new IconSizeScope(14))
            {
                EditorGUI.DrawRect(headerRect, new Color(0.1f, 0.1f, 0.1f, 0.4f));
                EditorGUI.LabelField(titleRect, title, EditorStyles.miniBoldLabel);
            }

            return headerRect;
        }

        /// <summary>
        /// Draw header with border.
        /// </summary>
        public static Rect DrawHeaderWithBorder(GUIContent title, float headerHeight = 18f, bool roundedBox = true)
        {
            // Calculate the rectangle for the header based on the control rect
            Rect rect = EditorGUILayout.GetControlRect(false, headerHeight);

            // Adjust the rect for the border drawing
            Rect borderRect = rect;
            borderRect.height = headerHeight + EditorGUIUtility.standardVerticalSpacing;

            // Draw the border box
            GUI.Box(borderRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));

            // Adjust the rect for the title inside the header
            Rect titleRect = rect;
            titleRect.x += 4f;
            titleRect.y += 2f;
            titleRect.height = headerHeight;

            // Draw the title background
            EditorGUI.DrawRect(borderRect, new Color(0.1f, 0.1f, 0.1f, 0.4f));

            // Draw the title text
            using (new IconSizeScope(14))
            {
                EditorGUI.LabelField(titleRect, title, EditorStyles.miniBoldLabel);
            }

            // Return the rect for further custom drawing if needed
            return rect;
        }

        /// <summary>
        /// Begin a bordered vertical group;
        /// </summary>
        public static void BeginBorderLayout(bool roundedBox = true)
        {
            Rect drawingRect = EditorGUILayout.BeginVertical(Styles.borderBoxStyle);
            GUI.Box(drawingRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
        }

        /// <summary>
        /// Begin a bordered vertical header group;
        /// </summary>
        public static Rect BeginHeaderBorderLayout(GUIContent title, float headerHeight = 18f, bool roundedBox = true)
        {
            Rect headerRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect drawingRect = EditorGUILayout.BeginVertical(Styles.borderBoxHeaderStyle);
            drawingRect.yMin -= headerHeight + 6f;

            GUI.Box(drawingRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
            DrawHeader(headerRect, title);
            return headerRect;
        }

        /// <summary>
        /// Begin a bordered vertical foldout group.
        /// </summary>
        public static bool BeginFoldoutBorderLayout(GUIContent title, ref bool expanded, float headerHeight = 18f, bool roundedBox = true)
        {
            Rect headerRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect boxRect = headerRect;
            bool foldoutResult = expanded;

            if (expanded)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(Styles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            GUI.Box(boxRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
            expanded = DrawFoldoutHeader(headerRect, title, expanded);
            return foldoutResult;
        }

        /// <summary>
        /// Begin a bordered vertical foldout group.
        /// </summary>
        public static bool BeginFoldoutBorderLayout(GUIContent title, bool expanded, float headerHeight = 18f, bool roundedBox = true)
        {
            Rect headerRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect boxRect = headerRect;

            if (expanded)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(Styles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            GUI.Box(boxRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
            return DrawFoldoutHeader(headerRect, title, expanded);
        }

        /// <summary>
        /// Begin a bordered vertical foldout group.
        /// </summary>
        public static bool BeginFoldoutBorderLayout(GUIContent title, ref bool expanded, out Rect headerRect, float headerHeight = 18f, bool roundedBox = true)
        {
            headerRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect boxRect = headerRect;
            bool foldoutResult = expanded;

            if (expanded)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(Styles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            GUI.Box(boxRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
            expanded = DrawFoldoutHeader(headerRect, title, expanded);
            return foldoutResult;
        }

        /// <summary>
        /// Begin a bordered vertical foldout group.
        /// </summary>
        public static bool BeginFoldoutBorderLayout(SerializedProperty foldoutProperty, GUIContent title, float headerHeight = 18f, bool roundedBox = true)
        {
            Rect headerRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect boxRect = headerRect;
            bool foldoutResult = foldoutProperty.isExpanded;

            if (foldoutResult)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(Styles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            GUI.Box(boxRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
            foldoutProperty.isExpanded = DrawFoldoutHeader(headerRect, title, foldoutProperty.isExpanded);
            return foldoutResult;
        }

        /// <summary>
        /// Begin a bordered vertical foldout group.
        /// </summary>
        public static bool BeginFoldoutBorderLayout(SerializedProperty foldoutProperty, GUIContent title, out Rect headerRect, float headerHeight = 18f, bool roundedBox = true)
        {
            headerRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect boxRect = headerRect;
            bool foldoutResult = foldoutProperty.isExpanded;

            if (foldoutResult)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(Styles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            GUI.Box(boxRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
            foldoutProperty.isExpanded = DrawFoldoutHeader(headerRect, title, foldoutProperty.isExpanded);
            return foldoutResult;
        }

        /// <summary>
        /// Begin a bordered vertical foldout group.
        /// </summary>
        public static bool BeginFoldoutBorderLayout(SerializedProperty foldoutProperty, GUIContent title, out Rect headerRect, float minusWidth, float headerHeight = 18f, bool roundedBox = true)
        {
            headerRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect boxRect = headerRect;
            bool foldoutResult = foldoutProperty.isExpanded;

            if (foldoutResult)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(Styles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            GUI.Box(boxRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
            foldoutProperty.isExpanded = DrawFoldoutHeader(headerRect, title, minusWidth, foldoutProperty.isExpanded);
            return foldoutResult;
        }

        /// <summary>
        /// Begin a bordered vertical toggle group.
        /// </summary>
        public static bool BeginToggleBorderLayout(GUIContent title, bool toggle, float headerHeight = 18f, bool roundedBox = true)
        {
            Rect headerRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect drawingRect = EditorGUILayout.BeginVertical(Styles.borderBoxHeaderStyle);
            drawingRect.yMin -= headerHeight + 6f;

            GUI.Box(drawingRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
            return DrawToggleHeader(headerRect, title, toggle);
        }

        /// <summary>
        /// Begin a bordered vertical toggle group.
        /// </summary>
        public static bool BeginToggleBorderLayout(GUIContent title, bool toggle, out Rect headerRect, float headerHeight = 18f, bool roundedBox = true)
        {
            headerRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect drawingRect = EditorGUILayout.BeginVertical(Styles.borderBoxHeaderStyle);
            drawingRect.yMin -= headerHeight + 6f;

            GUI.Box(drawingRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
            return DrawToggleHeader(headerRect, title, toggle);
        }

        /// <summary>
        /// Begin a bordered vertical foldout toggle group.
        /// </summary>
        public static bool BeginFoldoutToggleBorderLayout(GUIContent title, ref bool expanded, ref bool toggle, float headerHeight = 18f, bool roundedBox = true, bool canExpand = true)
        {
            Rect headerRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect boxRect = headerRect;
            bool foldoutResult = canExpand && expanded;

            if (foldoutResult)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(Styles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            GUI.Box(boxRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
            DrawFoldoutToggleHeader(headerRect, title, ref expanded, ref toggle, canExpand);
            return foldoutResult;
        }

        /// <summary>
        /// Begin a bordered vertical foldout toggle group.
        /// </summary>
        public static bool BeginFoldoutToggleBorderLayout(GUIContent title, ref bool expanded, ref bool toggle, out Rect headerRect, float headerHeight = 18f, bool roundedBox = true, bool canExpand = true)
        {
            headerRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect boxRect = headerRect;
            bool foldoutResult = canExpand && expanded;

            if (foldoutResult)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(Styles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            GUI.Box(boxRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
            DrawFoldoutToggleHeader(headerRect, title, ref expanded, ref toggle, canExpand);
            return foldoutResult;
        }

        /// <summary>
        /// Begin a bordered vertical foldout toggle group.
        /// </summary>
        public static bool BeginFoldoutToggleBorderLayout(SerializedProperty foldoutProperty, GUIContent title, ref bool toggle, float headerHeight = 18f, bool roundedBox = true, bool canExpand = true)
        {
            Rect headerRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect boxRect = headerRect;

            bool expanded = canExpand && foldoutProperty.isExpanded;
            bool foldoutResult = expanded;

            if (foldoutResult)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(Styles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            GUI.Box(boxRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
            DrawFoldoutToggleHeader(headerRect, title, ref expanded, ref toggle, canExpand);
            foldoutProperty.isExpanded = expanded;
            return foldoutResult;
        }

        /// <summary>
        /// Begin a bordered vertical foldout toggle group.
        /// </summary>
        public static bool BeginFoldoutToggleBorderLayout(GUIContent title, SerializedProperty toggleProperty, float headerHeight = 18f, bool roundedBox = true, bool canExpand = true)
        {
            Rect headerRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect boxRect = headerRect;

            bool expanded = canExpand && toggleProperty.isExpanded;
            bool toggle = toggleProperty.boolValue;
            bool foldoutResult = expanded;

            if (foldoutResult)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(Styles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            GUI.Box(boxRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
            DrawFoldoutToggleHeader(headerRect, title, ref expanded, ref toggle, canExpand);
            toggleProperty.boolValue = toggle;
            toggleProperty.isExpanded = expanded;
            return foldoutResult;
        }

        /// <summary>
        /// Begin a bordered vertical foldout toggle group.
        /// </summary>
        public static bool BeginFoldoutToggleBorderLayout(GUIContent title, SerializedProperty toggleProperty, out Rect headerRect, float headerHeight = 18f, bool roundedBox = true, bool canExpand = true)
        {
            headerRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect boxRect = headerRect;

            bool expanded = canExpand && toggleProperty.isExpanded;
            bool toggle = toggleProperty.boolValue;
            bool foldoutResult = expanded;

            if (foldoutResult)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(Styles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            GUI.Box(boxRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
            DrawFoldoutToggleHeader(headerRect, title, ref expanded, ref toggle, canExpand);
            toggleProperty.boolValue = toggle;
            toggleProperty.isExpanded = expanded;
            return foldoutResult;
        }

        /// <summary>
        /// Begin a bordered vertical foldout toggle group.
        /// </summary>
        public static bool BeginFoldoutToggleBorderLayout(SerializedProperty foldoutProperty, GUIContent title, ref bool toggle, out Rect headerRect, float headerHeight = 18f, bool roundedBox = true, bool canExpand = true)
        {
            headerRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect boxRect = headerRect;

            bool expanded = canExpand && foldoutProperty.isExpanded;
            bool foldoutResult = expanded;

            if (foldoutResult)
            {
                Rect drawingRect = EditorGUILayout.BeginVertical(Styles.borderBoxHeaderStyle);
                boxRect.yMax = drawingRect.yMax;
            }

            GUI.Box(boxRect, GUIContent.none, new GUIStyle(roundedBox ? "HelpBox" : "Tooltip"));
            DrawFoldoutToggleHeader(headerRect, title, ref expanded, ref toggle, canExpand);
            foldoutProperty.isExpanded = expanded;
            return foldoutResult;
        }

        /// <summary>
        /// End a bordered vertical group.
        /// </summary>
        public static void EndBorderHeaderLayout()
        {
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw all class property childs inside bordered foldout.
        /// </summary>
        public static Rect DrawClassBorderFoldout(SerializedProperty classProperty, GUIContent title, float headerHeight = 18f, bool roundedBox = true)
        {
            var classChildrens = classProperty.GetVisibleChildrens();
            if (BeginFoldoutBorderLayout(classProperty, title, out Rect headerRect, headerHeight, roundedBox))
            {
                foreach (var child in classChildrens)
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        bool isArray = IsArray(child);

                        if (isArray) EditorGUI.indentLevel++;
                        {
                            EditorGUILayout.PropertyField(child, true);
                        }
                        if (isArray) EditorGUI.indentLevel--;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        classProperty.serializedObject.ApplyModifiedProperties();
                    }
                }

                EndBorderHeaderLayout();
            }

            return headerRect;
        }

        /// <summary>
        /// Draw all class property childs inside bordered foldout.
        /// </summary>
        public static Rect DrawClassBorderFoldout(SerializedProperty classProperty, GUIContent title, ref bool expanded, float headerHeight = 18f, bool roundedBox = true)
        {
            var classChildrens = classProperty.GetVisibleChildrens();
            if (BeginFoldoutBorderLayout(title, ref expanded, out Rect headerRect, headerHeight, roundedBox))
            {
                foreach (var child in classChildrens)
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        bool isArray = IsArray(child);

                        if (isArray) EditorGUI.indentLevel++;
                        {
                            EditorGUILayout.PropertyField(child, true);
                        }
                        if (isArray) EditorGUI.indentLevel--;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        classProperty.serializedObject.ApplyModifiedProperties();
                    }
                }

                EndBorderHeaderLayout();
            }

            return headerRect;
        }

        /// <summary>
        /// Draw scriptable property childs inside bordered foldout.
        /// </summary>
        public static Rect DrawScriptableBorderFoldout(SerializedProperty scriptableProperty, GUIContent title, float headerHeight = 18f, bool roundedBox = true)
        {
            SerializedObject serializedObject = scriptableProperty.propertyType == SerializedPropertyType.ObjectReference
                ? new SerializedObject(scriptableProperty.objectReferenceValue)
                : scriptableProperty.serializedObject;

            SerializedProperty iterator = serializedObject.GetIterator();
            HasIteratorChilds(iterator);

            if (BeginFoldoutBorderLayout(scriptableProperty, title, out Rect headerRect, headerHeight, roundedBox))
            {
                do
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        SerializedProperty child = serializedObject.FindProperty(iterator.name);
                        bool isArray = IsArray(child);

                        if (isArray) EditorGUI.indentLevel++;
                        {
                            EditorGUILayout.PropertyField(child, true);
                        }
                        if (isArray) EditorGUI.indentLevel--;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                while (iterator.NextVisible(false));

                EndBorderHeaderLayout();
            }

            return headerRect;
        }

        /// <summary>
        /// Draw scriptable property childs inside bordered foldout toggle.
        /// </summary>
        public static Rect DrawScriptableBorderFoldoutToggle(SerializedProperty scriptableProperty, GUIContent title, ref bool toggle, float headerHeight = 18f, bool roundedBox = true)
        {
            SerializedObject serializedObject = scriptableProperty.propertyType == SerializedPropertyType.ObjectReference
                ? new SerializedObject(scriptableProperty.objectReferenceValue)
                : scriptableProperty.serializedObject;

            SerializedProperty iterator = serializedObject.GetIterator();
            bool hasChilds = HasIteratorChilds(iterator);

            if (BeginFoldoutToggleBorderLayout(scriptableProperty, title, ref toggle, out Rect headerRect, headerHeight, roundedBox, canExpand: hasChilds))
            {
                if (hasChilds)
                {
                    do
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            SerializedProperty child = serializedObject.FindProperty(iterator.name);
                            bool isArray = IsArray(child);

                            if (isArray) EditorGUI.indentLevel++;
                            {
                                EditorGUILayout.PropertyField(child, true);
                            }
                            if (isArray) EditorGUI.indentLevel--;
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                    while (iterator.NextVisible(false));
                }

                EndBorderHeaderLayout();
            }

            return headerRect;
        }

        /// <summary>
        /// Draw scriptable property childs inside bordered foldout toggle.
        /// </summary>
        public static Rect DrawScriptableBorderFoldoutToggle(SerializedProperty scriptableProperty, GUIContent title, ref bool expanded, ref bool toggle, float headerHeight = 18f, bool roundedBox = true)
        {
            SerializedObject serializedObject = scriptableProperty.propertyType == SerializedPropertyType.ObjectReference
                ? new SerializedObject(scriptableProperty.objectReferenceValue)
                : scriptableProperty.serializedObject;

            SerializedProperty iterator = serializedObject.GetIterator();
            bool hasChilds = HasIteratorChilds(iterator);

            if (BeginFoldoutToggleBorderLayout(title, ref expanded, ref toggle, out Rect headerRect, headerHeight, roundedBox, canExpand: hasChilds))
            {
                if (hasChilds)
                {
                    do
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            SerializedProperty child = serializedObject.FindProperty(iterator.name);
                            bool isArray = IsArray(child);

                            if (isArray) EditorGUI.indentLevel++;
                            {
                                EditorGUILayout.PropertyField(child, true);
                            }
                            if (isArray) EditorGUI.indentLevel--;
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                    while (iterator.NextVisible(false));
                }

                EndBorderHeaderLayout();
            }

            return headerRect;
        }

        /// <summary>
        /// Create a new texture with the given dimensions and color.
        /// </summary>
        public static Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = color;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        /// <summary>
        /// Draw texture with a transprent background.
        /// </summary>
        public static void DrawTransparentTexture(Rect rect, Texture image)
        {
            Color guiColor = GUI.color;
            GUI.color = Color.clear;
            EditorGUI.DrawTextureTransparent(rect, image, ScaleMode.ScaleToFit);
            GUI.color = guiColor;
        }

        /// <summary>
        /// Draw texture with a transprent background.
        /// </summary>
        public static void DrawTransparentTextureWithChecker(Rect rect, Object spriteObj)
        {
            GUI.DrawTexture(rect, Styles.TransparentCheckerTexture, ScaleMode.ScaleToFit, alphaBlend: false);
            if(spriteObj != null) GUI.DrawTexture(rect, ((Sprite)spriteObj).texture, ScaleMode.ScaleToFit, alphaBlend: true);
        }

        public static PropertyCollection GetAllProperties(SerializedProperty classProperty)
        {
            PropertyCollection properties = new();

            SerializedProperty currentProperty = classProperty.Copy();
            SerializedProperty nextSiblingProperty = classProperty.Copy();
            nextSiblingProperty.NextVisible(false);

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    properties.Add(currentProperty.name, currentProperty.Copy());
                }
                while (currentProperty.NextVisible(false));
            }

            return properties;
        }

        public static PropertyCollection GetAllProperties(SerializedObject serializedObject)
        {
            PropertyCollection properties = new();

            SerializedProperty property = serializedObject.GetIterator();
            SerializedProperty currentProperty = property.Copy();
            SerializedProperty nextSiblingProperty = property.Copy();

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    properties.Add(currentProperty.name, currentProperty.Copy());
                }
                while (currentProperty.NextVisible(false));
            }

            return properties;
        }

        public static bool HasIteratorChilds(SerializedProperty iterator)
        {
            return iterator != null && iterator.NextVisible(true) && iterator.NextVisible(false);
        }

        public static bool IsArray(SerializedProperty property)
        {
            return property.isArray && property.propertyType != SerializedPropertyType.String;
        }

        public static float GetPropertyChildHeight(SerializedProperty iterator)
        {
            SerializedProperty ite = iterator.Copy();
            float totalHeight = EditorGUI.GetPropertyHeight(ite, true);

            while (ite.NextVisible(false))
            {
                totalHeight += EditorGUIUtility.standardVerticalSpacing;
                totalHeight += EditorGUI.GetPropertyHeight(ite, true);
            }

            return totalHeight;
        }
    }

    public static class SerializedPropertyE
    {
        public static void DrawRelative(this SerializedProperty property, string propertyName)
        {
            SerializedProperty relative = property.FindPropertyRelative(propertyName);
            EditorGUILayout.PropertyField(relative);
        }

        public static void DrawRelative(this SerializedProperty property, string propertyName, GUIContent label)
        {
            SerializedProperty relative = property.FindPropertyRelative(propertyName);
            EditorGUILayout.PropertyField(relative, label);
        }

        public static SerializedProperty Relative(this SerializedProperty property, string propertyName)
        {
            return property.FindPropertyRelative(propertyName);
        }

        public static SerializedProperty Find(this SerializedProperty property, string path)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            string[] elements = path.Split('.');
            SerializedProperty currentProperty = property;

            foreach (var element in elements)
            {
                currentProperty = currentProperty.FindPropertyRelative(element);
                if (currentProperty == null)
                    throw new ArgumentException($"Property not found in path: {path}", nameof(path));
            }

            return currentProperty;
        }
    }

    public static class AdvancedDropdownExtensions
    {
        public static void Show(this AdvancedDropdown dropdown, Rect buttonRect, float maxHeight)
        {
            dropdown.Show(buttonRect);
            SetMaxHeightForOpenedPopup(buttonRect, maxHeight);
        }

        private static void SetMaxHeightForOpenedPopup(Rect buttonRect, float maxHeight)
        {
            var window = EditorWindow.focusedWindow;

            if (window == null)
            {
                Debug.LogWarning("EditorWindow.focusedWindow was null.");
                return;
            }

            if (!string.Equals(window.GetType().Namespace, typeof(AdvancedDropdown).Namespace))
            {
                Debug.LogWarning("EditorWindow.focusedWindow " + EditorWindow.focusedWindow.GetType().FullName + " was not in expected namespace.");
                return;
            }

            var position = window.position;
            if (position.height <= maxHeight)
            {
                return;
            }

            position.height = maxHeight;
            window.minSize = position.size;
            window.maxSize = position.size;
            window.position = position;
            window.ShowAsDropDown(GUIUtility.GUIToScreenRect(buttonRect), position.size);
        }
    }
}