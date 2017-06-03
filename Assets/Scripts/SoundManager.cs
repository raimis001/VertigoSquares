using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
	public void SwitchSound(bool status)
	{
		Prefs.SoundStatus = status;
		AudioListener.volume = Prefs.SoundStatus ? 1 : 0;
	}
}
