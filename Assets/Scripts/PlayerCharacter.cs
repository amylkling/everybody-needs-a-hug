using UnityEngine;

//adapted from Unity's Stardard Asset "ThirdPersonCharacter"'s "ThirdPersonCharacter" script
//all additions noted in comments


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class PlayerCharacter : MonoBehaviour
{
	#region Variables
	//Original variables
	[SerializeField] float m_MovingTurnSpeed = 360;
	[SerializeField] float m_StationaryTurnSpeed = 180;
	[SerializeField] float m_MoveSpeedMultiplier = 1f;
	[SerializeField] float m_AnimSpeedMultiplier = 1f;
	[SerializeField] float m_GroundCheckDistance = 0.1f;

	Rigidbody m_Rigidbody;
	Animator m_Animator;
	bool m_IsGrounded;
	float m_TurnAmount;
	float m_ForwardAmount;
	Vector3 m_GroundNormal;


	//new variables
	float m_CapsuleHeight;									//the height of the attached capsule collider
	Vector3 m_CapsuleCenter;								//the center of the attached capsule collider
	CapsuleCollider m_Capsule;								//reference to the attached capsule collider
	bool m_Hugging;											//controls when the collider gets modified while hugging
	float m_CapsuleRadius;									//holds the collider's default radius
	[SerializeField] float m_HugRadiusModifier = 1.5f;		//the amount to divide the radius of the collider by when hugging
	[SerializeField] float m_HugCenterModifier = .5f;		//the amount to subtract the collider's center's z axis by when hugging
	[SerializeField] float m_HugHeightModifier = .1f;		//the amount to subtract the collider's height by when hugging
	[SerializeField] float m_HugDistLimit = .5f;			//how far the player can be from an enemy to latch onto them with a hug
	[SerializeField] float disengageTime = .5f;				//how long it takes for a hug to cancel once the enemy is incapacitated
	public bool hugEngaged;									//determines if the player has latched onto an enemy or not
	bool m_GroupHug;										//activates the group hug animation
	bool m_Dodging;											//controls when the collider gets modified when dodging
	[SerializeField] float m_DodgeRadiusModifier = 1.5f;	//the amount to divide the radius of the collider by when dodging
	[SerializeField] float m_DodgeCenterModifier = .5f;		//the amount to subtract the collider's center's z axis by when dodging
	public GameObject kissParticlePrefab;					//the prefab for the kiss particle effect
	GameObject kissParticle;								//the newly generated kiss particle
	public GameObject gHugParticlePrefab;					//the prefab for the group hug particle effect
	GameObject gHugParticle;								//the newly generated group hug particle system
	public AudioSource soundfx;								//the audiosource to feed the sound efefcts to
	public AudioClip kiss;									//the sound effect for the kiss
	public AudioClip gHugAudio;								//the sound efefct for the group hug
	#endregion

	#region Start
	void Start()
	{
		//initialize
		m_Animator = GetComponent<Animator>();
		m_Rigidbody = GetComponent<Rigidbody>();
		m_Capsule = GetComponent<CapsuleCollider>();
		m_CapsuleHeight = m_Capsule.height;
		m_CapsuleCenter = m_Capsule.center;
		m_CapsuleRadius = m_Capsule.radius;

		m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

		soundfx = GetComponent<AudioSource>();
	}
	#endregion

	#region Move
	public void Move(Vector3 move, bool hug, bool groupHug, GameObject hugTarget, bool dodge, bool blowKiss) //changed crouch to hug and jump to groupHug
	{

		// convert the world relative moveInput vector into a local-relative
		// turn amount and forward amount required to head in the desired
		// direction.
		if (move.magnitude > 1f) move.Normalize();
		move = transform.InverseTransformDirection(move);
		CheckGroundStatus();
		move = Vector3.ProjectOnPlane(move, m_GroundNormal);
		m_TurnAmount = Mathf.Atan2(move.x, move.z);
		m_ForwardAmount = move.z;

		//new functions
		HuggingStance(hug, hugTarget);
		Dodging(dodge);
		Finisher(groupHug);
		BlowAKiss(blowKiss, hugTarget);

		ApplyExtraTurnRotation(hug);

		// send input and other state parameters to the animator
		UpdateAnimator(move);
	}
	#endregion

	#region HuggingStance
	//make hugging happen
	void HuggingStance(bool hug, GameObject hugTarget)
	{
		//change capsule collider's size and position upon hugging
		if (hug)
		{
			//make sure the collider is only changed once, then initiate the hug
			if (m_Hugging)
			{
				//if the player is close enough to an enemy that isn't incapacitated
				if (Vector3.Distance(transform.position, hugTarget.transform.position) < m_HugDistLimit && 
					!hugTarget.GetComponent<HugHelper>().enemyScript.Dead)
				{
					//attach the player to the enemy and set off the huggles script
					transform.SetParent(hugTarget.transform);
					transform.LookAt(hugTarget.transform);
					hugEngaged = true;
				}
				else if (hugTarget.GetComponent<HugHelper>().enemyScript.Dead)
				{
					//if the target is incapacitated, disengage the hug
					hugEngaged = false;
					if (!gameObject.GetComponent<Huggles>().HugOff && !gameObject.GetComponent<Huggles>().HugOn)
					{
						gameObject.GetComponent<Huggles>().SetTimer(disengageTime);
						gameObject.GetComponent<Huggles>().HugOff = true;
					}
				}
				else
				{
					//otherwise, tell the huggles script to activate the miss timer
					hugEngaged = false;
					gameObject.GetComponent<Huggles>().HugOff = true;
				}
				return;
			}

			Vector3 center = m_Capsule.center;
			center.z = center.z - m_HugCenterModifier;

			m_Capsule.center = center;
			m_Capsule.radius = m_Capsule.radius / m_HugRadiusModifier;
			m_Capsule.height = m_Capsule.height - m_HugHeightModifier;

			m_Hugging = true;
		}
		else
		{
			//reset everything if the hug is cancelled by the player
			m_Capsule.center = m_CapsuleCenter;
			m_Capsule.radius = m_CapsuleRadius;
			m_Capsule.height = m_CapsuleHeight;
			transform.parent = null;
			m_Hugging = false;
			hugEngaged = false;
		}
	}
	#endregion

	#region Dodging
	//make dodging happen
	void Dodging(bool dodge)
	{
		//allow the animation to play
		if (dodge)
		{
			m_Dodging = true;
		}
		else
		{

			m_Dodging = false;
		}

		AnimatorStateInfo state = m_Animator.GetCurrentAnimatorStateInfo(0);

		//change capsule collider's size and position upon dodging
		if (state.IsName("Dodging"))
		{
			Vector3 center = m_Capsule.center;
			center.z = center.z - m_DodgeCenterModifier;

			m_Capsule.center = center;
			m_Capsule.radius = m_Capsule.radius / m_DodgeRadiusModifier;
		}
		else
		{
			//reset everything
			m_Capsule.center = m_CapsuleCenter;
			m_Capsule.radius = m_CapsuleRadius;
			m_Capsule.height = m_CapsuleHeight;
		}
	}
	#endregion

	#region Finisher
	//executes the finishing move, Group hug
	void Finisher(bool groupHug)
	{
		if (groupHug)
		{
			Debug.Log("c'mere everyone, GROUP HUG!! <3");

			//ensure a grouphug only happens once after the button is pressed
			if (!m_GroupHug)
			{
				//make the player invincible, just in case
				GetComponent<PlayerHealth>().Invincible = true;

				//tell all of the enemies that a group hug is happening so that they will move toward the player and disappear
				GameObject[] enemies = GameObject.FindGameObjectsWithTag("Incapacitated");

				foreach (GameObject e in enemies)
				{
					e.GetComponent<Enemy>().Finished = true;
				}

				//spawn the group hug particle effect
				gHugParticle = Instantiate(gHugParticlePrefab, new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z), Quaternion.identity);
				gHugParticle.GetComponent<ParticleSystem>().Play();

				//play the sound effect
				soundfx.PlayOneShot(gHugAudio);

				//the group hug is now complete
				m_GroupHug = true;
				GetComponent<Huggles>().GroupHugComplete = true;
			}
		}
		else
		{
			m_GroupHug = false;
		}
	}
	#endregion

	#region BlowAKiss
	void BlowAKiss(bool blowKiss, GameObject hugTarget)
	{
		//briefly turn off player movement
		//face the enemy
		//spawn a particle effect that travels to the enemy
		//when it hits the enemy, immobilize them with a time limit
		//restore player movement
		if(blowKiss)
		{
			Debug.Log("Mwah!");
			transform.LookAt (new Vector3 (hugTarget.transform.position.x, transform.position.y, hugTarget.transform.position.z));
			GetComponent<PlayerControl>().PauseMe(true);
			soundfx.PlayOneShot(kiss);
			kissParticle = Instantiate(kissParticlePrefab,new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), transform.rotation);
			kissParticle.GetComponent<ParticleSystem>().Play();
			GetComponent<PlayerControl>().PauseMe(false);
		}
	}
	#endregion

	#region UpdateAnimator
	void UpdateAnimator(Vector3 move)
	{
		// update the animator parameters
		m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
		m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);

		//control the hugging animations
		m_Animator.SetBool("Hug", m_Hugging);
		m_Animator.SetBool("GroupHug", m_GroupHug);
		m_Animator.SetBool("Dodge", m_Dodging);

		// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
		// which affects the movement speed because of the root motion.
		if (m_IsGrounded && move.magnitude > 0)
		{
			m_Animator.speed = m_AnimSpeedMultiplier;
		}
		else
		{
			// don't use that while airborne
			m_Animator.speed = 1;
		}
	}
	#endregion

	#region ApplyExtraTurnRotation
	void ApplyExtraTurnRotation(bool hug)
	{
		// help the character turn faster (this is in addition to root rotation in the animation)
		float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);

		//only utilize this if the character isn't in a hugging state
		if (!hug)
		{
			transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
		}
	}
	#endregion

	#region OnAnimatorMove
	public void OnAnimatorMove()
	{
		// we implement this function to override the default root motion.
		// this allows us to modify the positional speed before it's applied.
		if (m_IsGrounded && Time.deltaTime > 0)
		{
			Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

			// we preserve the existing y part of the current velocity.
			v.y = m_Rigidbody.velocity.y;
			m_Rigidbody.velocity = v;
		}
	}
	#endregion

	#region CheckGroundStatus
	void CheckGroundStatus()
	{
		RaycastHit hitInfo;
		#if UNITY_EDITOR
		// helper to visualise the ground check ray in the scene view
		Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
		#endif
		// 0.1f is a small offset to start the ray from inside the character
		// it is also good to note that the transform position in the sample assets is at the base of the character
		if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
		{
			m_GroundNormal = hitInfo.normal;
			m_IsGrounded = true;
			m_Animator.applyRootMotion = true;
		}
		else
		{
			m_IsGrounded = false;
			m_GroundNormal = Vector3.up;
			m_Animator.applyRootMotion = false;
		}
	}
	#endregion

	#region Gets
	//for other scripts to check hugEngaged
	public bool HugEngaged
	{
		get {return hugEngaged;}
	}

	//for other scripts to get gHugParticle
	public GameObject GHugParticle
	{
		get {return gHugParticle;}
	}
	#endregion
}

