using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
[Serializable]
public class PlayerData
{
	public int Score;
	public int BestScore;
	public string Board;
	public override string ToString()
	{
		return "Score:" + Score + " board:" + Board;
	}
}


public static class Progress
{
	public static PlayerData Data = new PlayerData();

	private const string FileName = "/save.dat";
	private static string FilePath
	{
		get { return Application.persistentDataPath + FileName; }
	}

	public static void Save()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(FilePath);

		bf.Serialize(file, Data);
		file.Close();
		//Debug.Log("Saved data:" + Data);
	}

	public static void Load()
	{
		if (File.Exists(FilePath))
		{
			Debug.Log("Load data");
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(FilePath, FileMode.Open);

			Data = (PlayerData)bf.Deserialize(file);

			file.Close();

		}
	}

	public static void ClearData()
	{
		Data = new PlayerData();
		Save();
	}
}

