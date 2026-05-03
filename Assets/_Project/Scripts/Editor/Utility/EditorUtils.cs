using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using UnityEditor;

namespace ThunderWire.Editors
{
    public static class EditorUtils
    {
        public static class Styles
        {
            public static GUIStyle IconButton => GUI.skin.FindStyle("IconButton");
            public static readonly GUIContent PlusIcon = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add Item");
            public static readonly GUIContent MinusIcon = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove Item");
            public static readonly GUIContent TrashIcon = EditorGUIUtility.TrIconContent("TreeEditor.Trash", "Remove Item");
            public static readonly GUIContent RefreshIcon = EditorGUIUtility.TrIconContent("Refresh", "Refresh");
            public static readonly GUIContent Linked = EditorGUIUtility.TrIconContent("Linked");
            public static readonly GUIContent UnLinked = EditorGUIUtility.TrIconContent("Unlinked");
            public static readonly GUIContent Database = EditorGUIUtility.TrIconContent("Package Manager");
            public static readonly GUIContent GreenLight = EditorGUIUtility.TrIconContent("greenLight");
            public static readonly GUIContent OrangeLight = EditorGUIUtility.TrIconContent("orangeLight");
            public static readonly GUIContent RedLight = EditorGUIUtility.TrIconContent("redLight");

            public static GUIStyle RichLabel => new GUIStyle(EditorStyles.label)
            {
                richText = true
            };
        }

        public class BoxGroupScope : GUI.Scope
        {
            public BoxGroupScope(string icon, string title, float height = 22)
            {
                GUIContent iconTitle = EditorGUIUtility.TrTextContentWithIcon(" " + title, icon);
                EditorGUILayout.BeginVertical(GUI.skin.box);

                Rect headerRect = GUILayoutUtility.GetRect(1, height);
                EditorGUI.DrawRect(headerRect, new Color(0.1f, 0.1f, 0.1f, 0.4f));

                headerRect.x += EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.LabelField(headerRect, iconTitle, EditorStyles.boldLabel);

                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            }

            public BoxGroupScope(string title, float height = 22)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);

                Rect headerRect = GUILayoutUtility.GetRect(1, height);
                EditorGUI.DrawRect(headerRect, new Color(0.1f, 0.1f, 0.1f, 0.4f));

                headerRect.x += EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.LabelField(headerRect, title, EditorStyles.boldLabel);

                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            }

            protected override void CloseScope()
            {
                EditorGUILayout.EndVertical();
            }
        }

        public struct PopupArray
        {
            public List<string> contents;
            public List<string> selected;

            public PopupArray(string[] contents, string[] selected)
            {
                this.contents = new List<string>(contents);
                this.selected = new List<string>(selected);
            }
        }

        public sealed class PopupElement
        {
            public Action<string[]> onSelect;
            public PopupArray popupArray;
            public string name;

            public PopupElement(PopupArray popupArray, string name, Action<string[]> onSelect)
            {
                this.popupArray = popupArray;
                this.name = name;
                this.onSelect = onSelect;
            }
        }

        public static void DrawOutline(Rect rect, RectOffset border)
        {
            Color color = new Color(0.6f, 0.6f, 0.6f, 1.333f);
            if (EditorGUIUtility.isProSkin)
            {
                color.r = 0.12f;
                color.g = 0.12f;
                color.b = 0.12f;
            }

            if (Event.current.type != EventType.Repaint)
                return;

            Color orgColor = GUI.color;
            GUI.color *= color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, border.top), EditorGUIUtility.whiteTexture); //top
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - border.bottom, rect.width, border.bottom), EditorGUIUtility.whiteTexture); //bottom
            GUI.DrawTexture(new Rect(rect.x, rect.y + 1, border.left, rect.height - 2 * border.left), EditorGUIUtility.whiteTexture); //left
            GUI.DrawTexture(new Rect(rect.xMax - border.right, rect.y + 1, border.right, rect.height - 2 * border.right), EditorGUIUtility.whiteTexture); //right

            GUI.color = orgColor;
        }

        public static void DrawOutline(Rect rect, RectOffset border, Color color)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Color orgColor = GUI.color;
            GUI.color *= color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, border.top), EditorGUIUtility.whiteTexture); //top
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - border.bottom, rect.width, border.bottom), EditorGUIUtility.whiteTexture); //bottom
            GUI.DrawTexture(new Rect(rect.x, rect.y + 1, border.left, rect.height - 2 * border.left), EditorGUIUtility.whiteTexture); //left
            GUI.DrawTexture(new Rect(rect.xMax - border.right, rect.y + 1, border.right, rect.height - 2 * border.right), EditorGUIUtility.whiteTexture); //right

            GUI.color = orgColor;
        }

        public static Rect DrawHeader(float height, string title)
        {
            Rect rect = GUILayoutUtility.GetRect(0, height);
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 0.4f));

            var labelRect = rect;
            labelRect.x += 3f;

            EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);
            return rect;
        }

        public static void DrawHeader(Rect rect, string title, float labelX, float labelY, GUIStyle labelStyle)
        {
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 0.4f));

            var labelRect = rect;
            labelRect.x += labelX;
            labelRect.y += labelY;

            EditorGUI.LabelField(labelRect, title, labelStyle);
        }

        public static Rect DrawHeaderWithBorder(GUIContent title, float height, ref Rect rect, bool rounded)
        {
            GUI.Box(rect, GUIContent.none, new GUIStyle(rounded ? "HelpBox" : "Tooltip"));

            Rect headerRect = rect;
            headerRect.height = height;
            EditorGUI.DrawRect(headerRect, new Color(0.1f, 0.1f, 0.1f, 0.4f));

            Rect labelRect = headerRect;
            labelRect.y += height / 2;
            EditorGUI.LabelField(labelRect, title, EditorStyles.miniBoldLabel);

            rect.x += 1;
            rect.y += 1;
            rect.height -= 1;
            rect.width -= 2;
            rect.y += height;
            rect.height -= height;
            return rect;
        }

        public static Rect DrawHeaderWithBorder(GUIContent title, float height, ref Rect rect, GUIStyle boxStyle)
        {
            GUI.Box(rect, GUIContent.none, boxStyle);
            rect.x += 1;
            rect.y += 1;
            rect.height -= 1;
            rect.width -= 2;

            var headerRect = rect;
            headerRect.height = height + EditorGUIUtility.standardVerticalSpacing;

            rect.y += headerRect.height;
            rect.height -= headerRect.height;

            EditorGUI.DrawRect(headerRect, new Color(0.1f, 0.1f, 0.1f, 0.4f));

            var labelRect = headerRect;
            labelRect.y += EditorGUIUtility.standardVerticalSpacing;
            labelRect.x += 2f;

            EditorGUI.LabelField(labelRect, title, EditorStyles.miniBoldLabel);

            return headerRect;
        }

        public static Rect DrawHeaderWithBorder(GUIContent title, float height, ref Rect rect, RectOffset border)
        {
            DrawOutline(rect, border);
            rect.x += 1;
            rect.y += 1;
            rect.height -= 1;
            rect.width -= 2;

            var headerRect = rect;
            headerRect.height = height + EditorGUIUtility.standardVerticalSpacing;

            rect.y += headerRect.height;
            rect.height -= headerRect.height;

            EditorGUI.DrawRect(headerRect, new Color(0.1f, 0.1f, 0.1f, 0.4f));

            var labelRect = headerRect;
            labelRect.y += EditorGUIUtility.standardVerticalSpacing;
            labelRect.x += 2f;

            EditorGUI.LabelField(labelRect, title, EditorStyles.miniBoldLabel);

            return headerRect;
        }

        public static bool DrawBoxFoldoutHeader(string title, bool state, float height = 22)
        {
            Rect rect = GUILayoutUtility.GetRect(1, height);
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 0.4f));

            rect.x += EditorGUIUtility.standardVerticalSpacing;
            Rect foldoutRect = EditorGUI.IndentedRect(rect);
            state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);

            rect.x += EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing * 2;
            EditorGUI.LabelField(rect, new GUIContent(title), EditorStyles.boldLabel);

            return state;
        }

        public static bool DrawFoldoutHeader(float height, string title, bool state)
        {
            Rect rect = GUILayoutUtility.GetRect(1, height);

            rect.x += EditorGUIUtility.standardVerticalSpacing;
            Rect foldoutRect = EditorGUI.IndentedRect(rect);
            state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);

            rect.x += EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing * 2;
            EditorGUI.LabelField(rect, new GUIContent(title), EditorStyles.boldLabel);

            return state;
        }

        public static void DrawFoldoutToggleHeader(Rect rect, string title, ref bool isExpanded, ref bool isEnabled, bool showFoldout = true, bool toggleDisable = false)
        {
            Color headerColor = new Color(0.1f, 0.1f, 0.1f, 0f);

            var expandRect = rect;
            expandRect.xMin += EditorGUIUtility.singleLineHeight * 2;

            var foldoutRect = rect;
            foldoutRect.width = EditorGUIUtility.singleLineHeight;

            var toggleRect = rect;
            toggleRect.width = EditorGUIUtility.singleLineHeight;
            toggleRect.x += EditorGUIUtility.singleLineHeight;

            var labelRect = rect;
            labelRect.xMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;

            if (showFoldout)
            {
                // events
                var e = Event.current;
                if (expandRect.Contains(e.mousePosition))
                {
                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        isExpanded = !isExpanded;
                        e.Use();
                    }
                }

                // foldout
                isExpanded = GUI.Toggle(foldoutRect, isExpanded, GUIContent.none, EditorStyles.foldout);
            }

            // background
            EditorGUI.DrawRect(rect, headerColor);

            // toggle
            using (new EditorGUI.DisabledGroupScope(toggleDisable))
            {
                isEnabled = GUI.Toggle(toggleRect, isEnabled, new GUIContent("", "Extension Enabled State"), EditorStyles.toggle);
            }

            // title
            EditorGUI.LabelField(labelRect, new GUIContent(title), EditorStyles.boldLabel);
        }

        private static void PopupSelect(object data)
        {
            PopupElement element = (PopupElement)data;
            PopupArray array = element.popupArray;
            string name = element.name;

            if (array.selected.Contains(name)) 
                array.selected.Remove(name);
            else array.selected.Add(name);

            element.onSelect?.Invoke(array.selected.ToArray());
        }

        public static void DrawMultiSelectionPopup(Rect rect, string title, PopupArray popupArray, Action<string[]> onSelect)
        {
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < popupArray.contents.Count; i++)
            {
                string name = popupArray.contents[i];
                bool on = popupArray.selected.Contains(name);

                PopupElement element = new PopupElement(popupArray, name, onSelect);
                menu.AddItem(new GUIContent(name), on, PopupSelect, element);
            }

            if(GUI.Button(rect, title, EditorStyles.popup))
            {
                menu.ShowAsContext();
            }
        }

        public static void DrawRelativeProperties(SerializedProperty root, float width)
        {
            var childrens = root.GetVisibleChildrens();

            foreach (var childProperty in childrens)
            {
                float height = EditorGUI.GetPropertyHeight(childProperty, true);

                Rect rect = GUILayoutUtility.GetRect(1f, height);
                rect.xMin += width;
                EditorGUI.PropertyField(rect, childProperty, true);
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            }
        }

        public static IEnumerable<SerializedProperty> GetVisibleChildrens(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.NextVisible(false);
            }

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    yield return currentProperty;
                }
                while (currentProperty.NextVisible(false));
            }
        }

        public static void TrHelpIconText(string message, string icon, bool rich = false)
        {
            GUIStyle style = new GUIStyle(EditorStyles.helpBox)
            {
                richText = rich
            };

            EditorGUILayout.LabelField(GUIContent.none, EditorGUIUtility.TrTextContentWithIcon(" " + message, icon), style, new GUILayoutOption[0]);
        }

        public static void TrHelpIconText(Rect rect, string message, string icon, bool rich = false)
        {
            GUIStyle style = new GUIStyle(EditorStyles.helpBox)
            {
                richText = rich
            };

            EditorGUI.LabelField(rect, GUIContent.none, EditorGUIUtility.TrTextContentWithIcon(" " + message, icon), style);
        }

        public static void TrHelpIconText(string message, MessageType messageType, bool rich = false, bool space = true)
        {
            string icon = string.Empty;

            GUIStyle style = new GUIStyle(EditorStyles.helpBox)
            {
                richText = rich
            };

            switch (messageType)
            {
                case MessageType.Info:
                    icon = "console.infoicon.sml";
                    break;
                case MessageType.Warning:
                    icon = "console.warnicon.sml";
                    break;
                case MessageType.Error:
                    icon = "console.erroricon.sml";
                    break;
            }

            if (!string.IsNullOrEmpty(icon))
            {
                string text = space ? " " + message : message;
                EditorGUILayout.LabelField(GUIContent.none, EditorGUIUtility.TrTextContentWithIcon(text, icon), style, new GUILayoutOption[0]);
            }
            else
            {
                EditorGUILayout.LabelField(GUIContent.none, EditorGUIUtility.TrTextContent(message), style, new GUILayoutOption[0]);
            }
        }

        public static void TrHelpIconText(Rect rect, string message, MessageType messageType, bool rich = false, bool space = true)
        {
            string icon = string.Empty;

            GUIStyle style = new GUIStyle(EditorStyles.helpBox)
            {
                richText = rich
            };

            switch (messageType)
            {
                case MessageType.Info:
                    icon = "console.infoicon.sml";
                    break;
                case MessageType.Warning:
                    icon = "console.warnicon.sml";
                    break;
                case MessageType.Error:
                    icon = "console.erroricon.sml";
                    break;
            }

            if (!string.IsNullOrEmpty(icon))
            {
                string text = space ? " " + message : message;
                EditorGUI.LabelField(rect, GUIContent.none, EditorGUIUtility.TrTextContentWithIcon(text, icon), style);
            }
            else
            {
                EditorGUI.LabelField(rect, GUIContent.none, EditorGUIUtility.TrTextContent(message), style);
            }
        }

        public static void TrIconText(string message, string icon, GUIStyle style, bool rich = false, bool space = true)
        {
            style.richText = rich;
            string text = space ? " " + message : message;
            EditorGUILayout.LabelField(GUIContent.none, EditorGUIUtility.TrTextContentWithIcon(text, icon), style, new GUILayoutOption[0]);
        }

        public static void TrIconText(string message, MessageType messageType, GUIStyle style, bool rich = false, bool space = true)
        {
            string icon = string.Empty;
            style.richText = rich;

            switch (messageType)
            {
                case MessageType.Info:
                    icon = "console.infoicon.sml";
                    break;
                case MessageType.Warning:
                    icon = "console.warnicon.sml";
                    break;
                case MessageType.Error:
                    icon = "console.erroricon.sml";
                    break;
            }

            if (!string.IsNullOrEmpty(icon))
            {
                string text = space ? " " + message : message;
                EditorGUILayout.LabelField(GUIContent.none, EditorGUIUtility.TrTextContentWithIcon(text, icon), style, new GUILayoutOption[0]);
            }
            else
            {
                EditorGUILayout.LabelField(GUIContent.none, EditorGUIUtility.TrTextContent(message), style, new GUILayoutOption[0]);
            }
        }

        public static void TrIconText(Rect rect, string message, MessageType messageType, GUIStyle style, bool rich = false, bool space = true)
        {
            string icon = string.Empty;
            style.richText = rich;

            switch (messageType)
            {
                case MessageType.Info:
                    icon = "console.infoicon.sml";
                    break;
                case MessageType.Warning:
                    icon = "console.warnicon.sml";
                    break;
                case MessageType.Error:
                    icon = "console.erroricon.sml";
                    break;
            }

            if (!string.IsNullOrEmpty(icon))
            {
                string text = space ? " " + message : message;
                EditorGUI.LabelField(rect, GUIContent.none, EditorGUIUtility.TrTextContentWithIcon(text, icon), style);
            }
            else
            {
                EditorGUI.LabelField(rect, GUIContent.none, EditorGUIUtility.TrTextContent(message), style);
            }
        }
    }
}