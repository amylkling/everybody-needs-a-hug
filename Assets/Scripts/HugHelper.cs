using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

//an object used to assist with the hug mechanic
//by controlling targeting and player rotation

public class HugHelper : MonoBehaviour {

	#region Variables
	public GameObject[] enemies;			//array to hold all current enemies that are not incapacitated
	public GameObject targetedEnemy;		//reference the enemy that is determined to be closest
	public float turnSpeed = 180f;			//determines how sharply the player turns when attached to this object
	Vector3 transCenter;					//holds the new position for this object
	private GameObject player;				//reference the player object
	public float turnAmount = 90f;			//how much the player turns when attached to this object
	private bool isInUse = false;			//ensures that the player can't hold down the turning buttons
	public Enemy enemyScript;				//reference the enemy control script on the targeted enemy
	#endregion

	#region Start
	// Use this for initialization
	void Start () 
	{
		enemies = GameObject.FindGameObjectsWithTag("Enemy");
		player = GameObject.FindGameObjectWithTag("Player");
	}
	#endregion

	#region Update
	// Update is called once per frame
	void Update () 
	{
		//get a target
		enemies = GameObject.FindGameObjectsWithTag("Enemy");
		DetermineClosest();
		if (targetedEnemy != null)
		{
			enemyScript = targetedEnemy.GetComponent<Enemy>();
		}


		//do stuff only when there are targets available
		if (enemies.GetLength(0) != 0)
		{
			//and the target isn't incapacitated yet
			if (!enemyScript.Dead)
			{
				//pass the enemy's control script to the Huggles script so it can determine "damage", along with other things
				player.GetComponent<Huggles>().enemy = enemyScript;

				//determine pivot point for hugging
				transCenter = new Vector3(targetedEnemy.transform.position.x, 0, targetedEnemy.transform.position.z);
				transform.position = transCenter;

				//rotate by turnAmount according to input but only when the player is childed to this object
				if (transform.childCount != 0)
				{
					if (CrossPlatformInputManager.GetAxisRaw("Horizontal") > 0)
					{
						if (!isInUse)
						{
							transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
							isInUse = true;
						}

					}
					else if (CrossPlatformInputManager.GetAxisRaw("Horizontal") < 0)
					{
						if (!isInUse)
						{
							transform.Rotate(0, turnAmount * -1 * turnSpeed * Time.deltaTime, 0);
							isInUse = true;
						}
					}
					else if (CrossPlatformInputManager.GetAxisRaw("Horizontal") == 0)
					{
						isInUse = false;
					}

				}
				else
				{
					//when the player is no longer childed, reset rotation
					transform.rotation = Quaternion.identity;
				}
			}
			else
			{
				//once the enemy "dies",
				//allow the player's scripts time to remove the player from this object before marking it as incapacitated
				//this prevents the player from teleporting to each subsequent enemy as this object does
				//WHICH WAS A VERY ANNOYING PROBLEM
				if (transform.childCount == 0)
				{
					enemyScript.gameObject.tag = "Incapacitated";
				}
			}
		}
		else
		{
			//when all the enemies are incapacitated, reset this object out of the way
			player.GetComponent<Huggles>().enemy = null;
			transCenter = new Vector3(0, 100, 0);
			transform.position = transCenter;
		}
	}
	#endregion

	#region DetermineClosest
	//compare each enemy's distance to the player and select the closest one
	void DetermineClosest ()
	{
		float minDist = Mathf.Infinity;
		Vector3 currentPos = player.transform.position;
		foreach (GameObject t in enemies)
		{
			float dist = Vector3.Distance(t.transform.position, currentPos);
			if (dist < minDist)
			{
				targetedEnemy = t;
				minDist = dist;
			}
		}
	}
	#endregion
}
