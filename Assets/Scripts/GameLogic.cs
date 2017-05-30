using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum MoveDirection
{
	Left, Right, Up, Down
}
	
public class GameLogic : MonoBehaviour
{

	public static GameLogic Instance;

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
	public Text ScoreText;

	private int score;
	internal int Score
	{
		get { return score; }
		set
		{
			score = value;
			if (ScoreText) ScoreText.text = score.ToString();
		}
	}


	internal int Columns = 5;
	internal int Rows = 5;

	internal List<int> GameColors = new List<int>();

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

	void Awake()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start()
	{
		for (int i = 0; i < Columns; i++)
		{
			for (int j = 0; j < Rows; j++)
			{
				Board.Add(BoardIndex(i, j), CreateCube(i,j));
			}
		}
		Score = 0;
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

			lastCube = null;
			NewMove();
			return;
		}

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (!Physics.Raycast(ray, out hit))
		{
			//No object unde mouse
			Debug.Log("No object");
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
			//Cube with color;
			//Debug.Log("Click on colored cube");
			return;
		}

		//Debug.Log(hit.collider.transform.parent.name + " cube:" + cube);


		
		if (Input.GetMouseButtonDown(0))
		{
			//Begin drag
			//Debug.Log("Rotate cube:" + cube);
			cube.Rotate(MoveDirection.Right, GetColor());
			lastCube = cube;
			DrawPath(cube);

			return;
		}

		if (lastCube == null)
		{
			//No start cube
			return;
		}

		if (
			lastCube.col == cube.col && Mathf.Abs(lastCube.row - cube.row) != 1 ||
			lastCube.row == cube.row && Mathf.Abs(lastCube.col - cube.col) != 1 )
		{
			//Not valid distance
			//Debug.Log("Not valid distance last:" + lastCube + " current:" + cube);
			return;
		}


		MoveDirection dir = cube.col > lastCube.col
			? MoveDirection.Right
			: cube.col < lastCube.col
				? MoveDirection.Left
				: cube.row < lastCube.row
					? MoveDirection.Down
					: MoveDirection.Up;

		cube.Rotate(dir, GetColor());
		DrawPath(cube);

		lastCube = cube;
	}

	int GetColor()
	{
		if (GameColors.Count < 1)
		{
			NewMove();
		}
		int color = GameColors[0];
		GameColors.RemoveAt(0);
		NextColors.Rearange();
		return color;
	}

	void NewMove()
	{
		GameColors.Clear();

		for (int i = 0; i < 3; i++)
		{
			GameColors.Add(Random.Range(1,5));
		}
		NextColors.Rearange();
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
		ItemCube cube = null;

		if (!Board.TryGetValue(BoardIndex(col, row), out cube))
		{
			Debug.LogWarning("No cube index:" + BoardIndex(col, row));
			return null;
		}

		return cube;
	}

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

}
