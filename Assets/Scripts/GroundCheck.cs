using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour {

	public float checkDist = 3.1f;
	//GameObject ground;
	Vector3 origPos = Vector3.zero;

	// Use this for initialization
	void Start () {
		//ground = GameObject.FindGameObjectWithTag("Ground");
		origPos.y = transform.parent.transform.position.y;
	}
	
	// Update is called once per frame
	void Update () {

		origPos.x = transform.parent.transform.position.x;
		origPos.z = transform.parent.transform.position.z;

		RaycastHit hitInfo;
		#if UNITY_EDITOR
		// helper to visualise the ground check ray in the scene view
		Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * checkDist));
		#endif
		// 0.1f is a small offset to start the ray from inside the character
		// it is also good to note that the transform position in the sample assets is at the base of the character
		if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, checkDist))
		{
			//Debug.Log("grounded");
		}
		else
		{
			transform.parent.transform.position = origPos;
		}

	}
}
