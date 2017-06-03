using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GuiBoosterButton : MonoBehaviour, IPointerDownHandler
{
	public int BoosterId;
	public int FillNeed;

	[Range(0, 1)]
	public float BoosterFill;

	public Image FillImage;

	public UnityEvent OnBoosterUse;

	private int currentFill;

	private void OnEnable()
	{
		GameLogic.OnScoreChange += OnScoreChange;
	}

	private void OnDisable()
	{
		GameLogic.OnScoreChange -= OnScoreChange;
	}

	void Start()
	{
		currentFill = 0;
		BoosterFill = 0;
	}

	private void Update()
	{
		if (FillImage) FillImage.fillAmount = 1 - BoosterFill;
	}
	
	public void OnPointerDown(PointerEventData eventData)
	{
		//Debug.Log("Click on booster:" + BoosterId);
		if (currentFill < FillNeed)
		{
			//TODO: booster not ready message
			Debug.Log("Booster not filled");
			return;
		}

		OnBoosterUse.Invoke();
		currentFill = 0;
		BoosterFill = 0;
	}

	void OnScoreChange(int deltaScore)
	{
		//Debug.Log("Score change:" + deltaScore);
		if (deltaScore < 0)
		{
			currentFill = 0;
			BoosterFill = 0;
			return;
		}

		currentFill = Mathf.Clamp(currentFill + deltaScore, 0, FillNeed);
		BoosterFill = (float)currentFill / FillNeed;
	}
}
