using System;
using System.Collections.Generic;
using UnityEngine;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    public class ProceduralCable : MonoBehaviour
    {
        [Serializable]
        public sealed class CableSettings
        {
            public Material CableMaterial;
            public int Steps = 20;
            public float Curvature = 1;
            public float Radius = 0.2f;
            public int RadiusStep = 6;
            public float ColliderRadius = 1f;
            public bool GenerateCollider = true;
            public Vector2 uvMultiply = Vector2.one;
        }

        public Transform _startTransform;
        public Vector3 CableStart
        {
            get
            {
                if (!_startTransform)
                    throw new NullReferenceException("Cable start transform does not exist!");

                if (_startTransform.IsChildOf(transform)) return _startTransform.localPosition;
                else return transform.InverseTransformPoint(_startTransform.position);
            }

            set
            {
                if (!_startTransform)
                    throw new NullReferenceException("Cable start transform does not exist!");

                if (_startTransform.IsChildOf(transform)) _startTransform.localPosition = value;
                else _startTransform.position = transform.TransformPoint(value);
            }
        }

        public Transform _endTransform;
        public Vector3 CableEnd
        {
            get
            {
                if (!_endTransform)
                    throw new NullReferenceException("Cable end transform does not exist!");

                if (_endTransform.IsChildOf(transform)) return _endTransform.localPosition;
                else return transform.InverseTransformPoint(_endTransform.position);
            }

            set
            {
                if (!_endTransform)
                    throw new NullReferenceException("Cable end transform does not exist!");

                if (_endTransform.IsChildOf(transform)) _endTransform.localPosition = value;
                else _endTransform.position = transform.TransformPoint(value);
            }
        }

        public Vector3 CurvatorePoint
        {
            get
            {
                Vector3 mid = Vector3.Lerp(_startTransform.position, _endTransform.position, .5f);
                return mid + Vector3.down * settings.Curvature;
            }
        }

        public CableSettings settings = new CableSettings();
        public bool manualGeneration = false;
        public bool drawGizmos = false;
        public bool drawCableGizmos = false;
        public bool cableGenerated = false;

        public List<CapsuleCollider> colliders = new List<CapsuleCollider>();
        public List<Vector3> curvatorePoints = new List<Vector3>();

        private MeshFilter CableMeshFilter
        {
            get
            {
                MeshFilter meshFilter = GetComponent<MeshFilter>();
                if (meshFilter == null)
                    meshFilter = gameObject.AddComponent<MeshFilter>();

                return meshFilter;
            }
        }

        private MeshRenderer CableRenderer
        {
            get
            {
                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                    meshRenderer = gameObject.AddComponent<MeshRenderer>();

                return meshRenderer;
            }
        }

        public void GenerateCable(Vector3 start, Vector3 end, bool isWorldPos = false)
        {
            GameObject _start = new GameObject("CableStart");
            _start.transform.SetParent(transform);
            _start.transform.localPosition = Vector3.zero;
            _startTransform = _start.transform;
            CableStart = isWorldPos ? transform.InverseTransformPoint(start) : start;

            ProceduralCablePoint firstEnd = _start.AddComponent<ProceduralCablePoint>();
            firstEnd.proceduralCable = this;

            GameObject _end = new GameObject("CableEnd");
            _end.transform.SetParent(transform);
            _end.transform.localPosition = Vector3.zero;
            _endTransform = _end.transform;
            CableEnd = isWorldPos ? transform.InverseTransformPoint(end) : end;

            ProceduralCablePoint secondEnd = _end.AddComponent<ProceduralCablePoint>();
            secondEnd.proceduralCable = this;

            cableGenerated = true;
            RegenerateCable();
        }

        public void RegenerateCable()
        {
            if (!cableGenerated) return;

            CableMeshFilter.sharedMesh = GenerateMesh();
            CableRenderer.material = settings.CableMaterial;
            GenerateCollider();

            curvatorePoints = new List<Vector3>();
            for (int i = 0; i <= settings.Steps; i++)
            {
                curvatorePoints.Add(PointPosition(i));
            }
        }

        private Mesh GenerateMesh()
        {
            Mesh mesh = new Mesh { name = "CableMesh" };

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            float lenght = 0;

            for (int i = 0; i <= settings.Steps; i++)
            {
                Vector3[] verticesForPoint = VerticesForPoint(i);
                for (int h = 0; h < verticesForPoint.Length; h++)
                {
                    vertices.Add(verticesForPoint[h]);
                    normals.Add((verticesForPoint[h] - PointPosition(i)).normalized);

                    uvs.Add(new Vector2(lenght * settings.uvMultiply.x, (float)h / (verticesForPoint.Length - 1) * settings.uvMultiply.y));

                    if (i < settings.Steps)
                    {
                        int index = h + (i * settings.RadiusStep);

                        triangles.Add(index);
                        triangles.Add(index + 1);
                        triangles.Add(index + settings.RadiusStep);

                        triangles.Add(index);
                        triangles.Add(index + settings.RadiusStep);
                        triangles.Add(index + settings.RadiusStep - 1);
                    }
                }

                lenght += (PointPosition(i + 1) - PointPosition(i)).magnitude;
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            return mesh;
        }

        private void GenerateCollider()
        {
            Transform oldMesh = transform.Find("Colliders");
            if (oldMesh != null) DestroyImmediate(oldMesh.gameObject);

            if (settings.GenerateCollider)
            {
                GameObject root = new GameObject("Colliders");
                root.transform.SetParent(transform);
                root.transform.localPosition = Vector3.zero;

                colliders.Clear();
                CapsuleCollider prevCollider = null;

                for (int i = 0; i < settings.Steps; i++)
                {
                    Vector3 start = PointPosition(i);
                    Vector3 end = PointPosition(i + 1);
                    float segLength = (end - start).magnitude;

                    GameObject colObj = new GameObject("Collider_" + i);
                    colObj.transform.SetParent(root.transform);
                    Vector3 center = Vector3.Lerp(start, end, 0.5f);
                    colObj.transform.localPosition = center;
                    colObj.transform.up = end - start;

                    CapsuleCollider collider = colObj.AddComponent<CapsuleCollider>();
                    collider.radius = settings.Radius * settings.ColliderRadius;
                    collider.height = segLength;
                    colliders.Add(collider);

                    if (prevCollider != null) Physics.IgnoreCollision(prevCollider, collider);
                    prevCollider = collider;
                }
            }
        }

        public Vector3 Eval(float t)
        {
            return VectorE.QuadraticBezier(CableStart, CableEnd, transform.InverseTransformPoint(CurvatorePoint), t);
        }

        public Vector3 EvalRaw(float t)
        {
            Vector3 cableStart = _startTransform.position;
            Vector3 cableEnd = _endTransform.position;
            return VectorE.QuadraticBezier(cableStart, cableEnd, CurvatorePoint, t);
        }

        private Vector3 PointPosition(int i)
        {
            return Eval((float)i / settings.Steps);
        }

        private Vector3[] VerticesForPoint(int i)
        {
            Vector3 pointPosition = PointPosition(i);
            Vector3 orientation;

            if (i == 0) orientation = PointPosition(1) - PointPosition(0);
            else if (i == settings.Steps) orientation = PointPosition(settings.Steps) - PointPosition(settings.Steps - 1);
            else orientation = PointPosition(i + 1) - PointPosition(i - 1);

            Quaternion rotation = Quaternion.LookRotation(orientation, Vector3.Cross(Vector3.down, CableEnd - CableStart));

            List<Vector3> vertices = new List<Vector3>();
            float angleStep = 360f / (settings.RadiusStep - 1);

            for (int h = 0; h < settings.RadiusStep; h++)
            {
                float angle = angleStep * h * Mathf.Deg2Rad;
                vertices.Add(pointPosition + rotation * new Vector3(Mathf.Cos(angle) * settings.Radius, Mathf.Sin(angle) * settings.Radius, 0));
            }

            return vertices.ToArray();
        }

        private void OnDrawGizmos()
        {
            if (!_startTransform || !_endTransform || !drawGizmos)
                return;

            Vector3 start = transform.TransformPoint(CableStart);
            Vector3 end = transform.TransformPoint(CableEnd);

            Color color = Color.green;
            color.a = 0.5f;

            Gizmos.color = color;
            Gizmos.DrawSphere(start, 0.05f);
            Gizmos.DrawSphere(end, 0.05f);

            if (drawCableGizmos)
            {
                Vector3 llp = VectorE.QuadraticBezier(start, end, CurvatorePoint, 0);
                Gizmos.color = Color.white.Alpha(0.5f);

                int steps = settings.Steps;
                for (int i = 1; i <= steps; i++)
                {
                    float t = i / (float)steps;
                    Vector3 lp = VectorE.QuadraticBezier(start, end, CurvatorePoint, t);
                    Gizmos.DrawLine(llp, lp);
                    llp = lp;
                }
            }
        }
    }
}