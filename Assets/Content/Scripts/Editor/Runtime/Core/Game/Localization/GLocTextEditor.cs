using UHFPS.Runtime;
using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;
using UHFPS.Scriptable;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(GLocText)), CanEditMultipleObjects]
    public class GLocTextEditor : InspectorEditor<GLocText>
    {
        GameLocaizationTable localizationTable;

        public override void OnEnable()
        {
            base.OnEnable();

            if (GameLocalization.HasReference)
                localizationTable = GameLocalization.Instance.LocalizationTable;
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Gloc Text (Localization)"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("GlocKey");
                Properties.Draw("ObserveMany");

                EditorGUILayout.Space();
                Properties.Draw("OnUpdateText");

                EditorGUILayout.Space();
                using (new EditorGUI.DisabledGroupScope(localizationTable == null))
                {
                    if (GUILayout.Button("Ping Localization Table", GUILayout.Height(25)))
                    {
                        EditorGUIUtility.PingObject(localizationTable);
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}