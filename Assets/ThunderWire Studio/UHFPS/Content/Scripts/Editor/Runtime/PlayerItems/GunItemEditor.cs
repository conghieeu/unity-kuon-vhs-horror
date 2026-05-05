using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(GunItem))]
    public class GunItemEditor : PlayerItemEditor<GunItem>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Gun Item"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                Properties.Draw("GunName");
                Properties.DrawBacking("ItemObject");
                Properties.Draw("WeaponType");
                Properties.Draw("RaycastMask");

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    EditorGUILayout.LabelField("Surface", EditorStyles.boldLabel);
                    Properties.Draw("SurfaceDefinitionSet");
                    Properties.Draw("SurfaceDetection");
                    Properties.Draw("FleshTag");
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(2f);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    EditorGUILayout.LabelField("Inventory", EditorStyles.boldLabel);
                    Properties.Draw("GunInventoryItem");
                    Properties.Draw("AmmoInventoryItem");
                    Properties.Draw("FlashlightAttachmentItem");
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Gun Stats", EditorStyles.boldLabel);
                DrawGLDamageDropoff();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Gun Settings", EditorStyles.boldLabel);

                GUIContent baseSettingsGUI = EditorGUIUtility.TrTextContentWithIcon(" Base Settings", "Settings");
                EditorDrawing.DrawClassBorderFoldout(Properties["baseSettings"], baseSettingsGUI);

                EditorGUILayout.Space(1f);
                GUIContent recoilSettingsGUI = EditorGUIUtility.TrTextContentWithIcon(" Recoil Settings", "Settings");
                EditorDrawing.DrawClassBorderFoldout(Properties["recoilSettings"], recoilSettingsGUI);

                EditorGUILayout.Space(1f);
                GUIContent gunPropertiesGUI = EditorGUIUtility.TrTextContentWithIcon(" Gun Properties", "Settings");
                EditorDrawing.DrawClassBorderFoldout(Properties["gunProperties"], gunPropertiesGUI);

                EditorGUILayout.Space(1f);
                GUIContent aimingSettingsGUI = EditorGUIUtility.TrTextContentWithIcon(" Aiming Settings", "Toolbar Plus");
                EditorDrawing.DrawClassBorderFoldout(Properties["aimingSettings"], aimingSettingsGUI);

                EditorGUILayout.Space(1f);
                GUIContent attachmentSettingsGUI = EditorGUIUtility.TrTextContentWithIcon(" Attachment Settings", "Settings");
                EditorDrawing.DrawClassBorderFoldout(Properties["attachmentSettings"], attachmentSettingsGUI);

                EditorGUILayout.Space(1f);
                GUIContent bulletSettingsGUI = EditorGUIUtility.TrTextContentWithIcon(" Bullet Settings", "Settings");
                EditorDrawing.DrawClassBorderFoldout(Properties["bulletSettings"], bulletSettingsGUI);

                EditorGUILayout.Space(1f);
                GUIContent bulletAndMuzzleFlashGUI = EditorGUIUtility.TrTextContentWithIcon(" Bullet & MuzzleFlash", "TreeEditor.Material On");
                EditorDrawing.DrawClassBorderFoldout(Properties["bulletAndMuzzleFlash"], bulletAndMuzzleFlashGUI);

                EditorGUILayout.Space(1f);
                GUIContent animationSettingsGUI = EditorGUIUtility.TrTextContentWithIcon(" Animation Settings", "AnimatorState Icon");
                EditorDrawing.DrawClassBorderFoldout(Properties["animationSettings"], animationSettingsGUI);

                EditorGUILayout.Space(1f);
                GUIContent gunSoundsGUI = EditorGUIUtility.TrTextContentWithIcon(" Gun Sounds", "Profiler.Audio");
                EditorDrawing.DrawClassBorderFoldout(Properties["gunSounds"], gunSoundsGUI);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGLDamageDropoff()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                Material _uiMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
                float height = EditorGUIUtility.singleLineHeight;

                float shootRange = Target.baseSettings.ShootRange;
                float dropoffDistance = Target.baseSettings.DropoffDistance;

                int dropoffSections = (int)Target.baseSettings.DropoffSections;
                float distanceIte = dropoffDistance / dropoffSections - 1;

                Rect totalRect = EditorGUILayout.GetControlRect(true, height * 3);
                totalRect.yMin += height * 2 / 2;

                Rect labelRect = EditorGUI.PrefixLabel(totalRect, new GUIContent("Damage Dropoff"), EditorStyles.boldLabel);
                labelRect.height = height;

                Rect distanceLabelRect = labelRect;
                distanceLabelRect.y -= (labelRect.height / 2) + (height / 2);
                distanceLabelRect.xMin = totalRect.xMin;
                EditorGUI.LabelField(distanceLabelRect, "Distance", EditorStyles.miniLabel);

                Rect damageLabelRect = labelRect;
                damageLabelRect.y += (labelRect.height / 2) + (height / 2);
                damageLabelRect.xMin = totalRect.xMin;
                EditorGUI.LabelField(damageLabelRect, "Damage", EditorStyles.miniLabel);

                labelRect.width -= height;
                if (Event.current.type == EventType.Repaint)
                {
                    GUI.BeginClip(labelRect);
                    {
                        GL.PushMatrix();
                        GL.LoadPixelMatrix();
                        _uiMaterial.SetPass(0);

                        GL.Begin(GL.LINES);
                        {
                            // middle line
                            GL.Color(Color.white);
                            Line(new Vector2(0, labelRect.height / 2), new Vector2(labelRect.width, labelRect.height / 2));

                            // shoot range
                            GL.Color(Color.red);
                            float width = Mathf.Clamp(shootRange / dropoffDistance * labelRect.width, 0, labelRect.width);
                            Line(new Vector2(0, labelRect.height / 2), new Vector2(width, labelRect.height / 2));

                            // distances
                            GL.Color(Color.white);
                            for (int i = 0; i < dropoffSections; i++)
                            {
                                float x = labelRect.width / (dropoffSections - 1) * i;
                                float yMin = (labelRect.height / 2) - (height / 2);
                                float yMax = (labelRect.height / 2) + (height / 2);
                                Line(new Vector2(x, yMin), new Vector2(x, yMax));
                            }
                        }
                        GL.End();
                        GL.PopMatrix();
                    }
                    GUI.EndClip();
                }

                // distance dropoff texts
                float baseDamage = Target.baseSettings.BaseDamage;
                float rangeModifier = Target.baseSettings.RangeModifier;

                for (int i = 0; i < dropoffSections; i++)
                {
                    float x = labelRect.width / (dropoffSections - 1) * i;

                    // distance
                    float realDistance = distanceIte * i;
                    GUIContent distanceText = new GUIContent(Mathf.RoundToInt(realDistance).ToString());
                    Vector2 distanceTextSize = EditorStyles.miniBoldLabel.CalcSize(distanceText);

                    Rect distanceRect = labelRect;
                    distanceRect.x += x - distanceTextSize.x / 2;
                    distanceRect.y -= (labelRect.height / 2) + (height / 2);
                    EditorGUI.LabelField(distanceRect, distanceText, EditorStyles.miniBoldLabel);

                    // damage
                    float damage = baseDamage * Mathf.Pow(rangeModifier, realDistance / dropoffDistance);
                    GUIContent damageText = new GUIContent(Mathf.RoundToInt(damage).ToString());
                    Vector2 damageTextSize = EditorStyles.miniBoldLabel.CalcSize(damageText);

                    Rect damageRect = labelRect;
                    damageRect.x += x - damageTextSize.x / 2;
                    damageRect.y += (labelRect.height / 2) + (height / 2);
                    EditorGUI.LabelField(damageRect, damageText, EditorStyles.miniBoldLabel);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void Line(Vector2 p1, Vector2 p2)
        {
            GL.Vertex(p1);
            GL.Vertex(p2);
        }
    }
}