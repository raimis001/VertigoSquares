using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GuiBoosterButton : MonoBehaviour, IPointerDownHandler
{
	public int BoosterId;

	[Range(0, 1)]
	public float BoosterFill;

	public Image FillImage;

	public UnityEvent OnBoosterUse;

	private void Update()
	{
		if (FillImage) FillImage.fillAmount = BoosterFill;
	}
	
	public void OnPointerDown(PointerEventData eventData)
	{
		//Debug.Log("Click on booster:" + BoosterId);
		OnBoosterUse.Invoke();
	}
}
