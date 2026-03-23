using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using UHFPS.Scriptable;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(ObjectiveTrigger)), CanEditMultipleObjects]
    public class ObjectiveTriggerEditor : Editor
    {
        SerializedProperty triggerType;
        SerializedProperty objectiveType;

        SerializedProperty objective;
        SerializedProperty completeObjective;

        ObjectivesAsset objectivesAsset;

        private void OnEnable()
        {
            triggerType = serializedObject.FindProperty("triggerType");
            objectiveType = serializedObject.FindProperty("objectiveType");

            objective = serializedObject.FindProperty("objectiveToAdd");
            completeObjective = serializedObject.FindProperty("objectiveToComplete");

            if (ObjectiveManager.HasReference)
                objectivesAsset = ObjectiveManager.Instance.ObjectivesAsset;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ObjectiveTrigger.ObjectiveType objectiveTypeEnum = (ObjectiveTrigger.ObjectiveType)objectiveType.enumValueIndex;

            EditorDrawing.DrawInspectorHeader(new GUIContent("Objective Trigger"), target);
            EditorGUILayout.Space();

            using(new EditorDrawing.BorderBoxScope(false))
            {
                EditorGUILayout.PropertyField(triggerType);
                EditorGUILayout.PropertyField(objectiveType);
            }

            EditorGUILayout.Space();

            if (objectiveTypeEnum == ObjectiveTrigger.ObjectiveType.New)
            {
                EditorGUILayout.PropertyField(objective);
            }
            else if (objectiveTypeEnum == ObjectiveTrigger.ObjectiveType.Complete)
            {
                EditorGUILayout.PropertyField(completeObjective);
            }
            else if (objectiveTypeEnum == ObjectiveTrigger.ObjectiveType.NewAndComplete)
            {
                EditorGUILayout.PropertyField(objective);
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(completeObjective);
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledGroupScope(objectivesAsset == null))
            {
                if (GUILayout.Button("Ping Objectives Asset", GUILayout.Height(25)))
                {
                    EditorGUIUtility.PingObject(objectivesAsset);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}