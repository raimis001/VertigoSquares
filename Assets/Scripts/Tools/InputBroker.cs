using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class InputBroker : MonoBehaviour
{
	public static KeyCode actionKeyCode = KeyCode.E;
	private static bool actionKey;
	public static bool ActionKey
	{
		set
		{
			actionKey = value;
		}
		get
		{
			return actionKey || Input.GetKeyDown(actionKeyCode);
		}
	}

	private void LateUpdate()
	{
		actionKey = false;
	}


	public static bool GetMouseButtonDown(int mouseId, bool checkUI = true)
	{
		if (!Input.GetMouseButtonDown(mouseId)) return false;

		return !checkUI || !isPointerOverUI();
	}

	public static bool isPointerOverUI()
	{
		if (!EventSystem.current) return false;

		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}

}
