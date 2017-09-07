using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingMode : MonoBehaviour {

	private bool trainingMode = true;
	private GameObject enemies;
	private GameObject training;

	void Start()
	{
		enemies = GameObject.Find("Enemies");
		training = GameObject.Find("TrainingDummies");
	}
	
	// Update is called once per frame
	void Update () 
	{

		if (enemies == null)
		{
			enemies = GameObject.Find("Enemies");
		}

		if (training == null)
		{
			training = GameObject.Find("TrainingDummies");
		}

		if (enemies != null && training != null)
		{
			if (trainingMode)
			{
				training.SetActive(true);
				enemies.SetActive(false);
			}
			else
			{
				training.SetActive(false);
				enemies.SetActive(true);
			}
		}

		//Debug.Log("enemies: " + enemies.name);
		//Debug.Log("training: " + training.name);

	}

	public bool TrainingModeCheck
	{
		get{ return trainingMode;}
	}

	public void TrainOn()
	{
		trainingMode = !trainingMode;
	}

}
