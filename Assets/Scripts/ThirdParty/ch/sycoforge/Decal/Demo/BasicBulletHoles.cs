using UnityEngine;

namespace ch.sycoforge.Decal.Demo
{
	public class BasicBulletHoles : MonoBehaviour
	{
		public EasyDecal DecalPrefab;
		private bool t;

		public void Start()
		{
			if (DecalPrefab == null)
			{
				Debug.LogError("The DynamicDemo script has no decal prefab attached.");
			}
		}

		public void Update()
		{
			if (!Input.GetMouseButtonUp(0))
			{
				return;
			}
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out var hitInfo, 200f))
			{
				Debug.DrawLine(ray.origin, hitInfo.point, Color.red);
				EasyDecal easyDecal = EasyDecal.ProjectAt(DecalPrefab.gameObject, hitInfo.collider.gameObject, hitInfo.point, hitInfo.normal);
				t = !t;
				if (t)
				{
					easyDecal.CancelFade();
				}
			}
		}
	}
}
