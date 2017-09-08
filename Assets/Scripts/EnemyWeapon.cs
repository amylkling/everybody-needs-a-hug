using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//controls how the enemy interacts with the player through damage

public class EnemyWeapon : MonoBehaviour {

	#region Variables
	public float dmgAmount = 20f;			//amount of damage to do to the player
	public float huggedDmgAmt = 10f;		//amount of damage done to the player while being hugged
	public bool constDmg = false;			//option to constantly do damage
	public bool doDmg = true;				//allows for preventing the enemy from doing damage
	#endregion

	#region OnCollisionEnter
	void OnCollisionEnter(Collision col)
	{
		//when the player collides with the enemy
		if (col.collider.gameObject.CompareTag("Player"))
		{
			Debug.Log("it's a direct hit!");
			//and the enemy is capable of doing damage
			if (doDmg)
			{
				//damage the player
				if (!GetComponent<Enemy>().Hugged)
				{
					col.gameObject.GetComponent<PlayerHealth>().TakeDmg(dmgAmount);
				}
				else
				{
					col.gameObject.GetComponent<PlayerHealth>().TakeDmg(huggedDmgAmt);
				}
			}
		}
	}
	#endregion

	#region OnCollisionStay
	void OnCollisionStay(Collision col)
	{
		//when the player is colliding with the enemy and the enemy is able to do constant damage
		if (col.collider.gameObject.CompareTag("Player") && constDmg)
		{
			Debug.Log("Oh no it's poisoned!");
			//damage the player
			if (doDmg)
			{
				col.gameObject.GetComponent<PlayerHealth>().TakeDmg(dmgAmount);
			}
		}
	}
	#endregion
}
