using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UHFPS.Scriptable;
using ThunderWire.Editors;
using System;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(SurfaceDefinition)), CanEditMultipleObjects]
    public class SurfaceDefinitionEditor : InspectorEditor<SurfaceDefinition>
    {
        private ReorderableList surfaceTextures;
        private ReorderableList surfaceFootsteps;
        private ReorderableList surfaceLandSteps;

        private ReorderableList surfaceBulletImpact;
        private ReorderableList surfaceMeleeImpact;

        private ReorderableList surfaceBulletmarks;
        private ReorderableList surfaceMeleemarks;

        public override void OnEnable()
        {
            base.OnEnable();

            surfaceTextures = new ReorderableList(serializedObject, Properties["SurfaceTextures"], true, false, true, true);
            surfaceTextures.drawElementCallback += (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = Properties["SurfaceTextures"].GetArrayElementAtIndex(index);
                Rect elementRect = new(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.ObjectField(elementRect, element, GUIContent.none);
            };

            surfaceFootsteps = new ReorderableList(serializedObject, Properties["SurfaceFootsteps"], true, false, true, true);
            surfaceFootsteps.drawElementCallback += (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = Properties["SurfaceFootsteps"].GetArrayElementAtIndex(index);
                Rect elementRect = new(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.ObjectField(elementRect, element, GUIContent.none);
            };

            surfaceLandSteps = new ReorderableList(serializedObject, Properties["SurfaceLandSteps"], true, false, true, true);
            surfaceLandSteps.drawElementCallback += (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = Properties["SurfaceLandSteps"].GetArrayElementAtIndex(index);
                Rect elementRect = new(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.ObjectField(elementRect, element, GUIContent.none);
            };

            surfaceBulletImpact = new ReorderableList(serializedObject, Properties["SurfaceBulletImpact"], true, false, true, true);
            surfaceBulletImpact.drawElementCallback += (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = Properties["SurfaceBulletImpact"].GetArrayElementAtIndex(index);
                Rect elementRect = new(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.ObjectField(elementRect, element, GUIContent.none);
            };

            surfaceMeleeImpact = new ReorderableList(serializedObject, Properties["SurfaceMeleeImpact"], true, false, true, true);
            surfaceMeleeImpact.drawElementCallback += (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = Properties["SurfaceMeleeImpact"].GetArrayElementAtIndex(index);
                Rect elementRect = new(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.ObjectField(elementRect, element, GUIContent.none);
            };

            surfaceBulletmarks = new ReorderableList(serializedObject, Properties["SurfaceBulletmarks"], true, false, true, true);
            surfaceBulletmarks.drawElementCallback += (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = Properties["SurfaceBulletmarks"].GetArrayElementAtIndex(index);
                Rect elementRect = new(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.ObjectField(elementRect, element, GUIContent.none);
            };

            surfaceMeleemarks = new ReorderableList(serializedObject, Properties["SurfaceMeleemarks"], true, false, true, true);
            surfaceMeleemarks.drawElementCallback += (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = Properties["SurfaceMeleemarks"].GetArrayElementAtIndex(index);
                Rect elementRect = new(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.ObjectField(elementRect, element, GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Surface Definition"), Target);
            EditorDrawing.ResetIconSize();
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    Properties.Draw("SurfaceTag");
                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["SurfaceTextures"], new GUIContent("Surface Textures"), out Rect headerRect))
                    {
                        surfaceTextures.DoLayoutList();
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    GUIStyle texturesStyle = new GUIStyle(EditorStyles.textField);
                    texturesStyle.alignment = TextAnchor.MiddleLeft;

                    Rect texturesRect = headerRect;
                    texturesRect.height = 15f;
                    texturesRect.xMin = texturesRect.xMax - 20f;
                    texturesRect.y += 3f;
                    texturesRect.x -= 3f;

                    GUI.enabled = false;
                    int textures = Properties["SurfaceTextures"].arraySize;
                    EditorGUI.TextField(texturesRect, textures.ToString(), texturesStyle);
                    GUI.enabled = true;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    EditorGUILayout.LabelField("Surface Settings", EditorStyles.boldLabel);
                    Properties.Draw("SurfaceFriction");
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Surface Footsteps", EditorStyles.boldLabel);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["FootstepsVolume"], new GUIContent("Footsteps Audio")))
                {
                    Properties.Draw("FootstepsVolume");

                    EditorGUILayout.Space();
                    surfaceFootsteps.DoLayoutList();

                    EditorGUILayout.Space();
                    CreateDropRectangle((audio) => 
                    {
                        Target.SurfaceFootsteps.Add(audio);
                        serializedObject.ApplyModifiedProperties();
                    });

                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["SurfaceLandSteps"], new GUIContent("LandSteps Audio")))
                {
                    Properties.Draw("LandStepsVolume");

                    EditorGUILayout.Space();
                    surfaceLandSteps.DoLayoutList();

                    EditorGUILayout.Space();
                    CreateDropRectangle((audio) =>
                    {
                        Target.SurfaceLandSteps.Add(audio);
                        serializedObject.ApplyModifiedProperties();
                    });

                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Surface Impacts", EditorStyles.boldLabel);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["BulletImpactVolume"], new GUIContent("Bullet Impact Audio")))
                {
                    Properties.Draw("BulletImpactVolume");

                    EditorGUILayout.Space();
                    surfaceBulletImpact.DoLayoutList();

                    EditorGUILayout.Space();
                    CreateDropRectangle((audio) =>
                    {
                        Target.SurfaceBulletImpact.Add(audio);
                        serializedObject.ApplyModifiedProperties();
                    });

                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["MeleeImpactVolume"], new GUIContent("Melee Impact Audio")))
                {
                    Properties.Draw("MeleeImpactVolume");

                    EditorGUILayout.Space();
                    surfaceMeleeImpact.DoLayoutList();

                    EditorGUILayout.Space();
                    CreateDropRectangle((audio) =>
                    {
                        Target.SurfaceMeleeImpact.Add(audio);
                        serializedObject.ApplyModifiedProperties();
                    });

                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Surface Marks", EditorStyles.boldLabel);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["SurfaceBulletmarks"], new GUIContent("Surface Bulletmarks")))
                {
                    surfaceBulletmarks.DoLayoutList();
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["SurfaceMeleemarks"], new GUIContent("Surface Meleemarks")))
                {
                    surfaceMeleemarks.DoLayoutList();
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void CreateDropRectangle(Action<AudioClip> onDrop)
        {
            GUIStyle centeredStyle = new(EditorStyles.helpBox)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                {
                    Rect dropRect = EditorGUILayout.GetControlRect(GUILayout.Width(135f), GUILayout.Height(25f));
                    GUI.Box(dropRect, "Drop Audio Clips Here", centeredStyle);

                    Event evt = Event.current;
                    switch (evt.type)
                    {
                        case EventType.DragUpdated:
                        case EventType.DragPerform:
                            if (!dropRect.Contains(evt.mousePosition))
                                return;

                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                            if (evt.type == EventType.DragPerform)
                            {
                                DragAndDrop.AcceptDrag();

                                foreach (var drag in DragAndDrop.objectReferences)
                                {
                                    if (drag is AudioClip audioClip)
                                        onDrop?.Invoke(audioClip);
                                }
                            }

                            Event.current.Use();
                            break;
                    }
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}