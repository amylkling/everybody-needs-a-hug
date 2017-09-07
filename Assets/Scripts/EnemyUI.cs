using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour {

	//script variables
	private Enemy enemyScript;
	public EnemyUIDirectControl uiScript;

	//variables for creating GUI
	public Canvas canvas;
	public GameObject healthPrefab;

	//variables for manipulating GUI
	public float healthPanelOffset = 1.2f;
	public GameObject healthPanel;
	public Slider healthSlider;
	private Renderer selfRenderer;
	private CanvasGroup canvasGroup;
	public float viewRange = 15f;

	private GameControl control;
	

	// Use this for initialization
	void Awake () {
		//initialize and instantiate
		canvas = GameObject.Find("EnemyCanvas").GetComponent<Canvas>();
		enemyScript = gameObject.GetComponent<Enemy>();
		healthPanel = Instantiate(healthPrefab) as GameObject;
		healthPanel.transform.SetParent(canvas.transform, false);
		
		healthSlider = healthPanel.GetComponent<Slider>();
		selfRenderer = gameObject.GetComponent<Renderer>();

		//let the Enemy script attached to the same enemy know which health bar belongs to it
		enemyScript.healthBar = healthSlider;

		canvasGroup = healthPanel.GetComponent<CanvasGroup>();

		//this is purely so that this script can tell EnemyUIDirectControl script which enemy it is associated with
		uiScript = healthPanel.GetComponent<EnemyUIDirectControl>();
		uiScript.enemyScript = enemyScript;

		control = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GameControl>();
	
	}
	
	// Update is called once per frame
	void Update () {

		//position the health bar above the enemy
		Vector3 worldPos = new Vector3(transform.position.x, transform.position.y + healthPanelOffset, transform.position.z);
		Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
		healthPanel.transform.position = new Vector3(screenPos.x, screenPos.y, screenPos.z);

		//track camera distance and make the health bar invisible when the player is far enough away
		//float distance = (worldPos - Camera.main.transform.position).magnitude;
		//float alpha = viewRange - distance / 2.0f;
		//SetAlpha(alpha);

		if (selfRenderer.isVisible && !control.CheckGameOver)
		{
			healthPanel.SetActive(true);
		}
		else
		{
			healthPanel.SetActive(false);
		}

		if (!control.CheckTutOn)
		{
			SetAlpha(0.5f);
		}

	
	}

	//make health bar invisible
	public void SetAlpha(float alpha)
	{
		canvasGroup.alpha = alpha;
		
		if (alpha <= 0)
		{
			healthPanel.SetActive(false);
		}
		else
		{
			healthPanel.SetActive(true);
		}
	}
	
}
