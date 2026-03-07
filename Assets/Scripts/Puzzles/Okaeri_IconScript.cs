using UnityEngine;

public class Okaeri_IconScript : MonoBehaviour
{
	private GameObject player;

	public float appearDist = 3f;

	public GameObject interactIcon;

	[Space]
	public bool repeatable = true;

	private InteractManager intScript;

	private bool interacted;

	private void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player");
		intScript = InteractManager.Instance;
	}

	private void Update()
	{
		if (Vector3.Distance(player.transform.position, base.transform.position) < appearDist)
		{
			if (!interacted)
			{
				interactIcon.SetActive(value: true);
			}
			if (!repeatable && intScript.InteractingObject == base.gameObject && Input.GetKeyDown(intScript.interactKey))
			{
				interactIcon.SetActive(value: false);
				interacted = true;
			}
		}
		else
		{
			interactIcon.SetActive(value: false);
		}
	}
}
