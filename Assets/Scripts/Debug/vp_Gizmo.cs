using UnityEngine;

public class vp_Gizmo : MonoBehaviour
{
	public Color gizmoColor = new Color(1f, 1f, 1f, 0.4f);

	public Color selectedGizmoColor = new Color(1f, 1f, 1f, 0.4f);

	protected Collider m_Collider;

	protected Collider Collider
	{
		get
		{
			if (m_Collider == null)
			{
				m_Collider = GetComponent<Collider>();
			}
			return m_Collider;
		}
	}

	public void OnDrawGizmos()
	{
		if (Collider != null)
		{
			Vector3 center = Collider.bounds.center;
			Vector3 size = Collider.bounds.size;
			Gizmos.color = gizmoColor;
			Gizmos.DrawCube(center, size);
			Gizmos.color = new Color(0f, 0f, 0f, 1f);
			Gizmos.DrawLine(Vector3.zero, Vector3.forward);
		}
	}

	public void OnDrawGizmosSelected()
	{
		Vector3 center = Collider.bounds.center;
		Vector3 size = Collider.bounds.size;
		Gizmos.color = selectedGizmoColor;
		Gizmos.DrawCube(center, size);
		Gizmos.color = new Color(0f, 0f, 0f, 1f);
		Gizmos.DrawLine(Vector3.zero, Vector3.forward);
	}
}
