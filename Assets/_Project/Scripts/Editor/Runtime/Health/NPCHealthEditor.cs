using System.Linq;
using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;
using UHFPS.Runtime;
using UHFPS.Tools;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(NPCHealth))]
    public class NPCHealthEditor : InspectorEditor<NPCHealth>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("NPC Health"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                using(new EditorDrawing.BorderBoxScope(new GUIContent("Body Parts")))
                {
                    Properties.Draw("Hips");
                    Properties.Draw("Head");
                    Properties.Draw("BodyPartLayer");
                    EditorGUILayout.Space();

                    if(Target.BodySegments.Count > 0)
                    {
                        string parts = string.Join(", ", Target.BodySegments.Select(x => x.Collider.gameObject.name));
                        EditorGUILayout.HelpBox("Body Parts: " + parts, MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Click the \"Find body parts\" button to find all body parts in the hip transform. Body parts can only be found when the character is defined as a ragdoll. To define a character as a ragdoll, go to GameObject -> 3D Object -> Ragdoll.", MessageType.Info);
                    }

                    EditorGUILayout.Space(1f);

                    using (new EditorGUI.DisabledGroupScope(Target.Hips == null))
                    {
                        if (GUILayout.Button("Find Body Parts", GUILayout.Height(25)))
                        {
                            Target.BodySegments.Clear();
                            foreach (var collider in Target.Hips.GetComponentsInChildren<Collider>())
                            {
                                Rigidbody rigidbody = collider.GetComponent<Rigidbody>();

                                if(!collider.gameObject.TryGetComponent(out NPCBodyPart bodyPart))
                                    bodyPart = collider.gameObject.AddComponent<NPCBodyPart>();

                                if(Target.Head != null && collider == Target.Head)
                                    bodyPart.IsHeadDamage = true;

                                rigidbody.isKinematic = true;
                                rigidbody.useGravity = false;
                                collider.isTrigger = true;
                                bodyPart.HealthScript = Target;

                                Target.BodySegments.Add(new(rigidbody, collider, bodyPart));
                            }

                            Target.Hips.gameObject.SetLayerRecursively(Target.BodyPartLayer);
                        }
                    }
                }

                EditorGUILayout.Space();

                using(new EditorDrawing.BorderBoxScope(new GUIContent("Health Settings")))
                {
                    if(Properties.DrawGetBool("AllowHeadhsot"))
                        Properties.Draw("HeadshotMultiplier");

                    Properties.Draw("MaxHealth");
                    Properties.Draw("StartHealth");

                    EditorGUILayout.Space();
                    Rect healthRect = EditorGUILayout.GetControlRect();
                    float health = Application.isPlaying ? Target.EntityHealth : Target.StartHealth;
                    float healthPercent = health / Target.MaxHealth;
                    EditorGUI.ProgressBar(healthRect, healthPercent, $"Health ({health} HP)");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Corpse Remove"), Properties["RemoveCorpse"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("RemoveCorpse")))
                    {
                        Properties.Draw("DisableCorpse");
                        Properties.Draw("CorpseRemoveTime");
                    }
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Disable Components On Death")))
                {
                    Properties.DrawArray("DisableComponents");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sound Settings")))
                {
                    Properties.DrawArray("DamageSounds");
                    Properties.Draw("DamageVolume");

                    EditorGUILayout.Space();
                    Properties.Draw("DeathSound");
                }

                EditorGUILayout.Space();

                if(EditorDrawing.BeginFoldoutBorderLayout(Properties["OnTakeDamage"], new GUIContent("Events")))
                {
                    Properties.Draw("OnTakeDamage");
                    Properties.Draw("OnDeath");
                    if (Properties.BoolValue("RemoveCorpse"))
                        Properties.Draw("OnCorpseRemove");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}