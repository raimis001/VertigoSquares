using UnityEngine;
using System.Collections;

public enum SoundKind
{
	Cube,
	Botton,
	Victory,
	Hammer,
	Switch,
	Peep,
	Bomb
}

public class SoundManager : MonoBehaviour
{
	public static SoundManager Instance;

	private AudioSource[] sounds;

	void Awake()
	{
		Instance = this;
		sounds = GetComponentsInChildren<AudioSource>();

	}

	public void SwitchSound(bool status)
	{
		Prefs.SoundStatus = status;
		AudioListener.volume = Prefs.SoundStatus ? 1 : 0;
	}

	public void PlaySound(SoundKind kind, bool one)
	{
		int snd = (int) kind;
		if (snd >= sounds.Length) return;

		if (sounds[snd])
		{
			if (!one)
			{
				sounds[snd].Play();
			}
			else
			{
				sounds[snd].PlayOneShot(sounds[snd].clip);
			}
		}
	}

	public static void Play(SoundKind kind, bool one = false)
	{
		if (!Instance) return;
		Instance.PlaySound(kind, one);
	}
	
}
