using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;
using UHFPS.Runtime;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(DamageTrigger)), CanEditMultipleObjects]
    public class DamageTriggerEditor : InspectorEditor<DamageTrigger>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Damage Trigger"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("DamageReceiver");
                Properties.Draw("DamageType");

                EditorGUILayout.Space();
                using(new EditorDrawing.BorderBoxScope(new GUIContent("Damage Settings")))
                {
                    Properties.Draw("EnemyTag");
                    if (!Properties.DrawGetBool("InstantDeath"))
                    {
                        if (Properties.DrawGetBool("DamageInRange"))
                        {
                            Properties.Draw("DamageRange");
                        }
                        else
                        {
                            Properties.Draw("Damage");
                        }

                        if (Target.DamageType == DamageTrigger.DamageTypeEnum.Stay)
                            Properties.Draw("DamageRate");
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Maximum damage will be applied to the damageable object.", MessageType.Info);
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}