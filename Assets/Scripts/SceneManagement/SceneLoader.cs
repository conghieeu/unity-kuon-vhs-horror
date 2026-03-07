using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
	private bool loading;

	public void LoadScene(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}

	public void LoadScene(string sceneName, float delay)
	{
		if (!loading)
		{
			StartCoroutine(Delay(sceneName, delay));
		}
	}

	private IEnumerator Delay(string sceneName, float delay)
	{
		loading = true;
		yield return new WaitForSeconds(delay);
		SceneManager.LoadScene(sceneName);
	}
}
