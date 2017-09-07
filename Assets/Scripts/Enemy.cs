using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Enemy : MonoBehaviour {

	#region Variables
	public int minHealth = 0;
	public int maxHealth = 50;
	public float health;
	[SerializeField]private bool dead;
	public Slider healthBar;
	public EnemyUI uiScript;
	public EnemyUIDirectControl uiControl;
	[SerializeField]private bool finished = false;
	private bool hugged = false;

	public float pullInSpeed = 4f;
	public float pullOffsetX = 1f;
	public float pullOffsetZ = 1f;
	public float minShakeRotation = 5f;
	public float maxShakeRotation = 105f;
	public float shakeSpeed = 2f;
	public Slider gHugMeter;
	//public int gHugIncrease = 25;
	private bool deadNow = false;
	public GameControl gm;
	public float score = 100f;
	EnemyWeapon weapon;
	public float kissScore = 50;
	public float kissDmg = 5f;
	#endregion
	
	#region Start
	//initiate once object is ready
	void Start()
	{
		health = maxHealth;
		uiScript = gameObject.GetComponent<EnemyUI>();
		//healthBar = uiScript.healthSlider;
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
		//do things when hugged by player
		if (hugged)
		{
			Quaternion newRot = Quaternion.identity;
			Vector3 randEuler = transform.eulerAngles;
			randEuler.y = Random.Range(minShakeRotation, maxShakeRotation);
			newRot.eulerAngles = randEuler;
			transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.deltaTime * shakeSpeed);
		}

		//do things when the enemy "dies"
		if (deadNow)
		{
			Debug.Log(gameObject.name + ": I don't want to fight you anymore");
			//gHugMeter.value += gHugIncrease;
			gHugMeter.value += gHugMeter.maxValue/transform.parent.childCount;
			gm.Scoreboard(score);
			if (weapon != null)
			{
				weapon.doDmg = false;
				weapon.enabled = false;
			}
			dead = true;
			deadNow = false;
			//Destroy (gameObject);
		}

		//do things when the enemy is finished off
		if (finished)
		{
			Debug.Log(gameObject.name + ": love is over!");

			//move the enemy towards the player, dramatically
			//do this with navmesh?
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			transform.LookAt(player.transform.position);
			float step = pullInSpeed * Time.deltaTime;
			Vector3 newPos = new Vector3(player.transform.position.x, transform.position.y, 
				player.transform.position.z + pullOffsetZ);
			//transform.position = Vector3.Lerp(transform.position, newPos, step);
			if (Vector3.Distance(transform.position, player.transform.position) > pullOffsetZ + 0.5f)
			{
				transform.position = Vector3.MoveTowards(transform.position, newPos, step);
				//Debug.Log(Vector3.Distance(transform.position, player.transform.position));
			}
			else
			{
				Destroy(gameObject);
			}

			/*
			RaycastHit hit;

			if (Physics.Raycast(transform.position, transform.forward, out hit))
			{
				if (hit.collider.tag == "Player")
				{
					
				}
			}*/

			//Destroy(gameObject);
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
	//set the enemy's state to "dead"
	void Death()
	{
		deadNow = true;
	}
	#endregion

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

	//detect when a blown kiss hits the enemy
	void OnParticleCollision(GameObject e)
	{
		if (GetComponent<EnemyAI>() == null)
		{
			gm.Scoreboard(kissScore);
			TakeDmg(kissDmg);
			Destroy(e);
		}
	}
}
