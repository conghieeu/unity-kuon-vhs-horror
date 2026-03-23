using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(CustomInteractEvent)), CanEditMultipleObjects]
    public class CustomInteractEventEditor : InspectorEditor<CustomInteractEvent>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Custom Interact Event"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                DrawInteractTypeGroup();
                EditorGUILayout.Space();
                EditorDrawing.Separator();
                EditorGUILayout.Space();

                using(new EditorDrawing.BorderBoxScope(new GUIContent("Interact Settings")))
                {
                    Properties.Draw("FreezePlayer");
                    Properties.Draw("InteractOnce");
                    if (Properties.DrawGetBool("UseInteractSound"))
                    {
                        Properties.Draw("InteractSound");
                    }
                }
                EditorGUILayout.Space();

                if (Properties["UseOnStartEvent"].boolValue)
                {
                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["UseOnStartEvent"], new GUIContent("Interact Start Events")))
                    {
                        Properties.Draw("OnStart");
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                    EditorGUILayout.Space(2f);
                }

                if (Properties["UseOnHoldEvent"].boolValue)
                {
                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["UseOnHoldEvent"], new GUIContent("Interact Hold Events")))
                    {
                        Properties.Draw("OnHold");
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                    EditorGUILayout.Space(2f);
                }

                if (Properties["UseOnStopEvent"].boolValue)
                {
                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["UseOnStopEvent"], new GUIContent("Interact Stop Events")))
                    {
                        Properties.Draw("OnStop");
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawInteractTypeGroup()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 25);

            MultiToolbarItem[] toolbarItems = new MultiToolbarItem[]
            {
                new MultiToolbarItem(new GUIContent("Start", "Use the interact start event. Called once."), Properties["UseOnStartEvent"]),
                new MultiToolbarItem(new GUIContent("Hold", "Use the interact hold event. Called as long the button is pressed."), Properties["UseOnHoldEvent"]),
                new MultiToolbarItem(new GUIContent("Stop", "Use the interact stop event. Called once."), Properties["UseOnStopEvent"]),
            };

            float buttonWidth = 80f;
            rect.width = buttonWidth * 3;
            rect.x = EditorGUIUtility.currentViewWidth / 2 - rect.width / 2 + 7f;
            EditorDrawing.DrawMultiToolbar(rect, toolbarItems, buttonWidth);
        }
    }
}