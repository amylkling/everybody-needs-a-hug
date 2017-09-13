using UnityEngine;

//adapted from Unity's Stardard Asset "ThirdPersonCharacter"'s "ThirdPersonCharacter" script
//all additions noted in comments


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class PlayerCharacter : MonoBehaviour
{
	[SerializeField] float m_MovingTurnSpeed = 360;
	[SerializeField] float m_StationaryTurnSpeed = 180;
	[SerializeField] float m_JumpPower = 12f;
	[Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;
	[SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
	[SerializeField] float m_MoveSpeedMultiplier = 1f;
	[SerializeField] float m_AnimSpeedMultiplier = 1f;
	[SerializeField] float m_GroundCheckDistance = 0.1f;

	Rigidbody m_Rigidbody;
	Animator m_Animator;
	bool m_IsGrounded;
	float m_OrigGroundCheckDistance;
	const float k_Half = 0.5f;
	float m_TurnAmount;
	float m_ForwardAmount;
	Vector3 m_GroundNormal;
	float m_CapsuleHeight;
	Vector3 m_CapsuleCenter;
	CapsuleCollider m_Capsule;

	//new variables
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
	public GameObject kissParticlePrefab;
	GameObject kissParticle;
	public GameObject gHugParticlePrefab;
	GameObject gHugParticle;
	public AudioSource soundfx;
	public AudioClip kiss;
	public AudioClip gHugAudio;


	void Start()
	{
		m_Animator = GetComponent<Animator>();
		m_Rigidbody = GetComponent<Rigidbody>();
		m_Capsule = GetComponent<CapsuleCollider>();
		m_CapsuleHeight = m_Capsule.height;
		m_CapsuleCenter = m_Capsule.center;
		m_CapsuleRadius = m_Capsule.radius;

		m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
		m_OrigGroundCheckDistance = m_GroundCheckDistance;

		soundfx = GetComponent<AudioSource>();
	}


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

		//not needed
		#region jumping related
		// control and velocity handling is different when grounded and airborne:
		/*if (m_IsGrounded)
		{
			HandleGroundedMovement(crouch, jump);
		}
		else
		{
			HandleAirborneMovement();
		}*/
		#endregion

		//not needed
		#region crouching related
		//ScaleCapsuleForCrouching(crouch);
		//PreventStandingInLowHeadroom();
		#endregion

		// send input and other state parameters to the animator
		UpdateAnimator(move);
	}

	//not needed
	#region crouching related functions
	/*void ScaleCapsuleForCrouching(bool crouch)
	{
		if (m_IsGrounded && crouch)
		{
			if (m_Crouching) return;
			m_Capsule.height = m_Capsule.height / 2f;
			m_Capsule.center = m_Capsule.center / 2f;
			m_Crouching = true;
		}
		else
		{
			Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
			float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
			if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
			{
				m_Crouching = true;
				return;
			}
			m_Capsule.height = m_CapsuleHeight;
			m_Capsule.center = m_CapsuleCenter;
			m_Crouching = false;
		}
	}*/

	/*void PreventStandingInLowHeadroom()
	{
		// prevent standing up in crouch-only zones
		if (!m_Crouching)
		{
			Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
			float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
			if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
			{
				m_Crouching = true;
			}
		}
	}*/
	#endregion

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

	//executes the finishing move, Group hug
	void Finisher(bool groupHug)
	{
		if (groupHug)
		{
			Debug.Log("c'mere everyone, GROUP HUG!! <3");

			if (!m_GroupHug)
			{
				GetComponent<PlayerHealth>().Invincible = true;

				GameObject[] enemies = GameObject.FindGameObjectsWithTag("Incapacitated");

				foreach (GameObject e in enemies)
				{
					e.GetComponent<Enemy>().Finished = true;
				}

				gHugParticle = Instantiate(gHugParticlePrefab, new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z), Quaternion.identity);
				gHugParticle.GetComponent<ParticleSystem>().Play();

				soundfx.PlayOneShot(gHugAudio);

				m_GroupHug = true;
				GetComponent<Huggles>().GroupHugComplete = true;
			}


		}
		else
		{
			m_GroupHug = false;
		}
	}

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


	void UpdateAnimator(Vector3 move)
	{
		// update the animator parameters
		m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
		m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);

		//control the hugging animations
		m_Animator.SetBool("Hug", m_Hugging);
		m_Animator.SetBool("GroupHug", m_GroupHug);
		m_Animator.SetBool("Dodge", m_Dodging);

		//don't need this
		#region jumping related code
		//m_Animator.SetBool("OnGround", m_IsGrounded);
		/*if (!m_IsGrounded)
		{
			m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
		}*/
		#endregion

		// calculate which leg is behind, so as to leave that leg trailing in the jump animation
		// (This code is reliant on the specific run cycle offset in our animations,
		// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
		float runCycle =
			Mathf.Repeat(
				m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
		float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
		if (m_IsGrounded)
		{
			m_Animator.SetFloat("JumpLeg", jumpLeg);
		}

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

	//don't need
	#region more jumping and crouching functions
	/*void HandleAirborneMovement()
	{
		// apply extra gravity from multiplier:
		Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
		m_Rigidbody.AddForce(extraGravityForce);

		m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
	}
		
	void HandleGroundedMovement(bool crouch, bool jump)
	{
		// check whether conditions are right to allow a jump:
		if (jump && !crouch && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
		{
			// jump!
			m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
			m_IsGrounded = false;
			m_Animator.applyRootMotion = false;
			m_GroundCheckDistance = 0.1f;
		}
	}*/
	#endregion

	void ApplyExtraTurnRotation(bool hug)
	{
		// help the character turn faster (this is in addition to root rotation in the animation)
		float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);

		if (!hug)
		{
			transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
		}

	}


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
}

