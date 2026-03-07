using UnityEngine;

public class Paraglider : MonoBehaviour
{
	public Animator paraglider;

	public CarMotor carMotor;

	private void Start()
	{
		carMotor = GetComponent<CarMotor>();
	}

	private void Update()
	{
		if (carMotor.velocity > 5f)
		{
			paraglider.SetBool("Rise", value: true);
		}
		else
		{
			paraglider.SetBool("Rise", value: false);
		}
	}
}
