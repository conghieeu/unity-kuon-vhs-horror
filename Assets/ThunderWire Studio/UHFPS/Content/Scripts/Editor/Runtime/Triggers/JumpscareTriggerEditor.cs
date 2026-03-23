using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;
using static UHFPS.Runtime.JumpscareTrigger;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(JumpscareTrigger))]
    public class JumpscareTriggerEditor : InspectorEditor<JumpscareTrigger>
    {
        private bool eventsExpanded;

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Jumpscare Trigger"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                DrawJumpscareTypeGroup();
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    if (Target.JumpscareType == JumpscareTypeEnum.Direct)
                    {
                        Properties.Draw("DirectType");
                    }

                    Properties.Draw("TriggerType");
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                if(Target.TriggerType == TriggerTypeEnum.Event)
                {
                    EditorGUILayout.HelpBox("The Jumpscare will be triggered when the TriggerJumpscare() method is called from another script.", MessageType.Info);
                }
                else if (Target.TriggerType == TriggerTypeEnum.TriggerEnter)
                {
                    EditorGUILayout.HelpBox("The Jumpscare will be triggered when the player enters the trigger.", MessageType.Info);
                }
                else if (Target.TriggerType == TriggerTypeEnum.TriggerExit)
                {
                    EditorGUILayout.HelpBox("The Jumpscare will be triggered when the player exits the trigger.", MessageType.Info);
                }

                EditorGUILayout.Space();
                EditorDrawing.Separator();
                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Jumpscare Setup")))
                {
                    if(Target.JumpscareType == JumpscareTypeEnum.Direct)
                    {
                        if(Target.DirectType == DirectTypeEnum.Image)
                        {
                            Properties.Draw("JumpscareImage");
                        }
                        else if(Target.DirectType == DirectTypeEnum.Model)
                        {
                            Properties.Draw("JumpscareModelID");
                        }

                        Properties.Draw("DirectDuration");
                        EditorGUILayout.Space(1f);
                    }
                    else if(Target.JumpscareType == JumpscareTypeEnum.Indirect)
                    {
                        Properties.Draw("Animator");
                        Properties.Draw("AnimatorStateName");
                        Properties.Draw("AnimatorTrigger");

                        EditorGUILayout.Space(1f);
                    }

                    Properties.Draw("JumpscareSound");
                }

                EditorGUILayout.Space(1f);

                if (Target.JumpscareType == JumpscareTypeEnum.Indirect || Target.JumpscareType == JumpscareTypeEnum.Audio)
                {
                    if (EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Look At Jumpscare"), Properties["LookAtJumpscare"]))
                    {
                        using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("LookAtJumpscare")))
                        {
                            EditorGUILayout.HelpBox("Slowly move the rotation of the look towards the jumpscare target.", MessageType.Info);
                            EditorGUILayout.Space(1f);

                            Properties.Draw("LookAtTarget");
                            Properties.Draw("LookAtDuration");
                            Properties.Draw("LockPlayer");
                            Properties.Draw("EndJumpscareWithEvent");
                        }
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                }

                EditorGUILayout.Space(1f);

                if (EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Influence Wobble"), Properties["InfluenceWobble"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("InfluenceWobble")))
                    {
                        EditorGUILayout.HelpBox("Wobble is a camera effect that causes the screen to shake when the player experiences a jumpscare.", MessageType.Info);
                        EditorGUILayout.Space(1f);

                        Properties.Draw("WobbleAmplitudeGain");
                        Properties.Draw("WobbleFrequencyGain");
                        Properties.Draw("WobbleDuration");
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);

                if (EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Influence Fear"), Properties["InfluenceFear"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("InfluenceFear")))
                    {
                        EditorGUILayout.HelpBox("Display the fear tentacles effect at the player's screen edges when the player experiences a jumpscare.", MessageType.Info);
                        EditorGUILayout.Space(1f);

                        Properties.Draw("TentaclesIntensity");
                        Properties.Draw("TentaclesSpeed");
                        Properties.Draw("VignetteStrength");
                        Properties.Draw("FearDuration");
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);

                DrawJumpscareEvents();
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawJumpscareTypeGroup()
        {
            GUIContent[] toolbarContent = {
                new GUIContent(Resources.Load<Texture>("EditorIcons/Jumpscare/direct_jumpscare"), "Direct Jumpscare"),
                new GUIContent(Resources.Load<Texture>("EditorIcons/Jumpscare/indirect_jumpscare"), "Indirect Jumpscare"),
                new GUIContent(Resources.Load<Texture>("EditorIcons/Jumpscare/audio_jumpscare"), "Audio Jumpscare"),
            };

            using (new EditorDrawing.IconSizeScope(25))
            {
                GUIStyle toolbarButtons = new GUIStyle(GUI.skin.button);
                toolbarButtons.fixedHeight = 0;
                toolbarButtons.fixedWidth = 50;

                Rect toolbarRect = EditorGUILayout.GetControlRect(false, 30);
                toolbarRect.width = toolbarButtons.fixedWidth * toolbarContent.Length;
                toolbarRect.x = EditorGUIUtility.currentViewWidth / 2 - toolbarRect.width / 2 + 7f;

                SerializedProperty jumpscareType = Properties["JumpscareType"];
                jumpscareType.enumValueIndex = GUI.Toolbar(toolbarRect, jumpscareType.enumValueIndex, toolbarContent, toolbarButtons);
            }
        }
    
        private void DrawJumpscareEvents()
        {
            if(EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Events"), ref eventsExpanded))
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["TriggerEnter"], new GUIContent("Trigger Events")))
                {
                    Properties.Draw("TriggerEnter");
                    Properties.Draw("TriggerExit");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);

                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnJumpscareStarted"], new GUIContent("Jumpscare Events")))
                {
                    Properties.Draw("OnJumpscareStarted");
                    Properties.Draw("OnJumpscareEnded");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorDrawing.EndBorderHeaderLayout();
            }
        }
    }
}