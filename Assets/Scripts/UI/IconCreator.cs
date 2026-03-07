using UnityEngine;

public static class IconCreator
{
	public static GameObject CreatePlane(float width, float height, bool collider, Material mat)
	{
		GameObject gameObject = new GameObject("Plane");
		MeshFilter meshFilter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
		MeshRenderer meshRenderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[4]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(width, 0f, 0f),
			new Vector3(width, height, 0f),
			new Vector3(0f, height, 0f)
		};
		mesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f)
		};
		mesh.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
		meshFilter.mesh = mesh;
		if (collider)
		{
			(gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider).sharedMesh = mesh;
		}
		meshRenderer.material = mat;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		return gameObject;
	}
}
