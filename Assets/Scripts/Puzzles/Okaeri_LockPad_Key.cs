using UnityEngine;

public class Okaeri_LockPad_Key : MonoBehaviour
{
	public Okaeri_LockPad master;

	public int num;

	public Animator anim;

	private InteractManager manager;

	private void Start()
	{
		anim = GetComponent<Animator>();
		manager = InteractManager.Instance;
	}

	private void Update()
	{
		if (Input.GetKeyDown(manager.interactKey) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo) && hitInfo.transform.gameObject == base.gameObject)
		{
			master.ButtonPress(base.gameObject.GetComponent<Okaeri_LockPad_Key>());
		}
	}
}
