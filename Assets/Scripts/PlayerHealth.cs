using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//controls the player's health and related states

public class PlayerHealth : MonoBehaviour {

	#region Variables
	public float currentHealth = 0f;				//the current amount of health the player has
	public float maxHealth = 100;					//the maximum amount of health the player can have
	public Slider healthBar;						//reference to the player's health bar UI
	private GameControl control;					//reference to the GameControl script
	private bool invincible = false;				//whether or not the player is able to take damage
	private bool regen = false;						//whether or not the player is able to regenerate health
	private bool isRegen = false;					//whether or not the regen coroutine is running
	public bool changeRegen = true;					//whether or not other scripts are allowed to change the regen state
	[SerializeField]private int regenAmt = 1;		//the amount of health to regenerate
	public AudioSource soundfx;						//the audiosource to feed sound effects to
	public AudioClip hurt;							//the sound efefct for the player getting hurt
	#endregion

	#region Start
	// Use this for initialization
	void Start () 
	{
		currentHealth = maxHealth;
		if (GameObject.Find("PlayerHealth") != null)
		{
			healthBar = GameObject.Find("PlayerHealth").GetComponent<Slider>();
			healthBar.value = currentHealth;
		}
		control = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GameControl>();
		invincible = false;
		regen = false;
		isRegen = false;
		changeRegen = true;
		soundfx = GetComponent<AudioSource>();
	}
	#endregion

	#region Update
	void Update()
	{
		//as long as the player's health is not at max and regen is allowed and is not already occurring,
		//regen health
		if (currentHealth != maxHealth && regen && !isRegen)
		{
			StartCoroutine(RegainHealthOverTime());
		}
	}
	#endregion

	#region TakeDmg
	//cause the player's health to decrease by a set amount
	public void TakeDmg(float dmg)
	{
		//disallow regen while the player is getting hurt
		regen = false;
		changeRegen = false;

		//only get hurt if the player is not invincible
		if (!invincible)
		{
			//play the getting hurt sound and decrease health
			soundfx.PlayOneShot(hurt);
			currentHealth -= dmg;

			//change the UI, if it exists
			if (healthBar != null)
			{
				healthBar.value = currentHealth;
			}

			//when the player's health reaches zero or less, go into the death state
			if (currentHealth <= 0)
			{
				Death();
			}
		}

		changeRegen = true;
	}
	#endregion

	#region RegenHealth
	//increases player health by a set amount
	void RegenHealth(int amt)
	{
		currentHealth += amt;
		healthBar.value = currentHealth;
	}
	#endregion

	#region Heal
	//restores player health by a set amount
	public void Heal(int amt)
	{
		if (currentHealth < maxHealth)
		{
			currentHealth += amt;
			if (currentHealth > maxHealth)
			{
				currentHealth = maxHealth;
			}
			healthBar.value = currentHealth;
		}
	}
	#endregion

	#region Death
	//causes a game over
	void Death()
	{
		control.StartCoroutine(control.GameOver(false));
	}
	#endregion

	#region RegainHealthOverTime Coroutine
	//regen health a set amount every second
	private IEnumerator RegainHealthOverTime()
	{
		isRegen = true;
		while (currentHealth < maxHealth && regen)
		{
			RegenHealth(regenAmt);
			yield return new WaitForSeconds(1);
		}
		isRegen = false;
	}
	#endregion

	#region Get Sets
	//So other scripts can determine if the player is invincible
	public bool Invincible
	{
		get {return invincible;}
		set {invincible = value;}
	}

	//so other scripts can determine if the player can regen health
	public bool RegenHealthVar
	{
		get {return regen;}
		set {regen = value;}
	}
	#endregion
}
