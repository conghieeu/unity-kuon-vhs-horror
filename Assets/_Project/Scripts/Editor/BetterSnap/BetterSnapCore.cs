using UnityEngine;
using UnityEditor;
namespace BetterSnap
{
    [InitializeOnLoad]
    public static class BetterSnapCore
    {
        // Settings
        public static bool isEnabled = true;
        public static KeyCode hotkey1 = KeyCode.C;
        public static KeyCode hotkey2 = KeyCode.V;
        public static bool cursorVertexSnap = true;
        public static bool cursorEdgeSnap = false;
        public static float snapThreshold = 0.2f;
        public static float edgeAngleThreshold = 90f;
        public static bool showMarkers = true;
        public static bool showHoverPreview = true;
        public static Color selectionMarkerColor = Color.green;
        public static Color targetMarkerColor = Color.red;
        public static bool makeTargetTransparent = true;
        public static float transparencyAmount = 0.5f;
        public static bool includeChildren = true;
        // State
        private static bool isHotkey1Held = false;
        private static bool isHotkey2Held = false;
        
        private static GameObject selectedObject;
        private static GameObject targetSnapObject;
        
        private static bool sourceAnchorSet = false;
        private static bool isCustomAnchorSet = false;
        private static bool autoAnchorCalculated = false;
        private static Vector3 sourceAnchorLocal = Vector3.zero;
        private static Vector3 targetSnapVertex = Vector3.zero;
        
        private static int lastHotControl = 0;
        private static Vector3 dragStartObjPos;
        private static Quaternion dragStartObjRot;
        private static Vector3 dragStartObjScale;
        private static Vector3 dragStartAnchorPos;
        private static Quaternion dragStartAnchorRot;
        
        private static System.Collections.Generic.Dictionary<Renderer, Material> originalMaterials = new System.Collections.Generic.Dictionary<Renderer, Material>();
        private static System.Collections.Generic.List<Material> createdMaterials = new System.Collections.Generic.List<Material>();
        
        private static GameObject cachedPickedObject = null;
        static BetterSnapCore()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            Selection.selectionChanged += OnSelectionChanged;
            SetupGlobalEventHandler();
            LoadSettings();
        }
        private static void SetupGlobalEventHandler()
        {
            System.Reflection.FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (info != null)
            {
                EditorApplication.CallbackFunction cb = (EditorApplication.CallbackFunction)info.GetValue(null);
                if (cb != null) cb -= OnGlobalEventHandler;
                cb += OnGlobalEventHandler;
                info.SetValue(null, cb);
            }
        }
        private static void OnGlobalEventHandler()
        {
            if (!isEnabled) return;
            Event e = Event.current;
            if (e == null || !e.isKey) return;
            if (EditorGUIUtility.editingTextField) return;
            SceneView sceneView = EditorWindow.mouseOverWindow as SceneView;
            if (sceneView != null)
            {
                bool stateChanged = false;
                if (e.type == EventType.KeyDown)
                {
                    if (e.keyCode == hotkey1 && !isHotkey1Held) { isHotkey1Held = true; stateChanged = true; }
                    if (e.keyCode == hotkey2 && !isHotkey2Held) { isHotkey2Held = true; stateChanged = true; }
                }
                else if (e.type == EventType.KeyUp)
                {
                    if (e.keyCode == hotkey1 && isHotkey1Held) { isHotkey1Held = false; stateChanged = true; }
                    if (e.keyCode == hotkey2 && isHotkey2Held)
                    {
                        isHotkey2Held = false;
                        autoAnchorCalculated = false;
                        ResetTargetTransparency();
                        stateChanged = true;
                    }
                }
                if (stateChanged) sceneView.Repaint();
            }
            else
            {
                isHotkey1Held = false;
                if (isHotkey2Held)
                {
                    isHotkey2Held = false;
                    autoAnchorCalculated = false;
                    ResetTargetTransparency();
                }
            }
        }
        private static void OnSelectionChanged()
        {
            // Reset anchor when selection changes
            sourceAnchorSet = false;
            isCustomAnchorSet = false;
            autoAnchorCalculated = false;
            sourceAnchorLocal = Vector3.zero;
            ResetTargetTransparency();
            Tools.hidden = false;
            lastHotControl = 0;
            SceneView.RepaintAll();
        }
        public static void LoadSettings()
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
            if (ColorUtility.TryParseHtmlString(selColorStr, out Color sColor)) selectionMarkerColor = sColor;
            
            string tgtColorStr = EditorPrefs.GetString("BetterSnap_TargetMarkerColor", "#FF0000FF");
            if (ColorUtility.TryParseHtmlString(tgtColorStr, out Color tColor)) targetMarkerColor = tColor;
            
            makeTargetTransparent = EditorPrefs.GetBool("BetterSnap_MakeTargetTransparent", true);
            transparencyAmount = EditorPrefs.GetFloat("BetterSnap_TransparencyAmount", 0.5f);
            includeChildren = EditorPrefs.GetBool("BetterSnap_IncludeChildren", true);
        }
        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!isEnabled) 
            {
                Tools.hidden = false;
                return;
            }
            if (sceneView != null)
            {
                sceneView.wantsMouseMove = true;
            }
            Event e = Event.current;
            
            // Safely cache picked object outside of rendering phases to prevent GUI layout crashes
            if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag || e.type == EventType.MouseDown || e.type == EventType.MouseEnterWindow)
            if (e.type != EventType.Repaint && e.type != EventType.Layout && e.type != EventType.Ignore && e.type != EventType.Used)
            {
                cachedPickedObject = HandleUtility.PickGameObject(e.mousePosition, false);
            }
            selectedObject = Selection.activeGameObject;
            if (selectedObject == null) return;
            // Prevent Unity from complaining about changing layout
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            // Key Events are now handled globally via OnGlobalEventHandler to bypass Keyboard Focus issues.
            // Handle Key Events
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == hotkey1) isHotkey1Held = true;
                if (e.keyCode == hotkey2) isHotkey2Held = true;
            }
            else if (e.type == EventType.KeyUp)
            {
                if (e.keyCode == hotkey1) isHotkey1Held = false;
                if (e.keyCode == hotkey2)
                {
                    isHotkey2Held = false;
                    autoAnchorCalculated = false; // Reset auto calculation so next V press recalculates
                    ResetTargetTransparency();
                }
            }
            if (!isHotkey1Held && !isHotkey2Held)
            {
                // Draw the solid green cursor if it's already set and override Unity Tools
                if (sourceAnchorSet && isCustomAnchorSet)
                {
                    Vector3 worldAnchor = selectedObject.transform.TransformPoint(sourceAnchorLocal);
                    if (showMarkers)
                    {
                        Handles.color = selectionMarkerColor;
                        Handles.SphereHandleCap(0, worldAnchor, Quaternion.identity, HandleUtility.GetHandleSize(worldAnchor) * 0.1f, EventType.Repaint);
                    }

                    int hotControl = GUIUtility.hotControl;
                    if (hotControl != 0 && lastHotControl == 0)
                    {
                        dragStartObjPos = selectedObject.transform.position;
                        dragStartObjRot = selectedObject.transform.rotation;
                        dragStartObjScale = selectedObject.transform.localScale;
                        dragStartAnchorPos = worldAnchor;
                        dragStartAnchorRot = Tools.pivotRotation == PivotRotation.Local ? selectedObject.transform.rotation : Quaternion.identity;
                    }
                    lastHotControl = hotControl;

                    if (Tools.current == Tool.Move || Tools.current == Tool.Rotate || Tools.current == Tool.Scale)
                    {
                        Tools.hidden = true;
                        
                        Quaternion displayRot = (hotControl != 0) ? dragStartAnchorRot : (Tools.pivotRotation == PivotRotation.Local ? selectedObject.transform.rotation : Quaternion.identity);
                        Vector3 displayPos = (hotControl != 0 && Tools.current != Tool.Move) ? dragStartAnchorPos : worldAnchor;

                        EditorGUI.BeginChangeCheck();
                        
                        if (Tools.current == Tool.Move)
                        {
                            Vector3 newPos = Handles.PositionHandle(displayPos, displayRot);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(selectedObject.transform, "Move via Custom Anchor");
                                selectedObject.transform.position = dragStartObjPos + (newPos - dragStartAnchorPos);
                            }
                        }
                        else if (Tools.current == Tool.Rotate)
                        {
                            Quaternion newRot = Handles.RotationHandle(displayRot, displayPos);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(selectedObject.transform, "Rotate via Custom Anchor");
                                Quaternion absoluteDeltaRot = newRot * Quaternion.Inverse(dragStartAnchorRot);
                                selectedObject.transform.rotation = absoluteDeltaRot * dragStartObjRot;

                                Vector3 newAnchorWorld = selectedObject.transform.TransformPoint(sourceAnchorLocal);
                                selectedObject.transform.position += (dragStartAnchorPos - newAnchorWorld);
                            }
                        }
                        else if (Tools.current == Tool.Scale)
                        {
                            Vector3 currentScale = (hotControl != 0) ? dragStartObjScale : selectedObject.transform.localScale;
                            Vector3 newScale = Handles.ScaleHandle(currentScale, displayPos, displayRot, HandleUtility.GetHandleSize(displayPos));
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(selectedObject.transform, "Scale via Custom Anchor");
                                selectedObject.transform.localScale = newScale;

                                Vector3 newAnchorWorld = selectedObject.transform.TransformPoint(sourceAnchorLocal);
                                selectedObject.transform.position += (dragStartAnchorPos - newAnchorWorld);
                            }
                        }
                    }
                    else
                    {
                        Tools.hidden = false;
                    }
                }
                else
                {
                    Tools.hidden = false;
                }

                if (!showHoverPreview) return;
                Ray hoverRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                RaycastHit[] hoverHits = Physics.RaycastAll(hoverRay);
                bool hoverHit = false;
                RaycastHit hoverHitInfo = new RaycastHit();
                float hoverClosestDist = float.MaxValue;
                
                foreach (var h in hoverHits)
                {
                    bool isSelf = includeChildren ? h.collider.transform.IsChildOf(selectedObject.transform) : h.collider.gameObject == selectedObject;
                    if (!isSelf && h.distance < hoverClosestDist)
                    {
                        hoverHit = true;
                        hoverHitInfo = h;
                        hoverClosestDist = h.distance;
                    }
                }
                GameObject hoverTarget = null;
                Vector3 hoverTargetPoint = Vector3.zero;
                bool validHover = false;
                if (hoverHit)
                {
                    hoverTarget = hoverHitInfo.collider.gameObject;
                    hoverTargetPoint = hoverHitInfo.point;
                    validHover = true;
                }
                else
                {
                    GameObject picked = cachedPickedObject;
                    if (picked != null)
                    {
                        bool isSelf = includeChildren ? picked.transform.IsChildOf(selectedObject.transform) : picked == selectedObject;
                        if (!isSelf)
                        {
                            hoverTarget = picked;
                            hoverTargetPoint = GetNearestVertexToRay(picked, hoverRay, out bool foundVert);
                            if (foundVert) validHover = true;
                        }
                    }
                }
                if (validHover)
                {
                    if (hoverHit) hoverTargetPoint = GetNearestSnapPoint(hoverTarget, hoverTargetPoint, snapThreshold);
                    Vector3 previewSourceAnchorWorld;
                    if (sourceAnchorSet && isCustomAnchorSet)
                    {
                        previewSourceAnchorWorld = selectedObject.transform.TransformPoint(sourceAnchorLocal);
                    }
                    else
                    {
                        previewSourceAnchorWorld = CalculateAutoSourceAnchor(selectedObject, hoverTarget, hoverTargetPoint);
                    }
                    if (showMarkers)
                    {
                        // Draw red point on target
                        Handles.color = targetMarkerColor;
                        Handles.SphereHandleCap(0, hoverTargetPoint, Quaternion.identity, HandleUtility.GetHandleSize(hoverTargetPoint) * 0.1f, EventType.Repaint);
                        
                        // Draw yellow dashed line
                        Handles.color = Color.yellow;
                        Handles.DrawDottedLine(previewSourceAnchorWorld, hoverTargetPoint, 4f);
                    }
                }
                
                // Ensure scene view repaints to show hover preview smoothly
                if (e.type == EventType.MouseMove)
                {
                    sceneView.Repaint();
                }
                return;
            }
            // Consume event during layout to prevent normal interaction while holding hotkeys
            if (e.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(controlID);
            }
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            // Logic for Hotkey 1 (Set Source Anchor)
            if (isHotkey1Held)
            {
                RaycastHit[] hits = Physics.RaycastAll(ray);
                bool hitSelected = false;
                RaycastHit selectedHit = new RaycastHit();
                float closestDist = float.MaxValue;
                
                foreach (var h in hits)
                {
                    bool isSelf = includeChildren ? h.collider.transform.IsChildOf(selectedObject.transform) : h.collider.gameObject == selectedObject;
                    if (isSelf && h.distance < closestDist)
                    {
                        hitSelected = true;
                        closestDist = h.distance;
                        selectedHit = h;
                    }
                }
                Vector3 surfacePoint = Vector3.zero;
                bool validSurface = false;
                if (hitSelected)
                {
                    surfacePoint = selectedHit.point;
                    validSurface = true;
                }
                else
                {
                    GameObject picked = cachedPickedObject;
                    if (picked != null)
                    {
                        bool isSelf = includeChildren ? picked.transform.IsChildOf(selectedObject.transform) : picked == selectedObject;
                        if (isSelf)
                        {
                            surfacePoint = GetNearestVertexToRay(selectedObject, ray, out bool foundVert);
                            if (foundVert) validSurface = true;
                        }
                    }
                }
                if (validSurface)
                {
                    if (hitSelected) surfacePoint = GetNearestSnapPoint(selectedObject, surfacePoint, snapThreshold);
                    
                    sourceAnchorLocal = selectedObject.transform.InverseTransformPoint(surfacePoint);
                    sourceAnchorSet = true;
                    isCustomAnchorSet = true;
                    if (showMarkers)
                    {
                        Handles.color = selectionMarkerColor;
                        Handles.SphereHandleCap(0, surfacePoint, Quaternion.identity, HandleUtility.GetHandleSize(surfacePoint) * 0.1f, EventType.Repaint);
                    }
                }
                sceneView.Repaint();
            }
            
            // Logic for Hotkey 2 (Snap to Target)
            else if (isHotkey2Held)
            {
                RaycastHit[] hits = Physics.RaycastAll(ray);
                bool hit = false;
                RaycastHit hitInfo = new RaycastHit();
                float closestDist = float.MaxValue;
                
                foreach (var h in hits)
                {
                    bool isSelf = includeChildren ? h.collider.transform.IsChildOf(selectedObject.transform) : h.collider.gameObject == selectedObject;
                    if (!isSelf && h.distance < closestDist)
                    {
                        hit = true;
                        hitInfo = h;
                        closestDist = h.distance;
                    }
                }
                GameObject targetObj = null;
                Vector3 surfacePoint = Vector3.zero;
                bool validTarget = false;
                if (hit)
                {
                    targetObj = hitInfo.collider.gameObject;
                    surfacePoint = hitInfo.point;
                    validTarget = true;
                }
                else
                {
                    GameObject picked = cachedPickedObject;
                    if (picked != null)
                    {
                        bool isSelf = includeChildren ? picked.transform.IsChildOf(selectedObject.transform) : picked == selectedObject;
                        if (!isSelf)
                        {
                            targetObj = picked;
                            surfacePoint = GetNearestVertexToRay(picked, ray, out bool foundVert);
                            if (foundVert) validTarget = true;
                        }
                    }
                }
                if (validTarget && targetObj != selectedObject)
                {
                    GameObject newTarget = targetObj;
                    
                    if (newTarget != targetSnapObject)
                    {
                        ResetTargetTransparency();
                        targetSnapObject = newTarget;
                        ApplyTargetTransparency(targetSnapObject);
                    }
                    if (hit) surfacePoint = GetNearestSnapPoint(targetSnapObject, surfacePoint, snapThreshold);
                    
                    targetSnapVertex = surfacePoint;
                    if (!isCustomAnchorSet && !autoAnchorCalculated)
                    {
                        Vector3 closestV = CalculateAutoSourceAnchor(selectedObject, targetObj, targetSnapVertex);
                        sourceAnchorLocal = selectedObject.transform.InverseTransformPoint(closestV);
                        sourceAnchorSet = true;
                        autoAnchorCalculated = true;
                    }
                    // Realtime preview
                    Vector3 worldAnchor = selectedObject.transform.TransformPoint(sourceAnchorLocal);
                    Vector3 offset = targetSnapVertex - worldAnchor;
                    
                    if (offset.sqrMagnitude > 0.0001f)
                    {
                        Undo.RecordObject(selectedObject.transform, "BetterSnap Live Preview");
                        selectedObject.transform.position += offset;
                    }
                    if (showMarkers)
                    {
                        Handles.color = targetMarkerColor;
                        Handles.SphereHandleCap(0, targetSnapVertex, Quaternion.identity, HandleUtility.GetHandleSize(targetSnapVertex) * 0.1f, EventType.Repaint);
                        
                        Handles.color = selectionMarkerColor;
                        Handles.SphereHandleCap(0, targetSnapVertex, Quaternion.identity, HandleUtility.GetHandleSize(targetSnapVertex) * 0.08f, EventType.Repaint);
                    }
                }
                else
                {
                    ResetTargetTransparency();
                }
                sceneView.Repaint();
            }
            // Consume mouse events to override default tools while hotkey is held
            if (e.isMouse)
            {
                e.Use();
            }
        }
        private static Vector3 CalculateAutoSourceAnchor(GameObject selectedObj, GameObject targetObj, Vector3 fallbackPoint)
        {
            Vector3 centerA = selectedObj.transform.position;
            bool hasBounds = false;
            Bounds bounds = new Bounds(centerA, Vector3.zero);
            if (includeChildren)
            {
                foreach (Collider col in selectedObj.GetComponentsInChildren<Collider>())
                {
                    if (!hasBounds) { bounds = col.bounds; hasBounds = true; }
                    else bounds.Encapsulate(col.bounds);
                }
                foreach (Renderer r in selectedObj.GetComponentsInChildren<Renderer>())
                {
                    if (!hasBounds) { bounds = r.bounds; hasBounds = true; }
                    else bounds.Encapsulate(r.bounds);
                }
            }
            else
            {
                Collider colA = selectedObj.GetComponent<Collider>();
                if (colA != null) { bounds = colA.bounds; hasBounds = true; }
            }
            if (hasBounds) centerA = bounds.center;
            Vector3 closestOnB = fallbackPoint;
            
            Collider[] targetCols = includeChildren ? targetObj.GetComponentsInChildren<Collider>() : new Collider[] { targetObj.GetComponent<Collider>() };
            float minDB = float.MaxValue;
            bool foundCol = false;
            foreach (var c in targetCols)
            {
                if (c == null) continue;
                Vector3 pt;
                if (c is MeshCollider mc && !mc.convex) pt = c.bounds.ClosestPoint(centerA);
                else pt = c.ClosestPoint(centerA);
                float d = (pt - centerA).sqrMagnitude;
                if (d < minDB) { minDB = d; closestOnB = pt; foundCol = true; }
            }
            return GetClosestSurfacePoint(selectedObj, closestOnB);
        }
        private static Vector3 GetNearestSnapPoint(GameObject obj, Vector3 point, float threshold)
        {
            if (cursorVertexSnap)
            {
                Vector3 v = GetNearestVertexIfWithinThreshold(obj, point, threshold);
                if (v != point)
                {
                    return v; // Prioritize vertex (corner) over edge
                }
            }
            if (cursorEdgeSnap)
            {
                Vector3 e = GetNearestEdgePointIfWithinThreshold(obj, point, threshold);
                if (e != point)
                {
                    return e;
                }
            }
            return point;
        }
        private struct Edge
        {
            public Vector3 p1;
            public Vector3 p2;
            public Edge(Vector3 p1, Vector3 p2)
            {
                this.p1 = p1;
                this.p2 = p2;
            }
        }
        private static System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<Edge>> sharpEdgesCache = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<Edge>>();
        private static System.Collections.Generic.Dictionary<int, float> cachedEdgeAngles = new System.Collections.Generic.Dictionary<int, float>();
        private static void CalculateSharpEdges(Mesh mesh, float maxAngle)
        {
            int meshId = mesh.GetInstanceID();
            if (sharpEdgesCache.ContainsKey(meshId) && cachedEdgeAngles.ContainsKey(meshId) && cachedEdgeAngles[meshId] == maxAngle)
            {
                return;
            }
            System.Collections.Generic.List<Edge> sharpEdges = new System.Collections.Generic.List<Edge>();
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            System.Collections.Generic.Dictionary<long, System.Collections.Generic.List<int>> edgeToTriangles = new System.Collections.Generic.Dictionary<long, System.Collections.Generic.List<int>>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    int v1 = triangles[i + j];
                    int v2 = triangles[i + ((j + 1) % 3)];
                    long edgeKey = v1 < v2 ? ((long)v1 << 32) | (uint)v2 : ((long)v2 << 32) | (uint)v1;
                    if (!edgeToTriangles.ContainsKey(edgeKey))
                    {
                        edgeToTriangles[edgeKey] = new System.Collections.Generic.List<int>();
                    }
                    edgeToTriangles[edgeKey].Add(i / 3);
                }
            }
            Vector3[] normals = new Vector3[triangles.Length / 3];
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 p1 = vertices[triangles[i]];
                Vector3 p2 = vertices[triangles[i + 1]];
                Vector3 p3 = vertices[triangles[i + 2]];
                normals[i / 3] = Vector3.Cross(p2 - p1, p3 - p1).normalized;
            }
            foreach (var kvp in edgeToTriangles)
            {
                System.Collections.Generic.List<int> tris = kvp.Value;
                int v1 = (int)(kvp.Key >> 32);
                int v2 = (int)(kvp.Key & 0xFFFFFFFF);
                if (tris.Count == 1)
                {
                    sharpEdges.Add(new Edge(vertices[v1], vertices[v2]));
                }
                else if (tris.Count >= 2)
                {
                    Vector3 n1 = normals[tris[0]];
                    Vector3 n2 = normals[tris[1]];
                    float faceAngle = 180f - Vector3.Angle(n1, n2);
                    if (faceAngle <= maxAngle)
                    {
                        sharpEdges.Add(new Edge(vertices[v1], vertices[v2]));
                    }
                }
            }
            sharpEdgesCache[meshId] = sharpEdges;
            cachedEdgeAngles[meshId] = maxAngle;
        }
        private static Vector3 ClosestPointOnLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 lineDirection = lineEnd - lineStart;
            float lineLength = lineDirection.magnitude;
            if (lineLength == 0f) return lineStart;
            lineDirection /= lineLength;
            Vector3 pointToStart = point - lineStart;
            float dotProduct = Vector3.Dot(pointToStart, lineDirection);
            if (dotProduct <= 0f) return lineStart;
            if (dotProduct >= lineLength) return lineEnd;
            return lineStart + lineDirection * dotProduct;
        }
        private struct MeshData
        {
            public Mesh mesh;
            public Transform transform;
            public Renderer renderer;
        }
        private static System.Collections.Generic.List<MeshData> GetAllMeshes(GameObject obj)
        {
            System.Collections.Generic.List<MeshData> results = new System.Collections.Generic.List<MeshData>();
            
            MeshFilter[] meshFilters = includeChildren ? obj.GetComponentsInChildren<MeshFilter>() : new MeshFilter[] { obj.GetComponent<MeshFilter>() };
            foreach (var mf in meshFilters)
            {
                if (mf != null && mf.sharedMesh != null)
                {
                    results.Add(new MeshData { mesh = mf.sharedMesh, transform = mf.transform, renderer = mf.GetComponent<Renderer>() });
                }
            }
            SkinnedMeshRenderer[] smrs = includeChildren ? obj.GetComponentsInChildren<SkinnedMeshRenderer>() : new SkinnedMeshRenderer[] { obj.GetComponent<SkinnedMeshRenderer>() };
            foreach (var smr in smrs)
            {
                if (smr != null && smr.sharedMesh != null)
                {
                    results.Add(new MeshData { mesh = smr.sharedMesh, transform = smr.transform, renderer = smr });
                }
            }
            return results;
        }
        private static Vector3 GetNearestVertexToRay(GameObject obj, Ray ray, out bool found)
        {
            System.Collections.Generic.List<MeshData> meshes = GetAllMeshes(obj);
            Vector3 nearestWorldPoint = Vector3.zero;
            float minScore = float.MaxValue;
            found = false;
            foreach (var mData in meshes)
            {
                Transform t = mData.transform;
                foreach (Vector3 vertex in mData.mesh.vertices)
                {
                    Vector3 worldVertex = t.TransformPoint(vertex);
                    Vector3 toVertex = worldVertex - ray.origin;
                    float dot = Vector3.Dot(ray.direction, toVertex);
                    if (dot > 0) // In front of camera
                    {
                        float perpDistSq = Vector3.Cross(ray.direction, toVertex).sqrMagnitude;
                        float score = perpDistSq < 0.1f ? perpDistSq + dot * 0.1f : perpDistSq;
                        if (score < minScore)
                        {
                            minScore = score;
                            nearestWorldPoint = worldVertex;
                            found = true;
                        }
                    }
                }
            }
            return nearestWorldPoint;
        }
        private static Vector3 GetNearestEdgePointIfWithinThreshold(GameObject obj, Vector3 point, float threshold)
        {
            System.Collections.Generic.List<MeshData> meshes = GetAllMeshes(obj);
            
            float minDistanceSq = float.MaxValue;
            float thresholdSq = threshold * threshold;
            float boundsMarginSq = (threshold + 5.0f) * (threshold + 5.0f);
            Vector3 nearestWorldPoint = point;
            bool found = false;
            foreach (var mData in meshes)
            {
                if (mData.renderer != null && mData.renderer.bounds.SqrDistance(point) > boundsMarginSq) continue;
                Mesh mesh = mData.mesh;
                CalculateSharpEdges(mesh, edgeAngleThreshold);
                System.Collections.Generic.List<Edge> sharpEdges = sharpEdgesCache[mesh.GetInstanceID()];
                if (sharpEdges.Count == 0) continue;
                Transform t = mData.transform;
                foreach (Edge edge in sharpEdges)
                {
                    Vector3 worldP1 = t.TransformPoint(edge.p1);
                    Vector3 worldP2 = t.TransformPoint(edge.p2);
                    Vector3 closest = ClosestPointOnLineSegment(point, worldP1, worldP2);
                    float distSq = (closest - point).sqrMagnitude;
                    
                    if (distSq < minDistanceSq)
                    {
                        minDistanceSq = distSq;
                        nearestWorldPoint = closest;
                        found = true;
                    }
                }
            }
            if (found && minDistanceSq <= thresholdSq)
            {
                return nearestWorldPoint;
            }
            return point;
        }
        private static Vector3 GetNearestVertexIfWithinThreshold(GameObject obj, Vector3 point, float threshold)
        {
            System.Collections.Generic.List<MeshData> meshes = GetAllMeshes(obj);
            Vector3 nearestWorldPoint = point;
            float minDistanceSq = float.MaxValue;
            float thresholdSq = threshold * threshold;
            float boundsMarginSq = (threshold + 5.0f) * (threshold + 5.0f);
            bool found = false;
            foreach (var mData in meshes)
            {
                if (mData.renderer != null && mData.renderer.bounds.SqrDistance(point) > boundsMarginSq) continue;
                Transform t = mData.transform;
                foreach (Vector3 vertex in mData.mesh.vertices)
                {
                    Vector3 worldVertex = t.TransformPoint(vertex);
                    float distSq = (worldVertex - point).sqrMagnitude;
                    
                    if (distSq < minDistanceSq)
                    {
                        minDistanceSq = distSq;
                        nearestWorldPoint = worldVertex;
                        found = true;
                    }
                }
            }
            if (found && minDistanceSq <= thresholdSq)
            {
                return nearestWorldPoint;
            }
            
            return point;
        }
        private static Vector3 GetClosestSurfacePoint(GameObject obj, Vector3 point)
        {
            if (cursorVertexSnap)
            {
                Vector3 nearestV = GetNearestVertexOnly(obj, point);
                if (nearestV != obj.transform.position) 
                {
                    return nearestV;
                }
            }
            Collider[] colliders = includeChildren ? obj.GetComponentsInChildren<Collider>() : new Collider[] { obj.GetComponent<Collider>() };
            
            Vector3 nearestPoint = point;
            float minDistance = float.MaxValue;
            bool foundCol = false;
            foreach (Collider col in colliders)
            {
                if (col == null) continue;
                foundCol = true;
                
                Vector3 closest;
                if (col is MeshCollider mc && !mc.convex)
                {
                    closest = col.bounds.ClosestPoint(point);
                }
                else
                {
                    closest = col.ClosestPoint(point);
                }
                float dist = Vector3.Distance(point, closest);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestPoint = closest;
                }
            }
            if (foundCol) return nearestPoint;
            return GetNearestVertexOnly(obj, point);
        }
        private static Vector3 GetNearestVertexOnly(GameObject obj, Vector3 point)
        {
            System.Collections.Generic.List<MeshData> meshes = GetAllMeshes(obj);
            Vector3 nearestWorldPoint = point; // Or obj.transform.position
            float minDistanceSq = float.MaxValue;
            bool found = false;
            foreach (var mData in meshes)
            {
                Transform t = mData.transform;
                foreach (Vector3 vertex in mData.mesh.vertices)
                {
                    Vector3 worldVertex = t.TransformPoint(vertex);
                    float distSq = (worldVertex - point).sqrMagnitude;
                    
                    if (distSq < minDistanceSq)
                    {
                        minDistanceSq = distSq;
                        nearestWorldPoint = worldVertex;
                        found = true;
                    }
                }
            }
            return found ? nearestWorldPoint : obj.transform.position;
        }
        private static void ApplyTargetTransparency(GameObject target)
        {
            if (!makeTargetTransparent) return;
            Renderer[] renderers = includeChildren ? target.GetComponentsInChildren<Renderer>() : new Renderer[] { target.GetComponent<Renderer>() };
            
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    Material origMat = renderer.sharedMaterial;
                    Material transMat = new Material(origMat);
                    
                    transMat.SetFloat("_Mode", 3);
                    transMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    transMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    transMat.SetInt("_ZWrite", 0);
                    transMat.DisableKeyword("_ALPHATEST_ON");
                    transMat.EnableKeyword("_ALPHABLEND_ON");
                    transMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    transMat.renderQueue = 3000;
                    
                    Color c = transMat.color;
                    c.a = transparencyAmount;
                    transMat.color = c;
                    renderer.sharedMaterial = transMat;
                    
                    originalMaterials[renderer] = origMat;
                    createdMaterials.Add(transMat);
                }
            }
        }
        private static void ResetTargetTransparency()
        {
            if (targetSnapObject != null)
            {
                foreach (var kvp in originalMaterials)
                {
                    if (kvp.Key != null)
                    {
                        kvp.Key.sharedMaterial = kvp.Value;
                    }
                }
            }
            
            foreach (Material mat in createdMaterials)
            {
                if (mat != null) Object.DestroyImmediate(mat);
            }
            originalMaterials.Clear();
            createdMaterials.Clear();
            targetSnapObject = null;
        }
    }
}