using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Cameras
{
	public class TargetingSystem : MonoBehaviour {

		private GameObject camera;
		public GameObject[] enemies;
		public bool toggle = false;
		//private int count = -1;
		public GameObject closestEnemy;
		private GameObject player;
		private bool inRange;
		public AudioSource soundfx;
		public AudioClip error;

		// Use this for initialization
		void Start () 
		{
			camera = GameObject.FindGameObjectWithTag("MainCamera");
			enemies = GameObject.FindGameObjectsWithTag("Enemy");
			player = GameObject.FindGameObjectWithTag("Player");
			soundfx = camera.GetComponent<AudioSource>();
		
		}
	
		// Update is called once per frame
		void Update()
		{
			enemies = GameObject.FindGameObjectsWithTag("Enemy");
			DetermineClosest();


			if (Input.GetButtonDown("Fire1"))
			{
				//tell the camera's look at script to target the closest enemy that isn't incapacitated
				if (closestEnemy != null && enemies.GetLength(0) != 0)
				{
					camera.GetComponent<LookatTarget>().SetTarget(closestEnemy.transform);
				}
				else
				{
					//play a sound?
					soundfx.PlayOneShot(error);
				}

				#region prototype script
				//cycle targeting through an array of enemies
				/*
				count++;
				if (count >= 0 && count < enemies.Length)
				{
					camera.GetComponent<LookatTarget>().SetTarget(enemies[count].transform);
				}
				else
				{
					count = -1;
					camera.GetComponent<LookatTarget>().SetTarget(player.transform);
				}
				*/


				//basic toggle based enemy targeting
				/*
				toggle = !toggle;
				if(toggle)
					camera.GetComponent<LookatTarget>().SetTarget(enemy1.transform);
				else
					camera.GetComponent<LookatTarget>().SetTarget(player.transform);
				*/
				#endregion
			}

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


			//if the targeted enemy leaves the screen, switch back to player
			//this was just insurance, but I don't think it's needed
		
		}

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
	}
}
