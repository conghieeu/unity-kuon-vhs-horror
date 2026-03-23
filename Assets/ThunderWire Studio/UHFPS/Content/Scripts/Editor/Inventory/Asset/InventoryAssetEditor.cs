using UnityEngine;
using UnityEditor;
using UHFPS.Scriptable;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(InventoryAsset))]
    public class InventoryAssetEditor : InspectorEditor<InventoryAsset>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Inventory Asset"));
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Inventory items have been moved to the new asset Inventory Database. You can import old inventory items into the new database in the Inventory Builder window.", MessageType.Warning);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("Items");
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}