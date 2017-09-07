using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class QuitApplication : MonoBehaviour {

	public void Quit()
	{
		//If we are running in a standalone build of the game
	#if UNITY_STANDALONE
		//Quit the application
		Application.Quit();
	#endif

		//If we are running in the editor
	#if UNITY_EDITOR
		//Stop playing the scene
		UnityEditor.EditorApplication.isPlaying = false;
	#endif
	}

	public void ReturnMainMenu()
	{
		//Set time.timescale to 1, this will cause animations and physics to continue updating at regular speed
		Time.timeScale = 1;
		SceneManager.LoadScene(0);
		Destroy(GameObject.Find("UI"));
	}
}
