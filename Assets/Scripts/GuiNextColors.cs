using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GuiNextColors : MonoBehaviour
{

	public void Rearange()
	{
		int cnt = GameLogic.Instance.GameColors.Count;

		for (int i = 0; i < transform.childCount; i++)
		{
			if (i >= cnt)
			{
				//Child is not active
				transform.GetChild(i).gameObject.SetActive(false);
				continue;
			}
			
			Image image = transform.GetChild(i).gameObject.GetComponent<Image>();
			if (!image)
			{
				//Not image component
				continue;
			}

			//Set next move color
			image.color = GameLogic.Instance.colors[GameLogic.Instance.GameColors[i]];

			transform.GetChild(i).gameObject.SetActive(true);
		}
		
	}
}
