using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Controls the buttons on the exit menu of the training level

public class ContinueQuit : MonoBehaviour {

	#region Continue
	//changes to the main scene, with the main game
	public void Continue()
	{
		Time.timeScale = 1;
		SceneManager.LoadScene(1);
		//set trainingMode to false so the dummies don't appear
		if (GameObject.Find("UI") != null)
		{
			GameObject.Find("UI").GetComponent<TrainingOptions>().TrainingMode = false;
		}
	}
	#endregion

	#region ReturnMainMenu
	//changes to the main menu scene
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
	#endregion
}
