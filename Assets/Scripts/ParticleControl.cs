using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleControl : MonoBehaviour {

	private float timer = 0f;
	public float aliveTime = 3f;
	public GameObject targetObj;
	private Vector3 targetPos;
	public GameObject target;
	public float moveSpeed = 0.5f;

	// Use this for initialization
	void Awake () {
		timer = aliveTime;
		//targetObj = GameObject.FindGameObjectWithTag ("HugTarget");
		//target = targetObj.GetComponent<HugHelper>().targetedEnemy;
		//targetPos = target.transform.position;

	}
	
	// Update is called once per frame
	void Update () {
		//targetPos = target.transform.position;
		//transform.position = Vector3.Slerp(transform.position, targetPos, moveSpeed * Time.deltaTime);

		timer -= Time.deltaTime;
		if (timer <= 0)
		{
			Destroy(gameObject);
		}
	
	}
}
