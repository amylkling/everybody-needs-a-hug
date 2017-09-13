using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//controls the kiss particles

public class ParticleControl : MonoBehaviour {

	#region Variables
	//basics
	private float timer = 0f;			//counts down to the particle's destruction
	public float aliveTime = 3f;		//amount of time for the particle to live

	//homing effect
	public GameObject targetObj;		//reference to the targeting helper
	public GameObject target;			//the target enemy
	private Vector3 targetPos;			//the target enemy's position
	public float moveSpeed = 0.5f;		//the speed the particle should move at
	#endregion

	#region Awake
	// Use this for initialization
	void Awake () 
	{
		timer = aliveTime;

		//initialize variables for homing effect
		//targetObj = GameObject.FindGameObjectWithTag ("HugTarget");
		//target = targetObj.GetComponent<HugHelper>().targetedEnemy;
		//targetPos = target.transform.position;

	}
	#endregion

	#region Update
	// Update is called once per frame
	void Update () 
	{
		//makes the particle home in on the enemy
		//targetPos = target.transform.position;
		//transform.position = Vector3.Slerp(transform.position, targetPos, moveSpeed * Time.deltaTime);

		//destroy the particle after its time is up
		timer -= Time.deltaTime;
		if (timer <= 0)
		{
			Destroy(gameObject);
		}
	
	}
	#endregion
}
