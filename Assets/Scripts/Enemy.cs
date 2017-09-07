using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//the base script for enemies

public class Enemy : MonoBehaviour {

	#region Variables
	public int minHealth = 0;						//the minimum amount for the health bar
	public int maxHealth = 50;						//the maximum amount for the health bar
	public float health;							//the current amount of health
	[SerializeField]private bool dead;				//flag for the "death" state of the enemy
	public Slider healthBar;						//reference to the health bar UI
	public EnemyUI uiScript;						//reference to the enemy's UI script
	public EnemyUIDirectControl uiControl;			//reference to the enemy's UI control script
	[SerializeField]private bool finished = false;	//controls whether or not the enemy is in a 'finished' state
	private bool hugged = false;					//the state of being hugged or not

	public float pullInSpeed = 4f;					//how fast the enemy gets pulled in during a group hug
	public float pullOffsetZ = 1f;					//offset to keep the enemy out of the player's space during a group hug
	public float minShakeRotation = 5f;				//minimum amount to rotate during hug
	public float maxShakeRotation = 105f;			//maximum amount to rotate during hug
	public float shakeSpeed = 2f;					//speed at which to rotate during hug
	public Slider gHugMeter;						//reference to the group hug meter UI
	private bool deadNow = false;					//internal flag to react to "death" when it happens
	public GameControl gm;							//reference to the GameControl script
	public float score = 100f;						//the amount of score to give the player after being defeated
	EnemyWeapon weapon;								//reference to the EnemyWeapon script
	public float kissScore = 50;					//the amount of score to give the player after being hit by a kiss
	public float kissDmg = 5f;						//the amount of damage to take from a kiss
	#endregion
	
	#region Start
	//initiate variables at start
	void Start()
	{
		health = maxHealth;
		uiScript = gameObject.GetComponent<EnemyUI>();
		healthBar.maxValue = maxHealth;
		uiControl = uiScript.uiScript;
		gHugMeter = GameObject.Find("GroupHugMeter").GetComponent<Slider>();
		gm = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GameControl>();
		if(GetComponent<EnemyWeapon>() != null)
		{
			weapon = GetComponent<EnemyWeapon>();
		}
	}
	#endregion

	#region Update
	// Update is called once per frame
	void Update()
	{
		//"shake" (ie rotate haphazardly) when hugged by player
		if (hugged)
		{
			Quaternion newRot = Quaternion.identity;
			Vector3 randEuler = transform.eulerAngles;
			randEuler.y = Random.Range(minShakeRotation, maxShakeRotation);
			newRot.eulerAngles = randEuler;
			transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.deltaTime * shakeSpeed);
		}

		//when the enemy "dies", update the group hug meter, scoreboard, and enemy state, and disable the enemy's weapon
		if (deadNow)
		{
			Debug.Log(gameObject.name + ": I don't want to fight you anymore");
			gHugMeter.value += gHugMeter.maxValue/transform.parent.childCount;
			gm.Scoreboard(score);
			if (weapon != null)
			{
				weapon.doDmg = false;
				weapon.enabled = false;
			}
			dead = true;
			deadNow = false;
		}

		//when the enemy is finished off by a group hug, move it dramatically towards the player and then destroy it
		if (finished)
		{
			Debug.Log(gameObject.name + ": love is over!");

			GameObject player = GameObject.FindGameObjectWithTag("Player");
			transform.LookAt(player.transform.position);
			float step = pullInSpeed * Time.deltaTime;
			Vector3 newPos = new Vector3(player.transform.position.x, transform.position.y, 
											player.transform.position.z + pullOffsetZ);

			//if the enemy hasn't reached the offset from the player's pos, keep moving
			if (Vector3.Distance(transform.position, player.transform.position) > pullOffsetZ + 0.5f)
			{
				transform.position = Vector3.MoveTowards(transform.position, newPos, step);
			}
			else
			{
				Destroy(gameObject);
			}
		}
	}
	#endregion
	
	#region TakeDamage
	//track the amount of damage taken by this enemy,
	//called by outside scripts like the player's
	public void TakeDmg(float amount)
	{
		health -= amount;
		healthBar.value = health;
		Debug.Log ("ARGHHHHH");
		if (health <= minHealth && !dead)
		{
			Death();
		}
	}
	#endregion
	
	#region Death
	//react to its death
	void Death()
	{
		deadNow = true;
	}
	#endregion

	#region Get Sets
	//allow other scripts to check if the enemy is dead
	public bool Dead
	{
		get{return dead;}
	}

	//allow other scripts to set the enemy's state to "finished"
	public bool Finished
	{
		get{return finished;}
		set{finished = value;}
	}

	//allow other scripts to set the enemy's state to "hugged"
	public bool Hugged
	{
		get{return hugged;}
		set{hugged = value;}
	}
	#endregion

	#region OnParticleCollision
	//detect when a blown kiss hits the enemy
	void OnParticleCollision(GameObject e)
	{
		//react to the kiss if the enemy isn't a training dummy
		if (GetComponent<EnemyAI>() == null)
		{
			gm.Scoreboard(kissScore);
			TakeDmg(kissDmg);
			Destroy(e);
		}
	}
	#endregion
}
