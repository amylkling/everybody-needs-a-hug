using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//script meant to prevent enemies from clipping through walls

public class WallTrigger : MonoBehaviour {

	void OnTriggerStay(Collider col)
	{
		//when an enemy stays inside the trigger area, whether it is alive or not, 
		//reposition it near the player so that they can see it
		if (col.gameObject.CompareTag("Enemy") || col.gameObject.CompareTag("Incapacitated"))
		{
			GameObject player = GameObject.FindWithTag("Player");
			Vector3 newPos = new Vector3(player.transform.position.x + 0.5f, col.transform.position.y, player.transform.position.z + 0.5f);
			col.transform.position = newPos;
		}
	}
}
