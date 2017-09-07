using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour {

	public float currentHealth = 0f;
	public float maxHealth = 100;
	public Slider healthBar;
	private GameControl control;
	private bool invincible = false;
	private bool regen = false;
	private bool isRegen = false;
	public bool changeRegen = true;
	[SerializeField]private int regenAmt = 1;
	public AudioSource soundfx;
	public AudioClip hurt;

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

	void Update()
	{
		if (currentHealth != maxHealth && regen && !isRegen)
		{
			StartCoroutine(RegainHealthOverTime());
		}
	}

	public void TakeDmg(float dmg)
	{
		regen = false;
		changeRegen = false;

		if (!invincible)
		{
			soundfx.PlayOneShot(hurt);
			currentHealth -= dmg;

			if (healthBar != null)
			{
				healthBar.value = currentHealth;
			}

			if (currentHealth <= 0)
			{
				Death();
			}
		}

		changeRegen = true;
	}

	void RegenHealth(int amt)
	{
		currentHealth += amt;
		healthBar.value = currentHealth;
	}

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

	void Death()
	{
		control.StartCoroutine(control.GameOver(false));
	}

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

	public bool Invincible
	{
		get {return invincible;}
		set {invincible = value;}
	}

	public bool RegenHealthVar
	{
		get {return regen;}
		set {regen = value;}
	}
}
