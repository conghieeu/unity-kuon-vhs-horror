using System.Collections.Generic;
using UnityEngine;

namespace ch.sycoforge.Decal.Demo
{
	public static class LineUtil
	{
		public static void DrawPath(float thickness, Material material, List<Vector3> path)
		{
			if (path != null && (path == null || path.Count >= 2))
			{
				if (thickness <= Mathf.Epsilon)
				{
					GL.Begin(1);
				}
				else
				{
					GL.Begin(7);
				}
				material.SetPass(0);
				GL.Color(Color.blue);
				Vector3 start = path[0];
				for (int i = 1; i < path.Count; i++)
				{
					Vector3 vector = path[i];
					DrawLine(thickness, start, vector);
					start = vector;
				}
				GL.End();
			}
		}

		private static void DrawLine(float thickness, Vector3 start, Vector3 end)
		{
			if (thickness <= Mathf.Epsilon)
			{
				GL.Vertex(start);
				GL.Vertex(end);
				return;
			}
			Camera main = Camera.main;
			Vector3 normalized = (end - start).normalized;
			Vector3 vector = Vector3.Cross((start - main.transform.position).normalized, normalized) * (thickness / 2f);
			GL.Vertex(start - vector);
			GL.Vertex(start + vector);
			GL.Vertex(end + vector);
			GL.Vertex(end - vector);
		}
	}
}
