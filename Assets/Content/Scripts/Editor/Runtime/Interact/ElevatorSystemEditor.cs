using UHFPS.Runtime;
using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(ElevatorSystem))]
    public class ElevatorSystemEditor : InspectorEditor<ElevatorSystem>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Elevator System"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("Animator");
                Properties.Draw("AudioSource");

                EditorGUILayout.Space();

                using(new EditorDrawing.BorderBoxScope(new GUIContent("Elevator Settings")))
                {
                    Properties.Draw("Floors", 1);
                    Properties.Draw("FloorOffset");
                    Properties.Draw("OneFloorDuration");
                    Properties.Draw("AutoDoorCloseTime");
                    Properties.Draw("VerticalMoveOnly");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Animation Settings")))
                {
                    Properties.Draw("OpenDoorTrigger");
                    Properties.Draw("CloseDoorTrigger");
                    Properties.Draw("OpenDoorState");
                    Properties.Draw("CloseDoorState");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sound Settings")))
                {
                    Properties.Draw("ElevatorStartMove");
                    Properties.Draw("ElevatorEnd");
                    Properties.Draw("ElevatorOpenClean");
                    Properties.Draw("ElevatorOpenBeep");
                    Properties.Draw("ElevatorClose");
                }

                EditorGUILayout.Space();

                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnElevatorEnter"], new GUIContent("Events")))
                {
                    Properties.Draw("OnElevatorEnter");
                    Properties.Draw("OnElevatorExit");
                    Properties.Draw("OnElevatorEndMove");
                    Properties.Draw("OnElevatorStartMove");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}