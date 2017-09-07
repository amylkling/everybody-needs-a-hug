using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

//adapted from Unity's Stardard Asset "ThirdPersonCharacter"'s "ThirdPersonUserControl" script
//all changes noted in comments


[RequireComponent (typeof(PlayerCharacter))]
public class PlayerControl : MonoBehaviour
{
	private PlayerCharacter m_Character;	// A reference to the ThirdPersonCharacter on the object
	private Transform m_Cam;				// A reference to the main camera in the scenes transform
	private Vector3 m_CamForward;			// The current forward direction of the camera
	private Vector3 m_Move;					// the world-relative desired move direction, calculated from the camForward and user input.
	private bool m_Jump;					

	//new variables
	public bool hug;						//for activating hugging
	private GameObject hugTarget;			//reference to the object that latches onto enemies so the player can target and hug them
	bool hugControl = true;					//so scripts can control when hugging happens
	public bool groupHug;					//for activating the finishing move
	bool finishThem = false;				//so the groupHug only activates when all enemies are down
	bool dodge = false;						//for activating dodging
	bool blowKiss = false;
	bool kissie = false;
	bool pauseMe = false;
	private float h;
	private float v;
	private Vector3 startPosition;



	private void Start ()
	{
		// get the transform of the main camera
		if (Camera.main != null)
		{
			m_Cam = Camera.main.transform;
		}
		else
		{
			Debug.LogWarning (
				"Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
			// we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
		}

		// get the third person character ( this should never be null due to require component )
		m_Character = GetComponent<PlayerCharacter> ();

		hugTarget = GameObject.FindGameObjectWithTag ("HugTarget");

		startPosition = gameObject.transform.position;
	}


	private void Update ()
	{
		/*if (!m_Jump)
		{
			m_Jump = CrossPlatformInputManager.GetButtonDown ("Jump");
		}*/

		if(!pauseMe)
		{
			//grouphug input
			if (CrossPlatformInputManager.GetButtonDown("Jump") && finishThem)
			{
				groupHug = true;
			}
			else if (CrossPlatformInputManager.GetButtonDown("Jump") && kissie)
			{
				blowKiss = true;
			}
			else
			{
				groupHug = false;
				blowKiss = false;
			}

			//reset player position in case of oob
			if(Input.GetButtonDown("Reset"))
			{
				gameObject.transform.position = startPosition;
				GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
		}

	}


	// Fixed update is called in sync with physics
	private void FixedUpdate ()
	{

		// read inputs when the player isn't paused
		if(!pauseMe)
		{
			h = CrossPlatformInputManager.GetAxis ("Horizontal");
			v = CrossPlatformInputManager.GetAxis ("Vertical");

			//hug input
			if (CrossPlatformInputManager.GetAxisRaw ("Fire3") != 0 && hugControl)
			{
				hug = true;
			}
			else
			{
				hug = false;
			}

			//dodge input
			if(CrossPlatformInputManager.GetButtonDown("Dodge"))
			{
				dodge = true;
			}
			else
			{
				dodge = false;
			}
		}
			





		// calculate move direction to pass to character
		/*if (m_Cam != null)
		{
			// calculate camera relative direction to move:
			m_CamForward = Vector3.Scale (m_Cam.forward, new Vector3 (1, 0, 1)).normalized;
			m_Move = v * m_CamForward + h * m_Cam.right;
		}
		else
		{*/
			// we use world-relative directions in the case of no main camera
			m_Move = v * Vector3.forward + h * Vector3.right;
		//}
		#if !MOBILE_INPUT
		// walk speed multiplier
		if (Input.GetKey (KeyCode.LeftControl))
			m_Move *= 0.5f;
		#endif

		//ensure that the player always remains upright so they can move
		if (transform.rotation.eulerAngles.x != 0)
		{
			Vector3 rot = transform.rotation.eulerAngles;
			transform.rotation = Quaternion.Euler(0, rot.y, rot.z);
		}

		if (transform.rotation.eulerAngles.z != 0)
		{
			Vector3 rot = transform.rotation.eulerAngles;
			transform.rotation = Quaternion.Euler(rot.x, rot.y, 0);
		}

		// pass all parameters to the character control script
		m_Character.Move (m_Move, hug, groupHug, hugTarget, dodge, blowKiss); //changed crouch to hug and jump to groupHug and added hugTarget and dodge and blowKiss
		//m_Jump = false;
	}

	//for other scripts to set hugControl
	public void HugControl(bool b)
	{
		hugControl = b;
	}

	//for other scripts to access finishThem
	public bool FinishThem
	{
		get {return finishThem;}
		set {finishThem = value;}
	}

	//for other scripts to set kissie
	public void Kissie(bool b)
	{
		kissie = b;
	}

	//for other scripts to set pauseMe
	public void PauseMe(bool b)
	{
		pauseMe = b;
	}
}

