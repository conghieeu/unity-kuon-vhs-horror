using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using UHFPS.Scriptable;
using ThunderWire.Editors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UHFPS.Editors
{
    public class PlayerItemEditor<T> : Editor where T : PlayerItemBehaviour
    {
        public T Target { get; private set; }
        public PlayerItemBehaviour Behaviour { get; private set; }
        public PropertyCollection Properties { get; private set; }

        private SerializedProperty foldoutProperty;
        private MotionListHelper motionListHelper;
        private ExternalMotionsDrawer externalMotionsDrawer;

        public virtual void OnEnable()
        {
            Target = target as T;
            Behaviour = Target;

            Properties = EditorDrawing.GetAllProperties(serializedObject);
            foldoutProperty = Properties["<ItemObject>k__BackingField"];

            MotionPreset preset = Behaviour.MotionPreset;
            motionListHelper = new(preset);

            SerializedProperty externalMotions = Properties["ExternalMotions"];
            externalMotionsDrawer = new(externalMotions, Target.ExternalMotions);
        }

        public override void OnInspectorGUI()
        {
            GUIContent headerContent = EditorDrawing.IconTextContent("Wieldable Settings", "Settings");
            EditorDrawing.SetLabelColor("#E0FBFC");

            if (EditorDrawing.BeginFoldoutBorderLayout(foldoutProperty, headerContent))
            {
                EditorDrawing.ResetLabelColor();

                if (EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Wall Detection"), Properties["EnableWallDetection"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("EnableWallDetection")))
                    {
                        Properties.Draw("WallHitTransform");
                        Properties.Draw("WallHitMask");
                        Properties.Draw("WallHitRayDistance");
                        Properties.Draw("WallHitRayRadius");
                        Properties.Draw("WallHitRayOffset");

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
                        Properties.Draw("WallHitAmount");
                        Properties.Draw("WallHitTime");
                        Properties.Draw("ShowRayGizmos");

                        EditorGUILayout.Space();
                        if(GUILayout.Button("Copy Settings"))
                        {
                            StorableCollection copyData = new()
                            {
                                { "mask", Target.WallHitMask.value },
                                { "rayDistance", Target.WallHitRayDistance },
                                { "rayRadius", Target.WallHitRayRadius },
                                { "rayOffset", Target.WallHitRayOffset.ToSaveable() },
                                { "hitAmount", Target.WallHitAmount },
                                { "hitTime", Target.WallHitTime },
                            };

                            StorableCollection parent = new()
                            {
                                { "wallDetectionData", copyData },
                            };

                            string json = JsonConvert.SerializeObject(parent);
                            GUIUtility.systemCopyBuffer = json;
                        }

                        if (GUILayout.Button("Paste Settings"))
                        {
                            string data = GUIUtility.systemCopyBuffer;
                            if (data.Contains("wallDetectionData"))
                            {
                                JObject pasteData = JObject.Parse(data);
                                JToken copiedData = pasteData["wallDetectionData"];

                                Target.WallHitMask = (int)copiedData["mask"];
                                Target.WallHitRayDistance = (float)copiedData["rayDistance"];
                                Target.WallHitRayRadius = (float)copiedData["rayRadius"];
                                Target.WallHitRayOffset = copiedData["rayOffset"].ToObject<Vector3>();
                                Target.WallHitAmount = (float)copiedData["hitAmount"];
                                Target.WallHitTime = (float)copiedData["hitTime"];
                            }
                        }
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }

                if (EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Wieldable Motions"), Properties["EnableMotionPreset"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("EnableMotionPreset")))
                    {
                        motionListHelper.DrawMotionPresetField(Properties["MotionPreset"]);
                        Properties.DrawBacking("MotionPivot");

                        if (motionListHelper != null)
                        {
                            EditorGUILayout.Space();
                            MotionPreset presetInstance = Behaviour.MotionBlender.Instance;
                            motionListHelper.DrawMotionsList(presetInstance);
                        }
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }

                if (EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Camera Motions"), Properties["EnableExternalMotion"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("EnableExternalMotion")))
                    {
                        externalMotionsDrawer.DrawExternalMotions(new GUIContent("External Motions"));
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorDrawing.EndBorderHeaderLayout();
            }
            EditorDrawing.ResetLabelColor();
        }
    }
}