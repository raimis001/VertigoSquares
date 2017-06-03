using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public enum MoveDirection
{
	Left, Right, Up, Down
}



public class GameLogic : MonoBehaviour
{

	public static GameLogic Instance;

	public delegate void ScoreChange(int deltaScore);
	public static event ScoreChange OnScoreChange;

	[Header("Setup")]
	public GameObject ItemPrefab;

	[Range(1,2)]
	public float DistanceX = 1.1f;
	[Range(1, 2)]
	public float DistanceY = 1.1f;

	public Color[] colors = {
		new Color(0x00 / 255f, 0x37 / 255f, 0xFF / 255f, 1),
		new Color(0x09 / 255f, 0xFF / 255f, 0x00 / 255f, 1),
		new Color(0xFF / 255f, 0xEF / 255f, 0x05 / 255f, 1),
		new Color(0xF6 / 255f, 0xEF / 255f, 0x05 / 255f, 1),
		new Color(0x00 / 255f, 0xFF / 255f, 0xEA / 255f, 1),
	};

	[Header("GUI")]
	public GuiNextColors NextColors;
	public GuiNextColors PeepNext;
	public Text ScoreText;
	public GuiSwitch SoundSwitch;

	[Header("Victory")]
	public GameObject VictoryScreen;
	public Text VictoryScore;
	public Text VictoryBest;

	private int score;
	internal int Score
	{
		get { return score; }
		set
		{
			int oldScore = score;
			score = value;

			if (ScoreText) ScoreText.text = score.ToString();
			if (OnScoreChange != null)
			{
				OnScoreChange(score - oldScore);
			}
		}
	}


	internal int Columns = 5;
	internal int Rows = 5;

	internal List<int> GameColors = new List<int>();
	private List<int> PeepColors = new List<int>();
	private List<WeightClass<int>> WeightColors = new List<WeightClass<int>>();


	private Dictionary<string, ItemCube> Board = new Dictionary<string, ItemCube>();

	private readonly Vector2[] neighbours =
	{
		new Vector2(1,0),
		new Vector2(-1,0),
		new Vector2(0,1),
		new Vector2(0,-1),
	};

	private ItemCube lastCube;
	private List<ItemCube> path = new List<ItemCube>();

	//Booster in use 0 - no booster 1 - hammer, 2 - redraw hint, 3 - show next hint
	private int boosterStatus;

	void Awake()
	{
		Instance = this;
		Progress.Load();
		//Debug.Log(Progress.Data);
	}

	void Start()
	{
		if (SoundSwitch) SoundSwitch.Status = Prefs.SoundStatus;

		for (int i = 0; i < Columns; i++)
		{
			for (int j = 0; j < Rows; j++)
			{
				Board.Add(BoardIndex(i, j), CreateCube(i,j));
			}
		}
		score = Progress.Data.Score;
		if (ScoreText) ScoreText.text = score.ToString();

		//Create weighted list
		WeightColors.Add(Weighted.Create(0.5f, 1));
		WeightColors.Add(Weighted.Create(2, 2));
		WeightColors.Add(Weighted.Create(2, 3));
		WeightColors.Add(Weighted.Create(0.5f, 4));
		
		NewMove();
	}

	void Update()
	{

		if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonUp(0) && !Input.GetMouseButton(0))
		{
			//No mouse interaction do nothing
			return;
		}
		if (Helper.isPointerOverUI())
		{
			//Mouse is ower GUI
			//Debug.Log("GUI");
			return;
		}

		if (Input.GetMouseButtonUp(0))
		{
			//Release mouse

			foreach (ItemCube c in Board.Values)
			{
				if (c.Points > 2) Score += c.Points;

				c.ClearScore();
			}

			//Save score
			Progress.Data.Score = score;
			if (Progress.Data.BestScore < score)
			{
				Progress.Data.BestScore = score;
			}

			Progress.Save();

			if (lastCube) lastCube.Selected = false;
			lastCube = null;

			int free = CalcFreeMoves();
			if (free == 0)
			{
				//TODO: finish level
				FinishLevel();
			}
			else
			{
				NewMove();
			}
			return;
		}

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (!Physics.Raycast(ray, out hit))
		{
			//No object unde mouse
			//Debug.Log("No object");
			return;
		}

		ItemCube cube = hit.collider.GetComponent<ItemCube>();
		if (!cube)
		{
			//No itemCube uder mouse
			//Debug.Log("No cube object");
			return;
		}

		if (cube.color != 0)
		{
			//We have hammer booste
			if (Input.GetMouseButtonDown(0) && boosterStatus == 1)
			{
				boosterStatus = 0;
				cube.DestroyColor();
			}


			//Cube with color;
			//Debug.Log("Click on colored cube");
			return;
		}

		//Debug.Log(hit.collider.transform.parent.name + " cube:" + cube);


		
		if (Input.GetMouseButtonDown(0))
		{

			//Begin drag
			//Debug.Log("Rotate cube:" + cube);
			int c = GetColor();
			if (c > -1)
			{
				cube.Rotate(MoveDirection.Right, c);

				if (lastCube) lastCube.Selected = false;
				lastCube = cube;
				if (lastCube) lastCube.Selected = true;
				DrawPath(cube);
			}
			return;
		}

		if (lastCube == null)
		{
			//No start cube
			return;
		}

		int distRow = Mathf.Abs(lastCube.row - cube.row);
		int distCol = Mathf.Abs(lastCube.col - cube.col);
		if (
			distRow > 1 || distCol > 1 ||
			distRow != 1 && distCol != 1 ||
			distRow == 1 && distCol == 1
			)
		{
			//Not valid distance
			//Debug.Log("Not valid distance last:" + lastCube + " current:" + cube);
			return;
		}
		int cc = GetColor();
		if (cc < 0) return;

		MoveDirection dir = cube.col > lastCube.col
			? MoveDirection.Right
			: cube.col < lastCube.col
				? MoveDirection.Left
				: cube.row < lastCube.row
					? MoveDirection.Down
					: MoveDirection.Up;

		cube.Rotate(dir, cc);
		DrawPath(cube);

		if (lastCube) lastCube.Selected = false;
		lastCube = cube;
		if (lastCube) lastCube.Selected = true;

	}

	int GetColor()
	{
		if (GameColors.Count < 1)
		{
			return -1;
		}
		int color = GameColors[0];
		GameColors.RemoveAt(0);
		NextColors.Rearange(GameColors);
		return color;
	}

	void NewMove()
	{
		GameColors.Clear();

		if (PeepColors.Count > 0)
		{
			GameColors.AddRange(PeepColors);
			NextColors.Rearange(GameColors);
			PeepColors.Clear();

			if (PeepNext)
			{
				PeepNext.gameObject.SetActive(false);
			}
			return;
		}

		GenerateColors(GameColors);
		NextColors.Rearange(GameColors);
	}

	void DrawPath(ItemCube cube)
	{
		path.Clear();
		int res = CalcScorePath(cube);
		foreach (ItemCube c in path)
		{
			c.Points = res;
		}
	}

	/// <summary>
	/// Return string index based on col and row for Board Dictionary
	/// </summary>
	/// <param name="col"></param>
	/// <param name="row"></param>
	/// <returns>string index</returns>
	public string BoardIndex(int col, int row)
	{
		return col + ":" + row;
	}


	/// <summary>
	/// Create new cube
	/// </summary>
	/// <param name="col"></param>
	/// <param name="row"></param>
	/// <returns>new ItemCube</returns>
	ItemCube CreateCube(int col, int row)
	{
		GameObject obj = Instantiate(ItemPrefab);
		obj.transform.SetParent(transform);
		obj.transform.localPosition = new Vector3(col * DistanceX, 0, row * DistanceY);

		ItemCube cube = obj.GetComponent<ItemCube>();
		if (!cube)
		{
			Debug.LogError("ItemCube not present!");
			Destroy(obj);
			return null;
		}


		cube.col = col;
		cube.row = row;

		return cube;
	}

	/// <summary>
	/// Find ItemCube in Board Dictionary
	/// </summary>
	/// <param name="col"></param>
	/// <param name="row"></param>
	/// <returns></returns>
	ItemCube GetCube(int col, int row)
	{
		ItemCube cube;

		if (!Board.TryGetValue(BoardIndex(col, row), out cube))
		{
			Debug.LogWarning("No cube index:" + BoardIndex(col, row));
			return null;
		}

		return cube;
	}

	/// <summary>
	/// Calculate score in neighbours by cube
	/// </summary>
	/// <param name="cube"></param>
	/// <returns></returns>
	int CalcScorePath(ItemCube cube)
	{
		return CalcScorePath(cube.col, cube.row, cube.color);
	}

	/// <summary>
	/// Calculate score in neighbours
	/// Recursive call
	/// </summary>
	/// <param name="col"></param>
	/// <param name="row"></param>
	/// <param name="color"></param>
	/// <returns></returns>
	int CalcScorePath(int col, int row, int color)
	{
		if (col < 0 || col >= Columns) return 0;
		if (row < 0 || row >= Rows) return 0;

		ItemCube cube = GetCube(col, row);
		if (!cube) return 0;

		if (path.Contains(cube)) return 0;
		if (cube.color != color) return 0;

		path.Add(cube);

		int result = 1;

		foreach (Vector2 neighbour in neighbours)
		{
			result += CalcScorePath(col + (int)neighbour.x, row + (int)neighbour.y, color);
		}

		return result;
	}

	int CalcFreeMoves()
	{
		int result = 0;
		path.Clear();

		foreach (ItemCube cube in Board.Values)
		{
			if (cube.color > 0) continue;

			int c = CalcScorePath(cube);
			if (c > result)
			{
				result = c;
			}
		}
		//Debug.Log(result);
		return result;
	}

	void GenerateColors(List<int> list)
	{
		//TODO: get free moves

		list.Clear();
		int count = Random.Range(2, 4);

		for (int i = 0; i < count; i++)
		{
			//GameColors.Add(Random.Range(1,5));

			//Get weighted color
			list.Add(WeightColors.GetWeighted());
		}
	}

#region BOOSTERS
	/// <summary>
	/// Event on booster is pressed
	/// </summary>
	/// <param name="BoosterId"></param>
	public void OnBoosterUse(int BoosterId)
	{
		if (BoosterId == 1)
		{
			//Next hint
			NewMove();
			return;
		}

		if (BoosterId == 2)
		{
			ShowPeep();
			return;
		}

		Debug.Log("Booster use:" + BoosterId);
		boosterStatus = BoosterId + 1;
	}

	void ShowPeep()
	{
		GenerateColors(PeepColors);
		
		if (PeepNext)
		{
			PeepNext.Rearange(PeepColors);
			PeepNext.gameObject.SetActive(true);
		}

	}

	#endregion

	void FinishLevel()
	{

		if (VictoryScore)
		{
			VictoryScore.text = Score.ToString();
		}

		if (VictoryBest)
		{
			VictoryBest.text = Progress.Data.BestScore.ToString();
		}

		if (VictoryScreen)
		{
			VictoryScreen.SetActive(true);
		}

		Progress.Data.Score = 0;
		Progress.Data.Board = "";

		Progress.Save();
	}

	public void StartNewGame()
	{

		Score = 0;


		foreach (ItemCube cube in Board.Values)
		{
			cube.DestroyColor(false);
		}

		

		if (VictoryScreen)
		{
			VictoryScreen.SetActive(false);
		}

		NewMove();
	}
	
}
