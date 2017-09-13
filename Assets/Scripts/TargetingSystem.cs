using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this script controls the camera so that it can follow an enemy the player chooses to target

namespace UnityStandardAssets.Cameras
{
	public class TargetingSystem : MonoBehaviour {

		#region Variables
		private GameObject camera;				//reference to the main camera object
		public GameObject[] enemies;			//array of enemies in the scene
		public GameObject closestEnemy;			//reference to the closest enemy object
		private GameObject player;				//reference to the player object
		public AudioSource soundfx;				//audiosource to feed the sound effects to
		public AudioClip error;					//error sound effect for when it is unable to target
		#endregion

		#region Start
		// Use this for initialization
		void Start () 
		{
			camera = GameObject.FindGameObjectWithTag("MainCamera");
			enemies = GameObject.FindGameObjectsWithTag("Enemy");
			player = GameObject.FindGameObjectWithTag("Player");
			soundfx = camera.GetComponent<AudioSource>();
		
		}
		#endregion
	
		#region Update
		// Update is called once per frame
		void Update()
		{
			//get all of the enemies in the scene and determine which is closest to the player
			enemies = GameObject.FindGameObjectsWithTag("Enemy");
			DetermineClosest();

			//when the player presses the Fire1 button
			if (Input.GetButtonDown("Fire1"))
			{
				//tell the camera's look at script to target the closest enemy that isn't incapacitated
				if (closestEnemy != null && enemies.GetLength(0) != 0)
				{
					camera.GetComponent<LookatTarget>().SetTarget(closestEnemy.transform);
				}
				else
				{
					//play a sound
					soundfx.PlayOneShot(error);
				}
			}

			//when the player presses the Fire2 button
			if (Input.GetButtonDown("Fire2"))
			{
				//tell the camera's look at script to target the player
				camera.GetComponent<LookatTarget>().SetTarget(player.transform);
			}

			//if the player leaves the screen, automatically switch back to them
			Vector3 pScreenPoint = camera.GetComponent<Camera>().WorldToViewportPoint(player.transform.position);
			if (pScreenPoint.x < 0 || pScreenPoint.x > 1 || pScreenPoint.y < 0 || pScreenPoint.y > 1)
			{
				camera.GetComponent<LookatTarget>().SetTarget(player.transform);
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
				if (t != null)
				{
					float dist = Vector3.Distance(t.transform.position, currentPos);
					if (dist < minDist)
					{
						closestEnemy = t;
						minDist = dist;
					}
				}
			}
		}
		#endregion
	}
}
