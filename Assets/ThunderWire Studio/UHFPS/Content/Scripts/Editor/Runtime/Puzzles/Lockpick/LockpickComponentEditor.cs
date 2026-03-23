using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(LockpickComponent))]
    public class LockpickComponentEditor : InspectorEditor<LockpickComponent>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Lockpick Component"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("AudioSource");
                Properties.Draw("BobbyPin");
                Properties.Draw("LockpickKeyhole");
                Properties.Draw("KeyholeCopyLocation");

                EditorGUILayout.Space();
                using(new EditorDrawing.BorderBoxScope(new GUIContent("Lockpick Axis")))
                {
                    Properties.Draw("BobbyPinForward");
                    Properties.Draw("KeyholeForward");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("BobbyPin Properties")))
                {
                    Properties.Draw("BobbyPinRotateSpeed");
                    Properties.Draw("BobbyPinResetTime");
                    Properties.Draw("BobbyPinShakeAmount");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Keyhole Properties")))
                {
                    Properties.Draw("KeyholeUnlockAngle");
                    Properties.Draw("KeyholeRotateSpeed");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sounds")))
                {
                    Properties.Draw("Unlock");
                    Properties.Draw("BobbyPinBreak");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}