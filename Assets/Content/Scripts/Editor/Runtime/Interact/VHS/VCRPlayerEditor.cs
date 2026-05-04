using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(VCRPlayer))]
    public class VCRPlayerEditor : InspectorEditor<VCRPlayer>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("VCR Player"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("VHSItem");
                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("References")))
                {
                    Properties.Draw("animator");
                    Properties.Draw("audioSource");
                    Properties.Draw("VHSTape");
                    Properties.Draw("insertCollider");
                    Properties.Draw("tapeMaterialProperty");
                }

                EditorGUILayout.Space(1f);
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Display")))
                {
                    Properties.Draw("timeText");
                    Properties.Draw("stateText");
                    Properties.Draw("displayParent");
                    Properties.Draw("VHSIcon");
                    Properties.Draw("displayFormat");
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["emissionMaterial"], new GUIContent("Emission Settings")))
                {
                    Properties.Draw("emissionMaterial");
                    Properties.Draw("emissionKeyword");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["monitor"], new GUIContent("Video Settings")))
                {
                    Properties.Draw("monitor");
                    Properties.Draw("outputTextureSize");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["rewindSpeed"], new GUIContent("Player Settings")))
                {
                    Properties.Draw("rewindSpeed");
                    Properties.Draw("fastForwardSpeed");
                    Properties.Draw("windingStartupSpeed");
                    Properties.Draw("timeBeforeWinding");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["fastForwardSymbol"], new GUIContent("Display Icons")))
                {
                    Properties.Draw("fastForwardSymbol");
                    Properties.Draw("rewindSymbol");
                    Properties.Draw("playSymbol");
                    Properties.Draw("stopSymbol");
                    Properties.Draw("pauseSymbol");
                    Properties.Draw("ejectSymbol");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["insertTrigger"], new GUIContent("Animation")))
                {
                    Properties.Draw("insertTrigger");
                    Properties.Draw("ejectTrigger");
                    Properties.Draw("closeCoverTrigger");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["tapeInsert"], new GUIContent("Sounds")))
                {
                    Properties.Draw("tapeInsert");
                    Properties.Draw("tapeEject");
                    Properties.Draw("play");
                    Properties.Draw("stop");
                    Properties.Draw("rewind");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}