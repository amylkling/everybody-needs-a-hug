using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//creates the enemy health bar and determines its position and visibility

public class EnemyUI : MonoBehaviour {

	#region Variables
	//scripts
	private Enemy enemyScript;					//reference to the enemy's base script
	public EnemyUIDirectControl uiScript;		//reference to the enemy's UI control script

	//creating GUI
	public Canvas canvas;						//reference to the scene's canvas
	public GameObject healthPrefab;				//reference to the UI prefab

	//manipulating GUI
	public float healthPanelOffset = 1.2f;		//amount to offset when displaying the health bar over the enemy
	public GameObject healthPanel;				//reference to the parent UI object
	public Slider healthSlider;					//reference to the health bar UI
	private Renderer selfRenderer;				//reference to the renderer on the enemy
	private CanvasGroup canvasGroup;			//reference to the UI's canvasgroup

	//other
	private GameControl control;				//reference to the GameControl script
	#endregion

	#region Awake
	// Use this for initialization
	void Awake () {
		
		//initialize and instantiate
		canvas = GameObject.Find("EnemyCanvas").GetComponent<Canvas>();
		enemyScript = gameObject.GetComponent<Enemy>();
		healthPanel = Instantiate(healthPrefab) as GameObject;
		healthPanel.transform.SetParent(canvas.transform, false);
		
		healthSlider = healthPanel.GetComponent<Slider>();
		selfRenderer = gameObject.GetComponent<Renderer>();
		canvasGroup = healthPanel.GetComponent<CanvasGroup>();
		control = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GameControl>();

		//let the Enemy script attached to the same enemy know which health bar belongs to it
		enemyScript.healthBar = healthSlider;

		//tell EnemyUIDirectControl script which enemy it is associated with
		uiScript = healthPanel.GetComponent<EnemyUIDirectControl>();
		uiScript.enemyScript = enemyScript;
	}
	#endregion

	#region Update
	// Update is called once per frame
	void Update () {

		//position the health bar above the enemy
		Vector3 worldPos = new Vector3(transform.position.x, transform.position.y + healthPanelOffset, transform.position.z);
		Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
		healthPanel.transform.position = new Vector3(screenPos.x, screenPos.y, screenPos.z);

		//make the health bar visible only when the enemy is visible and the game is not over
		if (selfRenderer.isVisible && !control.CheckGameOver)
		{
			healthPanel.SetActive(true);
		}
		else
		{
			healthPanel.SetActive(false);
		}

		//after the tutorial is displayed, lower opacity of the health bar
		if (!control.CheckTutOn)
		{
			SetAlpha(0.5f);
		}
	}
	#endregion

	#region SetAlpha
	//change the alpha value and deactivate the health bar if it is 0
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
	#endregion
}
