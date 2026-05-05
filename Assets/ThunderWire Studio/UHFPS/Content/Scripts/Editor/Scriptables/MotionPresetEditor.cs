using System;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using UHFPS.Scriptable;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(MotionPreset)), CanEditMultipleObjects]
    public class MotionPresetEditor : Editor
    {
        private MotionPreset asset;
        private MotionListDrawer motionListDrawer;
        private SerializedProperty stateMotions;

        private void OnEnable()
        {
            asset = target as MotionPreset;
            stateMotions = serializedObject.FindProperty("StateMotions");

            motionListDrawer = new();
            motionListDrawer.OnAddState = AddState;
            motionListDrawer.OnAddModule = AddModule;
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Motion Preset"));
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                GUIContent motionsLabel = new GUIContent("State Motions");
                motionListDrawer.DrawMotionsList(stateMotions, motionsLabel);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void AddState()
        {
            asset.StateMotions.Add(new());
            serializedObject.ApplyModifiedProperties();
        }

        private void AddModule(Type moduleType, int state)
        {
            MotionModule motionModule = (MotionModule)Activator.CreateInstance(moduleType);
            asset.StateMotions[state].Motions.Add(motionModule);
            serializedObject.ApplyModifiedProperties();
        }
    }
}