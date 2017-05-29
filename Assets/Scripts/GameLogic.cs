using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MoveDirection
{
	Left, Right, Up, Down
}

public class BoardItem
{
	public int col;
	public int row;

	public int color = 0;
	public ItemCube item;

	public BoardItem(int col, int row)
	{
		this.col = col;
		this.row = row;
	}

	public static BoardItem Create(int col, int row, GameObject prefab)
	{
		if (!prefab)
		{
			Debug.LogError("Prefab is empty!");
			return null;
		}

		BoardItem item = new BoardItem(col, row);

		GameObject obj = GameObject.Instantiate(prefab);
		obj.name = "cube_" + col + "_" + row;

		obj.transform.position = new Vector3(col * GameLogic.Instance.DistanceX, 0, row * GameLogic.Instance.DistanceX);
		
		item.item = obj.GetComponent<ItemCube>();
		if (!item.item)
		{
			Debug.LogError("Create item without logic!");
		}

		item.item.col = col;
		item.item.row = row;

		return item;
	}

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

	internal int Columns = 5;
	internal int Rows = 5;

	internal List<int> GameColors = new List<int>();

	private Dictionary<string, BoardItem> Board = new Dictionary<string, BoardItem>();


	private ItemCube lastCube;

	public string BoardIndex(int x, int y)
	{
		return x + ":" + y;
	}

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
				Board.Add(BoardIndex(i, j), BoardItem.Create(i,j, ItemPrefab));
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonUp(0) && !Input.GetMouseButton(0))
		{
			//No mouse interaction GUI do nothing
			return;
		}
		if (Helper.isPointerOverUI())
		{
			//Mouse is ower GUI
			//Debug.Log("GUI");
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

		ItemCube cube = hit.collider.GetComponentInParent<ItemCube>();
		if (!cube)
		{
			//No itemCube uder mouse
			//Debug.Log("No cube object");
			return;
		}

		if (cube.color != 0)
		{
			//Cube with color;
			return;
		}

		//Debug.Log(hit.collider.transform.parent.name + " cube:" + cube);

		if (Input.GetMouseButtonUp(0))
		{
			//Release mouse
			lastCube = null;
			return;
		}

		if (Input.GetMouseButtonDown(0))
		{
			//Begin drag
			if (cube.color != 0)
			{
				//Click on colored cube
				lastCube = null;
				return;
			}

			cube.Rotate(MoveDirection.Right, GetColor());
			lastCube = cube;
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
		return color;
	}

	void NewMove()
	{
		GameColors.Clear();

		for (int i = 0; i < 3; i++)
		{
			GameColors.Add(Random.Range(1,6));
		}

	}
		
}
