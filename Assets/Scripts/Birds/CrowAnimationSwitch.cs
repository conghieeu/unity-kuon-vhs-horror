using UnityEngine;

public class CrowAnimationSwitch : MonoBehaviour
{
	private Animation anim;

	private void Start()
	{
		anim = GetComponent<Animation>();
	}

	public void SwitchAnimation(string switchToName)
	{
		anim.Play(switchToName);
	}
}
