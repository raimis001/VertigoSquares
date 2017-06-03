using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class SwitchEvent : UnityEvent<bool> { }

public class GuiSwitch : MonoBehaviour, IPointerDownHandler
{
	public Image ImageOn;
	public Image ImageOff;

	public SwitchEvent OnSwitch;

	private bool status = true;
	internal bool Status
	{
		get { return status; }
		set
		{
			status = value;
			SetImages();
		}
	}

	void Start()
	{
		SetImages();
	}

	void SetImages()
	{
		if (ImageOn) ImageOn.enabled = Status;
		if (ImageOff) ImageOff.enabled = !Status;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		status = !status;
		SetImages();

		OnSwitch.Invoke(Status);
	}
}
