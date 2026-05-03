using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using UHFPS.Tools;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(ZiplineBuilder))]
    public class ZiplineBuilderEditor : InspectorEditor<ZiplineBuilder>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Zipline Builder"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("ZiplineRack");
                Properties.Draw("ZiplineForward");
                Properties.Draw("ZiplineUpward");
                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Zipline Offsets")))
                {
                    Properties.Draw("ZiplineEnd");
                    Properties.Draw("CenterOffset");

                    EditorGUILayout.Space(2f);
                    if (GUILayout.Button("Reset Zipline End", GUILayout.Height(20f)))
                    {
                        Target.ResetEndPosition();
                    }
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Cable Properties")))
                {
                    SerializedProperty cableProperties = Target.Cable == null
                        ? Properties["CableSettings"]
                        : new SerializedObject(Target.Cable).FindProperty("settings");

                    foreach (var child in cableProperties.GetVisibleChildrens())
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            bool isArray = EditorDrawing.IsArray(child);

                            if (isArray) EditorGUI.indentLevel++;
                            {
                                EditorGUILayout.PropertyField(child, true);
                            }
                            if (isArray) EditorGUI.indentLevel--;
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            cableProperties.serializedObject.ApplyModifiedProperties();
                            if (Target.Cable != null && !Target.Cable.manualGeneration)
                                Target.Cable.RegenerateCable();
                        }
                    }
                }

                EditorGUILayout.Space();
                Properties.Draw("PreviewCable");
                if (Properties.DrawGetBool("PreviewPlayer"))
                {
                    Properties.Draw("PlayerRadius");
                    Properties.Draw("PlayerHeight");
                }

                EditorGUILayout.Space();
                using (new EditorGUI.DisabledGroupScope(Target.ZiplineRack == null))
                {
                    if (GUILayout.Button("Build Zipline", GUILayout.Height(25f)))
                    {
                        Vector3 startPosition = Target.transform.position;
                        Vector3 endPosition = Target.ZiplineEnd;

                        if (Vector3.Distance(startPosition, endPosition) >= 2)
                        {
                            Transform oldStart = Target.transform.Find("ZiplineStart");
                            if (oldStart != null) DestroyImmediate(oldStart.gameObject);

                            Transform oldEnd = Target.transform.Find("ZiplineEnd");
                            if (oldEnd != null) DestroyImmediate(oldEnd.gameObject);

                            Transform oldCable = Target.transform.Find("ZiplineCable");
                            if (oldCable != null) DestroyImmediate(oldCable.gameObject);

                            GameObject ziplineStart = Instantiate(Target.ZiplineRack, startPosition, Quaternion.identity, Target.transform);
                            ziplineStart.name = "ZiplineStart";
                            GameObject ziplineEnd = Instantiate(Target.ZiplineRack, endPosition, Quaternion.identity, Target.transform);
                            ziplineEnd.name = "ZiplineEnd";

                            Vector3 startForward = ziplineStart.transform.Direction(Target.ZiplineForward);
                            Vector3 startUpward = ziplineStart.transform.Direction(Target.ZiplineUpward);
                            Vector3 startDirection = Quaternion.LookRotation(endPosition - startPosition, startUpward) * startForward;
                            Quaternion startLook = Quaternion.LookRotation(startDirection);

                            Vector3 startAngles = ziplineStart.transform.eulerAngles;
                            startAngles.y = startLook.eulerAngles.y;
                            ziplineStart.transform.eulerAngles = startAngles;

                            Vector3 endForward = ziplineEnd.transform.Direction(Target.ZiplineForward);
                            Vector3 endUpward = ziplineEnd.transform.Direction(Target.ZiplineUpward);
                            Vector3 endDirection = Quaternion.LookRotation(endPosition - startPosition, endUpward) * endForward;
                            Quaternion endLook = Quaternion.LookRotation(endDirection);

                            Vector3 endAngles = ziplineEnd.transform.eulerAngles;
                            endAngles.y = endLook.eulerAngles.y;
                            ziplineEnd.transform.eulerAngles = endAngles;

                            ProceduralCablePoint cableStart = ziplineStart.GetComponentInChildren<ProceduralCablePoint>();
                            ProceduralCablePoint cableEnd = ziplineEnd.GetComponentInChildren<ProceduralCablePoint>();

                            GameObject cable = new GameObject("ZiplineCable");
                            cable.transform.SetParent(Target.transform);
                            cable.transform.localPosition = Vector3.zero;

                            ProceduralCable proceduralCable = Target.Cable;
                            if (proceduralCable == null)
                            {
                                proceduralCable = cable.AddComponent<ProceduralCable>();
                                proceduralCable.manualGeneration = true;
                                proceduralCable.cableGenerated = true;
                                proceduralCable.settings = Target.CableSettings;
                                Target.Cable = proceduralCable;
                            }

                            if (cableStart != null && cableEnd != null)
                            {
                                proceduralCable._startTransform = cableStart.transform;
                                cableStart.proceduralCable = proceduralCable;

                                proceduralCable._endTransform = cableEnd.transform;
                                cableEnd.proceduralCable = proceduralCable;

                                proceduralCable.RegenerateCable();
                            }
                            else
                            {
                                Debug.LogError("Please add the 'ProceduralCableEnd' component to the zipline prefab!");
                            }
                        }
                        else
                        {
                            Debug.LogError("The end position of the zipline must be at least 2m away from the start position!");
                        }
                    }
                }

                if(Target.ZiplineRack == null)
                {
                    EditorGUILayout.Space(2f);
                    EditorGUILayout.HelpBox("Please assign a ZiplineRack object to build a zipline.", MessageType.Warning);
                }
            }
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void OnSceneGUI()
        {
            EditorGUI.BeginChangeCheck();
            Vector3 endPosition = Handles.PositionHandle(Target.ZiplineEnd, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Target.ZiplineEnd = endPosition;
            }

            Handles.Label(Target.transform.position, "Start Position");
            if(Vector3.Distance(Target.transform.position, Target.ZiplineEnd) > 0.1f)
                Handles.Label(endPosition, "End Position");
        }
    }
}