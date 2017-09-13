using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//keeps the object above the ground, in case it clips through somehow
//portions of this code copied from the Standard Asset First Person RigidBody Controller

public class GroundCheck : MonoBehaviour {

	#region Variables
	public float checkDist = 3.1f;			//maximum distance for the raycast
	Vector3 origPos = Vector3.zero;			//holds the position the object is supposed to be at
	#endregion

	#region Start
	// Use this for initialization
	void Start () 
	{
		//set the base y vector at start so it can be reset to this point if need be
		origPos.y = transform.parent.transform.position.y;
	}
	#endregion

	#region Update
	// Update is called once per frame
	void Update () 
	{
		//always remember what x and z position the object was at so it doesn't appear to shift anywhere
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
			//reset the object's position if it is not above ground
			transform.parent.transform.position = origPos;
		}
	}
	#endregion
}
