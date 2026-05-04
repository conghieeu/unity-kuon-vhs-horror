using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(Radio))]
    public class RadioEditor : InspectorEditor<Radio>
    {
        private ReorderableList radioChannels;

        public override void OnEnable()
        {
            base.OnEnable();
            radioChannels = new ReorderableList(serializedObject, Properties["RadioChannels"], true, false, true, true);

            radioChannels.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = radioChannels.serializedProperty.GetArrayElementAtIndex(index);
                Rect elementRect = new(rect.x, rect.y + 1f, rect.width, EditorGUIUtility.singleLineHeight);
                elementRect.xMin += 15;

                EditorGUI.PropertyField(elementRect, element, new GUIContent("Channel " + index), true);
            };

            radioChannels.elementHeightCallback = (int index) =>
            {
                SerializedProperty element = radioChannels.serializedProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + 4.0f;
            };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            {
                EditorDrawing.SetLabelColor("#F7E987");
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["RadioChannels"], new GUIContent("Radio Channels")))
                {
                    EditorDrawing.ResetLabelColor();
                    radioChannels.DoLayoutList(); // draw default reorderable list with expandable items
                    EditorDrawing.EndBorderHeaderLayout();
                }
                EditorDrawing.ResetLabelColor();

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Radio Settings")))
                {
                    Properties.Draw("RadioTuner");
                    Properties.Draw("TunerRod");
                    Properties.Draw("TunerMoveAxis");
                    Properties.Draw("TunerLimits");
                    Properties.Draw("TuneRange");
                    Properties.Draw("EmissionKeyword");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Radio Settings")))
                {
                    Properties.Draw("AudioSource");
                    Properties.Draw("RadioStatic");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Tune Settings", EditorStyles.miniBoldLabel);
                    Properties.Draw("TuneSounds", 1);
                    Properties.Draw("TuneVolume");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}