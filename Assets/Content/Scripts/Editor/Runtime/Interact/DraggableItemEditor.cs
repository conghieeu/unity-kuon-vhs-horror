using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;
using UHFPS.Runtime;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(DraggableItem))]
    public class DraggableItemEditor : InspectorEditor<DraggableItem>
    {
        private Rigidbody Rigidbody;

        public override void OnEnable()
        {
            base.OnEnable();
            Rigidbody = Target.GetComponent<Rigidbody>();
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Draggable Item"), Target);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("This object will be defined as draggable, so the player can move it. To define it's weight, change the mass value of the rigidbody component.", MessageType.Info);
            EditorGUILayout.Space(2f);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Weight: " + Rigidbody.mass + "kg", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            serializedObject.Update();
            {
                Properties.Draw("ZoomDistance");
                Properties.Draw("MaxHoldDistance");

                EditorGUILayout.Space();
                using(new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Impact Detection"), Properties["EnableImpactSound"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("EnableImpactSound")))
                    {
                        EditorGUI.indentLevel++;
                        Properties.Draw("ImpactSounds");
                        EditorGUI.indentLevel--;
                        Properties.Draw("ImpactVolume");
                        Properties.Draw("VolumeModifier");
                        Properties.Draw("NextImpact");
                    }
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Sliding Detection"), Properties["EnableSlidingSound"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("EnableSlidingSound")))
                    {
                        Properties.Draw("MinSlidingFactor");
                        Properties.Draw("SlidingVelocityRange");
                        Properties.Draw("SlidingVolumeModifier");
                        Properties.Draw("VolumeFadeOffSpeed");
                    }
                }

                EditorGUILayout.Space();
                if(EditorDrawing.BeginFoldoutBorderLayout(Properties["OnDragStarted"], new GUIContent("Events")))
                {
                    Properties.Draw("OnDragStarted");
                    Properties.Draw("OnDragEnded");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}