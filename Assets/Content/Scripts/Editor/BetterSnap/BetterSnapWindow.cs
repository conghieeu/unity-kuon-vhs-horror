using UnityEngine;
using UnityEditor;

namespace BetterSnap
{
    public class BetterSnapWindow : EditorWindow
    {
        private bool isEnabled;
        private KeyCode hotkey1;
        private KeyCode hotkey2;
        private bool cursorVertexSnap;
        private bool cursorEdgeSnap;
        private float snapThreshold;
        private float edgeAngleThreshold;
        private bool showMarkers;
        private bool showHoverPreview;
        private Color selectionMarkerColor;
        private Color targetMarkerColor;
        private bool makeTargetTransparent;
        private float transparencyAmount;
        private bool includeChildren;

        [MenuItem("Tools/BetterSnap")]
        public static void ShowWindow()
        {
            GetWindow<BetterSnapWindow>("BetterSnap");
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void OnDisable()
        {
            SaveSettings();
        }

        private void LoadSettings()
        {
            isEnabled = EditorPrefs.GetBool("BetterSnap_Enabled", true);
            hotkey1 = (KeyCode)EditorPrefs.GetInt("BetterSnap_Hotkey1", (int)KeyCode.C);
            hotkey2 = (KeyCode)EditorPrefs.GetInt("BetterSnap_Hotkey2", (int)KeyCode.V);
            cursorVertexSnap = EditorPrefs.GetBool("BetterSnap_CursorVertexSnap", true);
            cursorEdgeSnap = EditorPrefs.GetBool("BetterSnap_CursorEdgeSnap", false);
            snapThreshold = EditorPrefs.GetFloat("BetterSnap_SnapDist", 0.2f);
            edgeAngleThreshold = EditorPrefs.GetFloat("BetterSnap_EdgeAngleThreshold", 90f);
            showMarkers = EditorPrefs.GetBool("BetterSnap_ShowMarkers", true);
            showHoverPreview = EditorPrefs.GetBool("BetterSnap_ShowHoverPreview", true);
            
            string selColorStr = EditorPrefs.GetString("BetterSnap_SelectionMarkerColor", "#00FF00FF");
            ColorUtility.TryParseHtmlString(selColorStr, out selectionMarkerColor);
            
            string tgtColorStr = EditorPrefs.GetString("BetterSnap_TargetMarkerColor", "#FF0000FF");
            ColorUtility.TryParseHtmlString(tgtColorStr, out targetMarkerColor);
            
            makeTargetTransparent = EditorPrefs.GetBool("BetterSnap_MakeTargetTransparent", true);
            transparencyAmount = EditorPrefs.GetFloat("BetterSnap_TransparencyAmount", 0.5f);
            includeChildren = EditorPrefs.GetBool("BetterSnap_IncludeChildren", true);
        }

        private void SaveSettings()
        {
            EditorPrefs.SetBool("BetterSnap_Enabled", isEnabled);
            EditorPrefs.SetInt("BetterSnap_Hotkey1", (int)hotkey1);
            EditorPrefs.SetInt("BetterSnap_Hotkey2", (int)hotkey2);
            EditorPrefs.SetBool("BetterSnap_CursorVertexSnap", cursorVertexSnap);
            EditorPrefs.SetBool("BetterSnap_CursorEdgeSnap", cursorEdgeSnap);
            EditorPrefs.SetFloat("BetterSnap_SnapDist", snapThreshold);
            EditorPrefs.SetFloat("BetterSnap_EdgeAngleThreshold", edgeAngleThreshold);
            EditorPrefs.SetBool("BetterSnap_ShowMarkers", showMarkers);
            EditorPrefs.SetBool("BetterSnap_ShowHoverPreview", showHoverPreview);
            
            EditorPrefs.SetString("BetterSnap_SelectionMarkerColor", "#" + ColorUtility.ToHtmlStringRGBA(selectionMarkerColor));
            EditorPrefs.SetString("BetterSnap_TargetMarkerColor", "#" + ColorUtility.ToHtmlStringRGBA(targetMarkerColor));
            
            EditorPrefs.SetBool("BetterSnap_MakeTargetTransparent", makeTargetTransparent);
            EditorPrefs.SetFloat("BetterSnap_TransparencyAmount", transparencyAmount);
            EditorPrefs.SetBool("BetterSnap_IncludeChildren", includeChildren);

            // Notify Core that settings changed
            BetterSnapCore.LoadSettings();
        }

        private void OnGUI()
        {
            GUILayout.Label("BetterSnap Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            isEnabled = EditorGUILayout.Toggle("Enable BetterSnap", isEnabled);
            
            EditorGUILayout.Space();
            GUILayout.Label("Hotkeys", EditorStyles.boldLabel);
            hotkey1 = (KeyCode)EditorGUILayout.EnumPopup("Source Anchor Hotkey (H1)", hotkey1);
            hotkey2 = (KeyCode)EditorGUILayout.EnumPopup("Target Snap Hotkey (H2)", hotkey2);

            EditorGUILayout.Space();
            GUILayout.Label("Cursor Settings", EditorStyles.boldLabel);
            cursorVertexSnap = EditorGUILayout.Toggle("Snap Cursor to Vertices", cursorVertexSnap);
            cursorEdgeSnap = EditorGUILayout.Toggle("Snap Cursor to Edges", cursorEdgeSnap);
            includeChildren = EditorGUILayout.Toggle("Include Children (As Block)", includeChildren);

            snapThreshold = EditorGUILayout.Slider("Snap Threshold", snapThreshold, 0.1f, 10f);
            if (cursorEdgeSnap)
            {
                edgeAngleThreshold = EditorGUILayout.Slider("Edge Angle Threshold", edgeAngleThreshold, 0f, 180f);
            }

            EditorGUILayout.Space();
            GUILayout.Label("Visuals", EditorStyles.boldLabel);
            showMarkers = EditorGUILayout.Toggle("Show Markers", showMarkers);
            showHoverPreview = EditorGUILayout.Toggle("Show Hover Preview", showHoverPreview);
            
            if (showMarkers)
            {
                selectionMarkerColor = EditorGUILayout.ColorField("Selection Color", selectionMarkerColor);
                targetMarkerColor = EditorGUILayout.ColorField("Target Color", targetMarkerColor);
            }

            makeTargetTransparent = EditorGUILayout.Toggle("Make Target Transparent", makeTargetTransparent);
            if (makeTargetTransparent)
            {
                transparencyAmount = EditorGUILayout.Slider("Transparency Amount", transparencyAmount, 0f, 1f);
            }

            if (EditorGUI.EndChangeCheck())
            {
                SaveSettings();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Reset to Defaults"))
            {
                EditorPrefs.DeleteKey("BetterSnap_Enabled");
                EditorPrefs.DeleteKey("BetterSnap_Hotkey1");
                EditorPrefs.DeleteKey("BetterSnap_Hotkey2");
                EditorPrefs.DeleteKey("BetterSnap_CursorVertexSnap");
                EditorPrefs.DeleteKey("BetterSnap_CursorEdgeSnap");
                EditorPrefs.DeleteKey("BetterSnap_SnapDist");
                EditorPrefs.DeleteKey("BetterSnap_EdgeAngleThreshold");
                EditorPrefs.DeleteKey("BetterSnap_ShowMarkers");
                EditorPrefs.DeleteKey("BetterSnap_ShowHoverPreview");
                EditorPrefs.DeleteKey("BetterSnap_SelectionMarkerColor");
                EditorPrefs.DeleteKey("BetterSnap_TargetMarkerColor");
                EditorPrefs.DeleteKey("BetterSnap_MakeTargetTransparent");
                EditorPrefs.DeleteKey("BetterSnap_TransparencyAmount");
                EditorPrefs.DeleteKey("BetterSnap_IncludeChildren");
                LoadSettings();
            }
        }
    }
}
