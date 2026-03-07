using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ch.sycoforge.Decal.Demo
{
	[RequireComponent(typeof(NavMeshAgent))]
	[RequireComponent(typeof(LineRenderer))]
	public class PathAgent : MonoBehaviour
	{
		public float PathThickness = 1f;

		[Tooltip("Distance from the ground.")]
		public float NormalPathOffset;

		[Tooltip("Max radius between segments.")]
		[Range(0.001f, 0.5f)]
		public float Radius = 0.25f;

		[Tooltip("Discard segments when their angle is smaller than this value.")]
		public float AngleThreshold = 5f;

		public bool DrawGizmos;

		public EasyDecal TargetAimDecal;

		public GameObject TargetPointDecalPrefab;

		private List<Vector3> path = new List<Vector3>();

		private NavMeshAgent agent;

		private LineRenderer lineRenderer;

		private Vector3 decalOffset = Vector3.up * 0.5f;

		private const int MAXDISTANCE = 50;

		private void Start()
		{
			TargetAimDecal.gameObject.SetActive(value: false);
			agent = GetComponent<NavMeshAgent>();
			lineRenderer = GetComponent<LineRenderer>();
		}

		private void Update()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			CreatePath(ray);
			SetTarget(ray);
		}

		private void SetTarget(Ray mouseRay)
		{
			if (Input.GetMouseButtonUp(0) && Physics.Raycast(mouseRay, out var hitInfo, 50f))
			{
				agent.SetDestination(hitInfo.point);
				EasyDecal.ProjectAt(TargetPointDecalPrefab, hitInfo.collider.gameObject, hitInfo.point + decalOffset, Quaternion.identity);
			}
		}

		private void CreatePath(Ray mouseRay)
		{
			if (Physics.Raycast(mouseRay, out var hitInfo, 50f))
			{
				Vector3 position = base.transform.position;
				Vector3 point = hitInfo.point;
				path.Clear();
				NavMeshPath navMeshPath = new NavMeshPath();
				if (NavMesh.CalculatePath(position, point, -1, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
				{
					int num = navMeshPath.corners.Length;
					Vector3 vector = base.transform.up;
					for (int i = 0; i < num; i++)
					{
						if (i > 0 && NormalPathOffset > 0f && Physics.Raycast(navMeshPath.corners[i], Vector3.down, out var _, NormalPathOffset * 10f))
						{
							vector = hitInfo.normal;
						}
						Vector3 item = navMeshPath.corners[i] + vector * NormalPathOffset;
						path.Add(item);
					}
					Vector3[] array = BezierUtil.InterpolatePath(path, 10, Radius, AngleThreshold).ToArray();
					lineRenderer.SetVertexCount(array.Length);
					lineRenderer.SetPositions(array);
					TargetAimDecal.gameObject.SetActive(value: true);
					TargetAimDecal.gameObject.transform.position = navMeshPath.corners[num - 1] + decalOffset;
					return;
				}
			}
			TargetAimDecal.gameObject.SetActive(value: false);
		}

		private void OnDrawGizmos()
		{
			if (!DrawGizmos)
			{
				return;
			}
			Gizmos.color = Color.red;
			foreach (Vector3 item in path)
			{
				Gizmos.DrawSphere(item, 0.05f);
			}
		}
	}
}
