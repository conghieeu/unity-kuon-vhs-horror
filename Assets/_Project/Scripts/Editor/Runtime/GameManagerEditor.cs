using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : InspectorEditor<GameManager>
    {
        private ReorderableList graphicReferences;

        public override void OnEnable()
        {
            base.OnEnable();
            SerializedProperty property = Properties.GetRelative("GraphicReferencesRaw");
            graphicReferences = new ReorderableList(serializedObject, property, true, false, true, true)
            {
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    SerializedProperty element = property.GetArrayElementAtIndex(index);
                    rect.xMin += 12f;
                    ReorderableList.defaultBehaviours.DrawElement(rect, element, null, isActive, isFocused, true, true);
                },

                elementHeightCallback = (int index) =>
                {
                    SerializedProperty element = property.GetArrayElementAtIndex(index);
                    return EditorGUI.GetPropertyHeight(element) + EditorGUIUtility.standardVerticalSpacing;
                }
            };
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("UHFPS GameManager"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("Modules");
                Properties.Draw("GlobalPPVolume");
                Properties.Draw("HealthPPVolume");
                Properties.Draw("BackgroundFade");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
                EditorGUILayout.Space(2f);

                if(EditorDrawing.BeginFoldoutBorderLayout(Properties["GamePanel"], new GUIContent("Game Panels")))
                {
                    EditorGUILayout.LabelField("Main Panels", EditorStyles.boldLabel);
                    Properties.Draw("GamePanel");
                    Properties.Draw("PausePanel");
                    Properties.Draw("DeadPanel");

                    EditorGUILayout.Space(2f);
                    EditorGUILayout.LabelField("Sub Panels", EditorStyles.boldLabel);
                    Properties.Draw("HUDPanel");
                    Properties.Draw("TabPanel");

                    EditorGUILayout.Space(2f);
                    EditorGUILayout.LabelField("Feature Panels", EditorStyles.boldLabel);
                    Properties.Draw("InventoryPanel");
                    Properties.Draw("AlertsPanel");
                    Properties.Draw("FloatingIcons");

                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);

                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["SaveGameButton"], new GUIContent("Pause References")))
                {
                    Properties.Draw("SaveGameButton");
                    Properties.Draw("LoadGameButton");

                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);

                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["ReticleImage"], new GUIContent("HUD References")))
                {
                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["ControlsInfoPanel"], new GUIContent("Reticle/Stamina")))
                    {
                        Properties.Draw("ReticleImage");
                        Properties.Draw("InteractProgress");
                        Properties.Draw("StaminaSlider");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["InteractInfoPanel"], new GUIContent("Interact/Controls Info")))
                    {
                        Properties.Draw("InteractInfoPanel");
                        Properties.Draw("ControlsInfoPanel");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["PointerImage"], new GUIContent("Interact Pointer")))
                    {
                        Properties.Draw("PointerImage");
                        EditorGUI.indentLevel++;
                        Properties.Draw("NormalPointer");
                        Properties.Draw("HoverPointer");
                        Properties.Draw("ClickPointer");
                        Properties.Draw("DragVerticalPointer");
                        Properties.Draw("DragHorizontalPointer");
                        EditorGUI.indentLevel--;
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["ItemPickupLayout"], new GUIContent("Pickup Message")))
                    {
                        Properties.Draw("ItemPickupLayout");
                        Properties.Draw("ItemPickup");
                        Properties.Draw("PickupMessageTime");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["HintMessageGroup"], new GUIContent("Hint Message")))
                    {
                        Properties.Draw("HintMessageGroup");
                        Properties.Draw("HintMessageFadeSpeed");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["HealthBar"], new GUIContent("Health Panel")))
                    {
                        Properties.Draw("HealthBar");
                        Properties.Draw("Hearthbeat");
                        Properties.Draw("HealthPercent");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["PaperPanel"], new GUIContent("Paper Panel")))
                    {
                        Properties.Draw("PaperPanel");
                        Properties.Draw("PaperText");
                        Properties.Draw("PaperFadeSpeed");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["ExamineInfoPanel"], new GUIContent("Examine Panel")))
                    {
                        Properties.Draw("ExamineInfoPanel");
                        Properties.Draw("ExamineHotspots");
                        Properties.Draw("ExamineText");
                        Properties.Draw("ExamineFadeSpeed");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OverlaysParent"], new GUIContent("Overlays")))
                    {
                        Properties.Draw("OverlaysParent");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);

                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["EnableBlur"], new GUIContent("Blur Settings")))
                {
                    Properties.Draw("EnableBlur");
                    Properties.Draw("BlurRadius");
                    Properties.Draw("BlurDuration");

                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);

                if(EditorDrawing.BeginFoldoutBorderLayout(Properties["GraphicReferencesRaw"], new GUIContent("Custom UI References")))
                {
                    graphicReferences.DoLayoutList();
                    EditorDrawing.EndBorderHeaderLayout();
                }

                //EditorDrawing.DrawList(Properties["GraphicReferencesRaw"], new GUIContent("Custom UI References"));

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Manager Modules", EditorStyles.boldLabel);
                EditorGUILayout.Space(2f);

                if (Target.Modules != null)
                {
                    SerializedObject modulesAsset = new SerializedObject(Target.Modules);
                    SerializedProperty modules = modulesAsset.FindProperty("ManagerModules");

                    if (Target.Modules.ManagerModules.Any(x => x == null))
                    {
                        EditorGUILayout.HelpBox("There are elements that have an empty module reference!", MessageType.Warning);
                    }

                    for (int i = 0; i < modules.arraySize; i++)
                    {
                        SerializedProperty moduleProperty = modules.GetArrayElementAtIndex(i);
                        PropertyCollection moduleProperties = EditorDrawing.GetAllProperties(moduleProperty);
                        string moduleName = ((ManagerModule)moduleProperty.boxedValue).Name;

                        Rect headerRect = EditorGUILayout.GetControlRect(false, 22f);
                        Texture2D icon = Resources.Load<Texture2D>("EditorIcons/module");
                        GUIContent header = new GUIContent($" {moduleName} (Module)", icon);

                        using (new EditorDrawing.IconSizeScope(12))
                        {
                            if (moduleProperty.isExpanded = EditorDrawing.DrawFoldoutHeader(headerRect, header, moduleProperty.isExpanded))
                                moduleProperties.DrawAll();
                        }
                    }

                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                    EditorGUILayout.HelpBox("To add new modules, open the Manager Modules asset.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Assign the Manager Modules asset to view all modules.", MessageType.Info);
                }

                EditorGUILayout.Space();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}