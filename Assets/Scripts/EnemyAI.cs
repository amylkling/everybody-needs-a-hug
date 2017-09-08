using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//script controlling the enemy's more advanced reactions

public class EnemyAI : MonoBehaviour 
{
	#region Variables
	//objects and scripts
	GameObject player;						//reference player object
	NavMeshAgent agent;						//the NavMeshAgent attached to this enemy object
	public GameControl gm;					//reference to the GameControl script

	//movement and attack parameters
	bool moveIn = true;						//determines when the enemy can move towards the player
	public float backOffDist = 3f;			//how far back the enemy will go after hitting the player
	public float maxDist = 5f;				//the furthest the player can be before the enemy starts moving again
	public float backOffTime = 3f;			//how much time the enemy waits before attempting another attack
	private float backOffTimer = 0f;		//timer that counts down how long the enemy waits between attacks
	bool backOff = false;					//determines when the timer starts
	public float attackDistLow = 3f;		//lower bound of the enemy's attack range
	public float attackDistHigh = 4f;		//upper bound of the enemy's attack range
	private float chargeTimer = 0f;			//timer to make the enemy wait before attacking
	public float chargeTime = 5f;			//how much time the enemy has to wait before it can attack
	private bool chargeUp = false;			//determines if the enemy is charging up an attack
	public float firstThreshold = 3f;		//the first point in the timer at which the enemy starts to flash a color
	public float secondThreshold = 1f;		//the next point in the timer at which the enemy flashes faster
	public Color flashCol;					//the color of the enemy's flash
	public Light halo;						//the light that makes it look like the enemy is flashing

	//attacking
	private bool theLoop = false;			//controls when the enemy can attempt to attack
	private bool navEngage = false;			//controls the state of the enemy's NavMesh
	private float coolTimer = 0f;			//timer that prevents the enemy from leaping
	public float coolDownTime = 5f;			//the amount of time to wait between leaps
	private bool pathPlacement = true;		//ensures the enemy only decides its path once per leap
	private bool attEngage = true;			//controls the enemy's ability to attack
	private bool iLeap = false;				//determines when the enemy is done leaping

	//player's kiss
	bool smooched = false;					//the smooched state of the enemy
	private float smoochTimer = 0f;			//timer for the length of the smooched state
	public float smoochEffectTime = 3f;		//the amount of time to be in the smooched state
	public float kissScore = 50;			//the score to award the player with after the enemy is hit by a kiss
	public float kissDmg = 5f;				//the amount of damage the enemy takes from a kiss

	//sound
	public AudioSource soundfx;				//the source to feed the soundfx to
	//soundfx to randomly choose from to play when attacking
	public AudioClip attack1;				
	public AudioClip attack2;				
	public AudioClip attack3;				
	public AudioClip attack4;				
	public AudioClip attack5;				
	public AudioClip attack6;				
	public AudioClip attack7;				
	public AudioClip attack8;				
	public AudioClip attack9;				
	public AudioClip attack10;				
	public AudioClip attack11;				
	private AudioClip[] attackSounds;		//the array of all of these soundfx

	//pathing
	public string attackPathName;			//name of the path of this enemy
	public iTweenPath path;					//reference to the path of this enemy
	#endregion

	#region Start
	// Use this for initialization
	void Start () 
	{
		player = GameObject.FindGameObjectWithTag("Player");
		agent = GetComponent<NavMeshAgent>();
		backOffTimer = backOffTime;
		coolTimer = coolDownTime;
		smoochTimer = smoochEffectTime;
		chargeTimer = chargeTime;
		gm = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GameControl>();
		halo = GetComponent<Light>();
		halo.enabled = false;

		path = GetComponent<iTweenPath>();
		//add a random number to the path so it can be unique from other enemies
		attackPathName = "jumper " + Random.Range(1, 100);
		path.pathName = attackPathName;
		//ensure the path is actually activated
		if(!iTweenPath.paths.ContainsKey(path.pathName))
		{
			iTweenPath.paths.Add(path.pathName.ToLower(), path);
		}

		soundfx = GetComponent<AudioSource>();
		attackSounds = new AudioClip[11];
		attackSounds[0] = attack1;
		attackSounds[1] = attack2;
		attackSounds[2] = attack3;
		attackSounds[3] = attack4;
		attackSounds[4] = attack5;
		attackSounds[5] = attack6;
		attackSounds[6] = attack7;
		attackSounds[7] = attack8;
		attackSounds[8] = attack9;
		attackSounds[9] = attack10;
		attackSounds[10] = attack11;
	}
	#endregion

	#region Update
	// Update is called once per frame
	void Update () 
	{
		//only act as long as the enemy isn't dead or smooched
		if (!GetComponent<Enemy>().Dead)
		{
			if (!smooched)
			{
				//ensure the enemy is always facing the player when not in a hug
				if (!GetComponent<Enemy>().Hugged)
				{
					transform.LookAt (new Vector3 (player.transform.position.x, transform.position.y, player.transform.position.z));
				}

				//move to/follow the player
				if (moveIn)
				{
					agent.SetDestination(player.transform.position);
				}

				//calculate the distance between the enemy and the player
				float dist = Vector3.Distance(transform.position, player.transform.position);
				//Debug.Log(dist);

				//recalculate leap path, unless already leaping
				if (pathPlacement)
				{
					Debug.Log("YOU'RE MINE NOW!");
					Vector3 newPos = Vector3.Lerp(transform.position, player.transform.position - new Vector3(0.5f, 0, 0.5f), 0.5f);
					GetComponent<iTweenPath>().nodes [0] = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);
					GetComponent<iTweenPath>().nodes [1] = new Vector3(newPos.x, 3f, newPos.z);
					GetComponent<iTweenPath>().nodes [2] = new Vector3(player.transform.position.x - 0.5f, transform.position.y + 0.2f, player.transform.position.z - 0.5f);
				}


				//when it gets in range, start preparing to attack
				if (dist > attackDistLow && dist <= attackDistHigh && attEngage)
				{
					agent.enabled = false;
					moveIn = false;
					navEngage = false;
					/*call attack function to "charge" a leap attack
				 	*with flashing color that gets faster as a timer goes down
			 		*Then call the actual attack function
					 *that makes it leap at the player and then stay still for a short time*/
					ChargeUp();
				}

				//when the leap is done, call activateNav
				if (V3Equal(transform.position, GetComponent<iTweenPath>().nodes [2]) && iLeap)
				{
					Debug.Log("called it");
					activateNav();
				}

				//reactivate navigation
				if (navEngage)
				{
					agent.enabled = true;
					moveIn = true;
				}

				//wait a certain amount of time before moving in to attack again
				if (backOff)
				{
					backOffTimer -= Time.deltaTime;
					if (backOffTimer <= 0)
					{
						backOffTimer = backOffTime;
						agent.isStopped = false;
						moveIn = true;
						backOff = false;
					}
				}

				//leap cooldown
				if (theLoop)
				{
					coolTimer -= Time.deltaTime;
					//Debug.Log("Timer: " + coolTimer);
					if (coolTimer <= 0)
					{
						attEngage = true;
						coolTimer = coolDownTime;
						theLoop = false;
					}
				}


				if (chargeUp)
				{
					chargeTimer -= Time.deltaTime;
					if (chargeTimer <= chargeTime && chargeTimer > firstThreshold)
					{
						//flash at a slow speed
						//call a coroutine to flash the halo at speed
						StartCoroutine(FlashColor(1f));
						Debug.Log("HRAA-");
					} 
					else if (chargeTimer <= firstThreshold && chargeTimer > secondThreshold)
					{
						//flash at a faster speed
						//call a coroutine to flash the halo at speed
						StartCoroutine(FlashColor(0.8f));
						Debug.Log("-AAAAA-");
					} 
					else if (chargeTimer <= secondThreshold && chargeTimer > 0)
					{
						//flash at fastest speed
						//call a coroutine to flash the halo at speed
						StartCoroutine(FlashColor(0.5f));
						Debug.Log("-AAAAAAAAAAAA");
					} 
					 else if (chargeTimer <= 0)
					{
						chargeUp = false;
						chargeTimer = chargeTime;
						Attack();
					}
				}
			}
			else
			{
				//if the enemy is smooched, stun it for a short time
				if (agent.isActiveAndEnabled)
				{
					agent.isStopped = true;
				}

				smoochTimer -= Time.deltaTime;
				if (smoochTimer <= 0)
				{
					smooched = false;
					smoochTimer = smoochEffectTime;
					if (agent.isActiveAndEnabled)
					{
						agent.isStopped = false;
					}
				}
			}
		}
		else
		{
			//if the enemy is dead and finished, deactivate the navmesh and rigidbody 
			//so it doesn't interfere with the group hug 
			if (gameObject.GetComponent<Enemy>().Finished)
			{
				agent.enabled = false;
				GetComponent<Rigidbody>().Sleep();
			}
		}
	}
	#endregion

	#region ChargeUp
	//set it to prepare to attack
	void ChargeUp()
	{
		chargeUp = true;
	}
	#endregion

	#region Attack
	//leap at the player
	void Attack()
	{
		//if it hasn't leaped yet or isn't cooling down from a leap
		if (!theLoop)
		{
			Debug.Log("HYUP");
			attEngage = false;
			pathPlacement = false;

			//choose a random sound to play as it leaps
			int ran = Random.Range(0,attackSounds.Length);
			soundfx.PlayOneShot(attackSounds[ran]);

			//actually leap
			iTween.MoveTo(gameObject, iTween.Hash("path", iTweenPath.GetPath(attackPathName), "time", 2f, "easetype", iTween.EaseType.linear, "looptype", iTween.LoopType.none));
			iLeap = true;
		}
	}
	#endregion

	#region activateNav
	//activate leap cooldown and reactivates dynamic pathing
	void activateNav()
	{
		Debug.Log("stahp");
		soundfx.Stop();
		theLoop = true;
		pathPlacement = true;
		iLeap = false;
		navEngage = true;
	}
	#endregion

	#region FlashColor Coroutine
	//make the light flash
	private IEnumerator FlashColor(float speed)
	{
		Debug.Log("don't forget about me, still running over here");
		halo.enabled = true;
		yield return new WaitForSeconds(speed);
		halo.enabled = false;
	}
	#endregion

	#region V3Equal
	//compare two vector3s
	public bool V3Equal(Vector3 a, Vector3 b)
	{
		//Debug.Log("sqr mag: " + Vector3.SqrMagnitude(a - b));
		return Vector3.SqrMagnitude(a - b) < 0.1;
	}
	#endregion

	#region Smooch
	//for other scripts to set smooched
	public void Smooch(bool b)
	{
		smooched = b;
	}
	#endregion

	#region OnParticleCollision
	//detect when a blown kiss hits the enemy
	void OnParticleCollision(GameObject e)
	{
		Smooch(true);
		gm.Scoreboard(kissScore);
		GetComponent<Enemy>().TakeDmg(kissDmg);
		Destroy(e);
	}
	#endregion
}
