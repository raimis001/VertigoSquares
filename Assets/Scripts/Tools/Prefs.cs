using UnityEngine;
using System.Collections;

public static class Prefs 
{

	public static bool SoundStatus
	{
		get
		{
			int status = PlayerPrefs.GetInt("SoundStatus", 0);
			return status == 0;
		}
		set
		{
			PlayerPrefs.SetInt("SoundStatus", value ? 0 : 1);
		}
	}

}
