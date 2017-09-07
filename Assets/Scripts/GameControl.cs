using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//controls all misc. aspects of the game
//such as score and UI and other management things

public class GameControl : MonoBehaviour {

	public GameObject enemyTrainingPrefab;
	public GameObject enemyPrefab;
	public GameObject enemyGroup;
	private GameObject player;
	public Canvas mainCanvas;
	public int dummyRespawnCount = 4;
	public int enemyRespawnCount = 4;
	public float spawnRadius = 5f;
	public Text scoreText;
	private float scoreTotal;
	bool trainingMode;
	public GameObject gameOverPanel;
	private Pause pauseScript;
	private bool gameOver = false;
	public Text loseText;
	public Text winText;
	public Text finalScoreText;
	private int roundCountDown;
	private int roundCountUp;
	public int wavesNum = 2;
	public int healthReward = 30;				//amount of health to get back after a group hug
	public Slider gHugMeter;
	public Text waveNumText;
	public AudioSource soundfx;
	public AudioClip winSound;
	public AudioClip loseSound;
	public GameObject playerHealth;

	public GameObject storyPanel;
	public GameObject tutorialPanel;
	public GameObject continuePanel;
	public Text[] tutorialTexts;
	public Text tutorialText1;
	public Text tutorialText2;
	public Text tutorialText3;
	public Text tutorialText4;
	public Text tutorialText5;
	public Text tutorialText6;
	public Text tutorialText7;
	public Text tutorialText8;
	public Text tutorialText9;
	public Text tutorialText10;
	public float tutorialUITime = 20f;
	private float tutorialUITimer = 0;
	private bool tutorialActive = false;
	private bool tutOn = false;
	private bool tutOff = false;

	// Use this for initialization
	void Start () 
	{
		enemyGroup = GameObject.FindGameObjectWithTag("Holder");
		player = GameObject.FindGameObjectWithTag("Player");
		scoreText = GameObject.Find("Score").GetComponent<Text>();
		scoreText.text = "Score: ";
		scoreTotal = 0;
		if (GameObject.Find("UI") != null)
		{
			trainingMode = GameObject.Find("UI").GetComponent<TrainingOptions>().TrainingMode;
			pauseScript = GameObject.Find("UI").GetComponent<Pause>();
		}
		playerHealth = GameObject.Find("PlayerHealth");

		//for debug/testing training scene
		#if UNITY_EDITOR
		//trainingMode = true;
		#endif

		if (!trainingMode)
		{
			gameOverPanel = GameObject.Find("GameOverPanel");
			winText = GameObject.Find("WinText").GetComponent<Text>();
			loseText = GameObject.Find("LoseText").GetComponent<Text>();
			finalScoreText = GameObject.Find("Score (Final)").GetComponent<Text>();
			gameOverPanel.SetActive(false);
		}
		else
		{
			//set up tutorial and story panels/texts
			//and the tutorial finished one too
			storyPanel = GameObject.Find("StoryPanel");
			tutorialPanel = GameObject.Find("TutorialPanel");
			continuePanel = GameObject.Find("ContinuePanel");
			tutorialText1 = tutorialPanel.transform.GetChild(9).GetComponent<Text>();
			tutorialText2 = tutorialPanel.transform.GetChild(8).GetComponent<Text>();
			tutorialText3 = tutorialPanel.transform.GetChild(7).GetComponent<Text>();
			tutorialText4 = tutorialPanel.transform.GetChild(6).GetComponent<Text>();
			tutorialText5 = tutorialPanel.transform.GetChild(5).GetComponent<Text>();
			tutorialText6 = tutorialPanel.transform.GetChild(4).GetComponent<Text>();
			tutorialText7 = tutorialPanel.transform.GetChild(3).GetComponent<Text>();
			tutorialText8 = tutorialPanel.transform.GetChild(2).GetComponent<Text>();
			tutorialText9 = tutorialPanel.transform.GetChild(1).GetComponent<Text>();
			tutorialText10 = tutorialPanel.transform.GetChild(0).GetComponent<Text>();
			tutorialText2.enabled = false;
			tutorialText3.enabled = false;
			tutorialText4.enabled = false;
			tutorialText5.enabled = false;
			tutorialText6.enabled = false;
			tutorialText7.enabled = false;
			tutorialText8.enabled = false;
			tutorialText9.enabled = false;
			tutorialText10.enabled = false;
			tutorialTexts = new Text[10];
			tutorialTexts[0] = tutorialText10;
			Debug.Log(tutorialTexts[0].name + " is loaded!");
			tutorialTexts[1] = tutorialText9;
			Debug.Log(tutorialTexts[1].name + " is loaded!");
			tutorialTexts[2] = tutorialText8;
			Debug.Log(tutorialTexts[2].name + " is loaded!");
			tutorialTexts[3] = tutorialText7;
			Debug.Log(tutorialTexts[3].name + " is loaded!");
			tutorialTexts[4] = tutorialText6;
			Debug.Log(tutorialTexts[4].name + " is loaded!");
			tutorialTexts[5] = tutorialText5;
			Debug.Log(tutorialTexts[5].name + " is loaded!");
			tutorialTexts[6] = tutorialText4;
			Debug.Log(tutorialTexts[6].name + " is loaded!");
			tutorialTexts[7] = tutorialText3;
			Debug.Log(tutorialTexts[7].name + " is loaded!");
			tutorialTexts[8] = tutorialText2;
			Debug.Log(tutorialTexts[8].name + " is loaded!");
			tutorialTexts[9] = tutorialText1;
			Debug.Log(tutorialTexts[9].name + " is loaded!");
			tutorialPanel.SetActive(false);
			continuePanel.SetActive(false);
			tutorialUITimer = tutorialUITime;
			//Set time.timescale to 0, this will cause animations and physics to stop updating
			Time.timeScale = 0;
		}
		gameOver = false;
		roundCountDown = wavesNum;
		gHugMeter = GameObject.Find("GroupHugMeter").GetComponent<Slider>();
		roundCountUp = 1;
		waveNumText = GameObject.Find("WaveNumber").GetComponent<Text>();
		waveNumText.text = "Wave: " + roundCountUp.ToString("D");
		soundfx = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		//find the gameobject that holds all the enemies
		enemyGroup = GameObject.FindGameObjectWithTag("Holder");

		//keep tabs on if it's in training mode or not
		if (GameObject.Find("UI") != null)
		{
			trainingMode = GameObject.Find("UI").GetComponent<TrainingOptions>().TrainingMode;
		}

		//for debug/testing training scene
		#if UNITY_EDITOR
		//trainingMode = true;
		#endif

		//set the wave counter and control tutorial/story text
		if (trainingMode)
		{
			waveNumText.text = "Wave: " + roundCountUp.ToString("D") + "/" + "∞";

			//story panel pops up at start
			//it has a button that links to a function in this script that disables it
			//once disabled, tutorial text starts popping up and disappearing based on a timer
			//once the last tutorial text has shown up, the final panel appears to continue the game
			if (tutorialActive && !tutOn)
			{
				tutorialPanel.SetActive(true);
				playerHealth.GetComponent<CanvasGroup>().alpha = 0.5f;
				gHugMeter.GetComponent<CanvasGroup>().alpha = 0.5f;
				scoreText.GetComponent<CanvasGroup>().alpha = 0.5f;
				waveNumText.GetComponent<CanvasGroup>().alpha = 0.5f;
				StartCoroutine(ShowTutorial());
			}
			else if (tutorialActive & tutOff)
			{
				tutorialPanel.SetActive(false);
				playerHealth.GetComponent<CanvasGroup>().alpha = 1f;
				gHugMeter.GetComponent<CanvasGroup>().alpha = 1f;
				scoreText.GetComponent<CanvasGroup>().alpha = 1f;
				waveNumText.GetComponent<CanvasGroup>().alpha = 1f;
				//end the tutorial
				if(Input.GetButtonDown("End"))
				{
					//press ` to end tutorial
					continuePanel.SetActive(true);
					//Set time.timescale to 0, this will cause animations and physics to stop updating
					Time.timeScale = 0;
				}
			}
			else if (!tutorialActive)
			{
				tutorialPanel.SetActive(false);
			}
		}
		else
		{
			tutorialActive = false;
			waveNumText.text = "Wave: " + roundCountUp.ToString("D") + "/" + (wavesNum + 1).ToString("D");
		}

		//if there are no more enemies
		if (enemyGroup.transform.childCount == 0)
		{
			//destroy the group hug particle efect, if it still exists
			if (player.GetComponent<PlayerCharacter>().GHugParticle != null)
			{
				Destroy(player.GetComponent<PlayerCharacter>().GHugParticle);
			}

			gHugMeter.value = gHugMeter.minValue;

			//make sure the player is able to take damage again
			player.GetComponent<PlayerHealth>().Invincible = false;

			//reward the player with health
			if (player.GetComponent<PlayerHealth>().changeRegen)
			{
				player.GetComponent<PlayerHealth>().RegenHealthVar = false;
				player.GetComponent<PlayerHealth>().Heal(healthReward);
			}

			//if the enemies were training dummies, respawn the set amount of them
			if (trainingMode)
			{
				for (int i = dummyRespawnCount; i > 0; i--)
				{
					Vector3 newPosition = Random.insideUnitSphere * spawnRadius + player.transform.position;
					newPosition.y = 0.7f;
					GameObject newEnemy = Instantiate(enemyTrainingPrefab, newPosition,
						Quaternion.identity, enemyGroup.transform);
					newEnemy.name = "Training Dummy " + i;
				}
				roundCountUp++;
				waveNumText.text = "Wave: " + roundCountUp.ToString("D") + "/" + "∞";
			}
			else
			{
				//otherwise they are AI enemies, so respawn the set amount of them
				//as long as there are still rounds left
				//when all rounds are complete, the player wins
				if (roundCountDown > 0)
				{
					for (int i = enemyRespawnCount; i > 0; i--)
					{
						Vector3 newPosition = Random.insideUnitSphere * spawnRadius * 2f + player.transform.position;
						newPosition.y = 0.7f;
						GameObject newEnemy = Instantiate(enemyPrefab, newPosition,
							Quaternion.identity, enemyGroup.transform);
						newEnemy.name = "Enemy " + i;
					}
					roundCountUp++;
					waveNumText.text = "Wave: " + roundCountUp.ToString("D") + "/" + (wavesNum + 1).ToString("D");
					roundCountDown--;
				}
				else
				{
					if (!gameOver)
					{
						StartCoroutine(GameOver(true));
					}

				}
			}

			player.GetComponent<Huggles>().NoMore = false;
		}
	}

	//update the score UI
	public void Scoreboard(float score)
	{
		scoreTotal += score;
		scoreText.text = "Score: " + scoreTotal.ToString("0000");
	}

	//when the game is over, pause everything and display the game over screen
	//the bool determines which message appears
	public IEnumerator GameOver(bool win)
	{
		gameOver = true;
		if(pauseScript != null)
		{
			pauseScript.Paused = true;
		}
		//Set time.timescale to 0, this will cause animations and physics to stop updating
		Time.timeScale = 0;
		//deactivate all UI then activate the game over panel
		for(int i = mainCanvas.transform.childCount; i > 0; i--)
		{
			mainCanvas.transform.GetChild(i-1).gameObject.SetActive(false);
			Debug.Log(i + ": " + mainCanvas.transform.GetChild(i-1).gameObject.name);
		}
		gameOverPanel.SetActive(true);
		//change message depending on circumstance of game over
		if (win)
		{
			soundfx.PlayOneShot(winSound);
			winText.gameObject.SetActive(true);
			loseText.gameObject.SetActive(false);
		}
		else
		{
			soundfx.PlayOneShot(loseSound);
			winText.gameObject.SetActive(false);
			loseText.gameObject.SetActive(true);
		}
		//show final score
		finalScoreText.text = "Final " + scoreText.text;
		yield break;
	}

	//reset the game back to the main menu
	public void ResetGame()
	{
		if(pauseScript != null)
		{
			pauseScript.Paused = false;
		}
		if (GameObject.Find("UI") != null)
		{
			Destroy(GameObject.Find("UI"));
		}
		//Set time.timescale to 1, this will cause animations and physics to continue updating at regular speed
		Time.timeScale = 1;
		SceneManager.LoadScene(0);
	}

	public void ContinueTraining()
	{
		if (storyPanel.activeSelf)
		{
			storyPanel.SetActive(false);
			tutorialActive = true;
		}
		else if (continuePanel.activeSelf)
		{
			continuePanel.SetActive(false);
		}
		//Set time.timescale to 1, this will cause animations and physics to continue updating at regular speed
		Time.timeScale = 1;
	}

	private IEnumerator ShowTutorial()
	{
		Debug.Log("tutorial on");
		tutOn = true;
		//for each tutorial text
		for (int i = tutorialPanel.transform.childCount; i > 0; i--)
		{
			//display it for a certain amount of time
			tutorialTexts[i-1].enabled = true;
			yield return new WaitForSeconds(tutorialUITime);
			tutorialTexts[i-1].enabled = false;
			Debug.Log("tutorial off");
		}
		tutOn = false;
		tutOff = true;
	}

	//so other scripts can see if the game is over
	public bool CheckGameOver
	{
		get {return gameOver;}
	}

	//so other scripts can see if the tutorial is going
	public bool CheckTutOn
	{
		get {return tutOn;}
	}
}
