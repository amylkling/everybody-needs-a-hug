using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//REPLACED BY TRAININGOPTIONS
//controls which type of enemies to use

public class TrainingMode : MonoBehaviour {

	#region Variables
	private bool trainingMode = true;		//whether or not training dummies spawn
	private GameObject enemies;				//the holding object for the enemy objects
	private GameObject training;			//the holding object for the training dummy objects
	#endregion

	#region Start
	//initialize
	void Start()
	{
		enemies = GameObject.Find("Enemies");
		training = GameObject.Find("TrainingDummies");
	}
	#endregion

	#region Update
	// Update is called once per frame
	void Update () 
	{
		//if it can't find the objects, keep searching
		if (enemies == null)
		{
			enemies = GameObject.Find("Enemies");
		}

		if (training == null)
		{
			training = GameObject.Find("TrainingDummies");
		}

		//when the objects have been found, determine which type to activate
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
	#endregion

	#region Get Sets
	//so other scripts can check trainingMode
	public bool TrainingModeCheck
	{
		get{ return trainingMode;}
	}

	//so a menu option can toggle trainingMode
	public void TrainOn()
	{
		trainingMode = !trainingMode;
	}
	#endregion
}
