using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

//controls the player's hug ability

public class Huggles : MonoBehaviour {

	#region Variables
	GameObject target;							//the object that marks an enemy as a target
	public Enemy enemy;							//the control script of the enemy marked as a target
	bool hugSnap;								//local holder for the hugEngaged variable from the PlayerCharacter script
	public float staminaDrainRate = 10f;		//the amount of "damage" to do to the enemy every second
	private bool hugOff;						//controls the timed cancelling of a hug
	float hugTimer = 0f;						//timer for how long a hug lasts or how long until the player can hug again
	public float hugTimeLimit = 2f;				//amount of time until a hug gets cancelled automatically
	bool hugOn;									//controls the cool down for being able to hug again
	public float hugTime = 1f;					//amount of time until the player can hug again
	bool gHugComplete = false;						//controls activation of group hug
	#endregion

	#region Start
	// Use this for initialization
	void Start () 
	{
		target = GameObject.FindGameObjectWithTag("HugTarget");
		hugSnap = gameObject.GetComponent<PlayerCharacter>().HugEngaged;
		hugTimer = hugTimeLimit;
	}
	#endregion

	#region Update
	// Update is called once per frame
	void Update () 
	{
		//keep the status of hugSnap updated from the PlayerCharacter script
		hugSnap = gameObject.GetComponent<PlayerCharacter>().HugEngaged;

		//when an enemy has been locked on to, drain its stamina
		if (hugSnap && enemy != null)
		{
			enemy.Hugged = true;
			enemy.TakeDmg(staminaDrainRate * Time.deltaTime);
			//prevent the player from regenerating health
			if (gameObject.GetComponent<PlayerHealth>().changeRegen)
			{
				gameObject.GetComponent<PlayerHealth>().RegenHealthVar = false;
			}
			//disable the ability to use the group hug or kiss
			gameObject.GetComponent<PlayerControl>().FinishThem = false;
			gameObject.GetComponent<PlayerControl>().Kissie(false);
		}
		//when no more enemies can be targeted, and a group hug hasn't been activated yet, allow a group hug
		//and deactivate kissing
		else if (enemy == null)
		{
			if (!gHugComplete)
			{
				gameObject.GetComponent<PlayerControl>().FinishThem = true;
			}
			else
			{
				gameObject.GetComponent<PlayerControl>().FinishThem = false;
			}
			gameObject.GetComponent<PlayerControl>().Kissie(false);
		}
		//when no enemy has been locked on to, allow player health regen and kissing, and disallow group hug
		else
		{
			if (gameObject.GetComponent<PlayerHealth>().changeRegen)
			{
				gameObject.GetComponent<PlayerHealth>().RegenHealthVar = true;
			}
			enemy.Hugged = false;
			gameObject.GetComponent<PlayerControl>().FinishThem = false;
			gameObject.GetComponent<PlayerControl>().Kissie(true);
		}

		//timer for cancelling a hug if there is no target
		if (hugOff)
		{
			hugTimer -= Time.deltaTime;

			//if the player lets go of the button, reset
			if (CrossPlatformInputManager.GetButtonUp("Fire3"))
			{
				SetTimer(hugTimeLimit);
				hugOff = false;
			}

			if (hugTimer <= 0)
			{
				hugTimer = hugTime;
				gameObject.GetComponent<PlayerControl>().HugControl(false);
				hugOff = false;
				hugOn = true;
			}


		}

		//timer for re-enabling hugging after an unsuccessful hug
		if (hugOn)
		{
			hugTimer -= Time.deltaTime;
			if (hugTimer <= 0)
			{
				hugTimer = hugTimeLimit;
				gameObject.GetComponent<PlayerControl>().HugControl(true);
				hugOn = false;
			}
		}
	}
	#endregion

	#region Get Sets
	//for other scripts to set timer length
	public void SetTimer(float t)
	{
		hugTimer = t;
	}

	//for other scripts to check the status of hugOn
	public bool HugOn
	{
		get {return hugOn;}
	}

	//for other scripts to access hugOff
	public bool HugOff
	{
		get {return hugOff;}
		set {hugOff = value;}
	}

	//for other scripts to access gHugComplete
	public bool GroupHugComplete
	{
		set {gHugComplete = value;}
	}
	#endregion
}
