using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(PlayerItemsManager))]
    public class PlayerItemsManagerEditor : InspectorEditor<PlayerItemsManager>
    {
        private ReorderableList playerItemsList;

        public override void OnEnable()
        {
            base.OnEnable();
            playerItemsList = new ReorderableList(serializedObject, Properties["PlayerItems"], true, false, true, true);
            playerItemsList.drawElementCallback += (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = Properties["PlayerItems"].GetArrayElementAtIndex(index);
                string itemName = element.objectReferenceValue != null ? (element.objectReferenceValue as PlayerItemBehaviour).Name : "New Item";
                Rect elementRect = new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);

                Rect labelRect = elementRect;
                labelRect.width = EditorGUIUtility.labelWidth;
                EditorGUI.LabelField(labelRect, new GUIContent($"<b>[{index}]</b> {itemName}"), EditorDrawing.Styles.RichLabel);

                Rect propertyRect = elementRect;
                propertyRect.x += EditorGUIUtility.labelWidth + 2f;
                propertyRect.xMax = rect.xMax;
                EditorGUI.PropertyField(propertyRect, element, GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Player Items Manager"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                if(EditorDrawing.BeginFoldoutBorderLayout(Properties["PlayerItems"], new GUIContent("Player Items")))
                {
                    playerItemsList.DoLayoutList();
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Settings")))
                {
                    Properties.Draw("AntiSpamDelay");
                    Properties.Draw("IsItemsUsable");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}