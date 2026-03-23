using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(ObjectiveEvent))]
    public class ObjectiveEventEditor : Editor
    {
        SerializedProperty Objective;

        SerializedProperty OnObjectiveAdded;
        SerializedProperty OnObjectiveCompleted;

        SerializedProperty OnSubObjectiveAdded;
        SerializedProperty OnSubObjectiveCompleted;
        SerializedProperty OnSubObjectiveCountChanged;

        bool[] expanded = new bool[2];

        private void OnEnable()
        {
            Objective = serializedObject.FindProperty("Objective");

            OnObjectiveAdded = serializedObject.FindProperty("OnObjectiveAdded");
            OnObjectiveCompleted = serializedObject.FindProperty("OnObjectiveCompleted");

            OnSubObjectiveAdded = serializedObject.FindProperty("OnSubObjectiveAdded");
            OnSubObjectiveCompleted = serializedObject.FindProperty("OnSubObjectiveCompleted");
            OnSubObjectiveCountChanged = serializedObject.FindProperty("OnSubObjectiveCountChanged");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorDrawing.DrawInspectorHeader(new GUIContent("Objective Event"), target);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(Objective);
            EditorGUILayout.Space();

            if(EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Objective Events"), ref expanded[0]))
            {
                EditorGUILayout.PropertyField(OnObjectiveAdded);
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(OnObjectiveCompleted);
                EditorDrawing.EndBorderHeaderLayout();
            }

            EditorGUILayout.Space(2f);

            if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("SubObjective Events"), ref expanded[1]))
            {
                EditorGUILayout.PropertyField(OnSubObjectiveAdded);
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(OnSubObjectiveCompleted);
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(OnSubObjectiveCountChanged);
                EditorDrawing.EndBorderHeaderLayout();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}