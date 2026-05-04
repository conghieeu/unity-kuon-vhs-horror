using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(InteractableItem))]
    public class InteractableItemEditor : InspectorEditor<InteractableItem>
    {
        private readonly bool[] foldout = new bool[6];

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Interactable Item"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                InteractableItem.InteractableTypeEnum interactableTypeEnum = (InteractableItem.InteractableTypeEnum)Properties["InteractableType"].enumValueIndex;
                InteractableItem.MessageTypeEnum messageTypeEnum = (InteractableItem.MessageTypeEnum)Properties["MessageType"].enumValueIndex;
                InteractableItem.ExamineTypeEnum examineTypeEnum = (InteractableItem.ExamineTypeEnum)Properties["ExamineType"].enumValueIndex;

                using (new EditorDrawing.BorderBoxScope())
                {
                    GUIContent title = EditorDrawing.IconTextContent("Interact Properties", "ViewToolMove", 14f);
                    EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
                    EditorDrawing.ResetIconSize();
                    EditorGUILayout.Space(2f);

                    if (interactableTypeEnum != InteractableItem.InteractableTypeEnum.InventoryItem)
                    {
                        Properties["UseInventoryTitle"].boolValue = false;
                        Properties["ExamineInventoryTitle"].boolValue = false;
                    }

                    // enums
                    {
                        Properties.Draw("InteractableType");
                        Properties.Draw("ExamineType");

                        if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None)
                            Properties.Draw("ExamineRotate");

                        if (interactableTypeEnum != InteractableItem.InteractableTypeEnum.ExamineItem)
                        {
                            Properties.Draw("MessageType");
                            Properties.Draw("DisableType");
                        }
                    }
                }
                EditorGUILayout.Space();

                // draw inventory item field
                if (interactableTypeEnum == InteractableItem.InteractableTypeEnum.InventoryItem)
                {
                    Properties.Draw("PickupItem");
                    EditorGUILayout.Space();
                }

                using (new EditorDrawing.BorderBoxScope())
                {
                    GUIContent title = EditorDrawing.IconTextContent("Item Settings", "Settings", 14f);
                    EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
                    EditorDrawing.ResetIconSize();
                    EditorGUILayout.Space(2f);

                    if (interactableTypeEnum == InteractableItem.InteractableTypeEnum.InventoryItem || interactableTypeEnum == InteractableItem.InteractableTypeEnum.InventoryExpand)
                    {
                        // draw item settings
                        if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Item Settings"), ref foldout[0]))
                        {
                            if (interactableTypeEnum == InteractableItem.InteractableTypeEnum.InventoryExpand)
                            {
                                if (Properties.DrawGetBool("ExpandRows"))
                                {
                                    Properties.Draw("SlotsToExpand", new GUIContent("Rows To Expand"));
                                }
                                else
                                {
                                    Properties.Draw("SlotsToExpand");
                                }
                            }
                            else
                            {
                                Properties.Draw("Quantity");
                                Properties.Draw("UseInventoryTitle");
                                if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None)
                                    Properties.Draw("ExamineInventoryTitle");

                                Properties.Draw("AutoShortcut");
                                Properties.Draw("AutoEquip");
                            }

                            EditorDrawing.EndBorderHeaderLayout();
                        }
                        EditorGUILayout.Space(1f);
                    }

                    if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None)
                    {
                        // draw examine settings
                        if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Examine Settings"), ref foldout[1]))
                        {
                            EditorGUILayout.BeginVertical(GUI.skin.box);
                            {
                                if (Properties.DrawToggleLeft("UseExamineZooming"))
                                {
                                    Properties.Draw("ExamineZoomLimits");
                                    float minLimit = Properties["ExamineZoomLimits"].FindPropertyRelative("min").floatValue;
                                    float maxLimit = Properties["ExamineZoomLimits"].FindPropertyRelative("max").floatValue;
                                    SerializedProperty examineDistance = Properties["ExamineDistance"];
                                    examineDistance.floatValue = EditorGUILayout.Slider(new GUIContent(examineDistance.displayName), examineDistance.floatValue, minLimit, maxLimit);
                                }
                                else
                                {
                                    Properties.Draw("ExamineDistance");
                                }
                            }
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.BeginVertical(GUI.skin.box);
                            {
                                using (new EditorGUI.DisabledGroupScope(!Properties.DrawToggleLeft("UseFaceRotation")))
                                {
                                    Properties.Draw("FaceRotation");
                                }
                            }
                            EditorGUILayout.EndVertical();

                            EditorGUILayout.BeginVertical(GUI.skin.box);
                            {
                                using (new EditorGUI.DisabledGroupScope(!Properties.DrawToggleLeft("UseControlPoint")))
                                {
                                    Properties.Draw("ControlPoint");
                                }
                            }
                            EditorGUILayout.EndVertical();

                            if (interactableTypeEnum != InteractableItem.InteractableTypeEnum.InventoryExpand)
                            {
                                Properties.Draw("IsPaper");
                                Properties.Draw("AllowCursorExamine");
                            }

                            if (interactableTypeEnum != InteractableItem.InteractableTypeEnum.ExamineItem)
                                Properties.Draw("TakeFromExamine");

                            EditorDrawing.EndBorderHeaderLayout();
                        }
                        EditorGUILayout.Space(1f);
                    }

                    // draw message settings
                    if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Message Settings"), ref foldout[2]))
                    {
                        if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None)
                            Properties.Draw("ShowExamineTitle");

                        if (!Properties["UseInventoryTitle"].boolValue)
                            Properties.Draw("InteractTitle");

                        if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None && Properties["ShowExamineTitle"].boolValue && !Properties["ExamineInventoryTitle"].boolValue)
                            Properties.Draw("ExamineTitle");

                        if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None && Properties["IsPaper"].boolValue)
                            Properties.Draw("PaperText");

                        if (messageTypeEnum == InteractableItem.MessageTypeEnum.Hint)
                            Properties.Draw("HintMessage");

                        if (interactableTypeEnum != InteractableItem.InteractableTypeEnum.ExamineItem && messageTypeEnum != InteractableItem.MessageTypeEnum.None)
                            Properties.Draw("MessageTime");

                        EditorDrawing.EndBorderHeaderLayout();
                    }
                    EditorGUILayout.Space(1f);

                    if (interactableTypeEnum == InteractableItem.InteractableTypeEnum.InventoryItem)
                    {
                        // draw custom item data
                        if (EditorDrawing.BeginFoldoutBorderLayout(Properties["ItemCustomData"], new GUIContent("Item Custom Data")))
                        {
                            SerializedProperty jsonData = Properties["ItemCustomData"].FindPropertyRelative("JsonData");
                            EditorGUILayout.PropertyField(jsonData);
                            EditorDrawing.EndBorderHeaderLayout();
                        }
                        EditorGUILayout.Space(1f);
                    }

                    if (interactableTypeEnum != InteractableItem.InteractableTypeEnum.GenericItem && examineTypeEnum == InteractableItem.ExamineTypeEnum.CustomObject)
                    {
                        // draw custom examine settings
                        if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Custom Examine Settings"), ref foldout[3]))
                        {
                            EditorGUI.indentLevel++;
                            {
                                Properties.Draw("CollidersEnable");
                                EditorGUILayout.Space(1f);
                                Properties.Draw("CollidersDisable");
                                EditorGUILayout.Space(1f);
                                Properties.Draw("ExamineHotspot");
                            }
                            EditorGUI.indentLevel--;
                            EditorDrawing.EndBorderHeaderLayout();
                        }
                        EditorGUILayout.Space(1f);
                    }

                    // draw sound settings
                    if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Sound Settings"), ref foldout[4]))
                    {
                        if (interactableTypeEnum != InteractableItem.InteractableTypeEnum.ExamineItem)
                        {
                            Properties.Draw("PickupSound");
                        }

                        if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None)
                        {
                            EditorGUILayout.Space(2f);
                            Properties.Draw("ExamineSound");
                        }

                        if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None && Properties["ShowExamineTitle"].boolValue)
                        {
                            EditorGUILayout.Space(2f);
                            Properties.Draw("ExamineHintSound");
                        }

                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    // draw events settings
                    if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None || interactableTypeEnum != InteractableItem.InteractableTypeEnum.ExamineItem)
                    {
                        EditorGUILayout.Space(1f);
                        if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Event Settings"), ref foldout[5]))
                        {
                            if (interactableTypeEnum != InteractableItem.InteractableTypeEnum.ExamineItem)
                                Properties.Draw("OnTakeEvent");

                            if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None)
                            {
                                Properties.Draw("OnExamineStartEvent");
                                Properties.Draw("OnExamineEndEvent");
                            }

                            EditorDrawing.EndBorderHeaderLayout();
                        }
                    }

                    if (Properties["Quantity"].intValue < 1) Properties["Quantity"].intValue = 1;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}