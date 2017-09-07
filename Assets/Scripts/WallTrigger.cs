using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallTrigger : MonoBehaviour {

	void OnTriggerStay(Collider col)
	{
		if (col.gameObject.CompareTag("Enemy") || col.gameObject.CompareTag("Incapacitated"))
		{
			GameObject player = GameObject.FindWithTag("Player");
			Vector3 newPos = new Vector3(player.transform.position.x + 0.5f, col.transform.position.y, player.transform.position.z + 0.5f);
			col.transform.position = newPos;
		}
	}
}
