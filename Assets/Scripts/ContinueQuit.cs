using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueQuit : MonoBehaviour {

	public void Continue()
	{
		Time.timeScale = 1;
		SceneManager.LoadScene(1);
		if (GameObject.Find("UI") != null)
		{
			GameObject.Find("UI").GetComponent<TrainingOptions>().TrainingMode = false;
		}
	}

	public void ReturnMainMenu()
	{
		//Set time.timescale to 1, this will cause animations and physics to continue updating at regular speed
		Time.timeScale = 1;
		SceneManager.LoadScene(0);
		if (GameObject.Find("UI") != null)
		{
			Destroy(GameObject.Find("UI"));
		}
	}
}
