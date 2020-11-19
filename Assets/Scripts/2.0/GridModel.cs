using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridModel : MonoBehaviour {

	public const int width = 10;
	public const int height = 25;
	public const int spawnRow = 23;
	private int[,] grid = new int[width, height]; // 0 is open - x > 1 is a tile

	[SerializeField]
	private BlocksGroup currentGroup;

	[Header("References")]
	[SerializeField]
	private GridVisualizer gridView;
	[SerializeField]
	private BlockGroupSpawner spawner;

	[BoxGroup("Debug")]
	[SerializeField] private int debugX;
	[BoxGroup("Debug")]
	[SerializeField] private int debugY;
	[BoxGroup("Debug")]
	[SerializeField] private int debugValue;

	[Button("Debug Set Tile")]
	public void DebugSetTile()
	{
		SetTile(debugX, debugY, debugValue);
	}

	public void SetTile(int x, int y, int value)
	{
		grid[x, y] = value;
	}

	private void Awake()
	{
		if (spawner == null)
			spawner = FindObjectOfType<BlockGroupSpawner>();
		if (gridView == null)
			gridView = FindObjectOfType<GridVisualizer>();
	}

	private void Start()
	{
		ResetGrid();
		SpawnNextGroup();
	}

	private void Update()
	{

		int xDelta = 0;
		if (Input.GetKeyDown(KeyCode.RightArrow))
			xDelta += 1;

		if (Input.GetKeyDown(KeyCode.LeftArrow))
			xDelta -= 1;

		if (xDelta != 0)
		{
			currentGroup.x += xDelta;
			if (!CheckCurrentGroup()) currentGroup.x -= xDelta;
			else UpdateView();
		}

		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			currentGroup.RotateCW();
			if (!CheckCurrentGroup()) currentGroup.RotateCWW();
			else UpdateView();
		}

		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			currentGroup.y -= 1;

			if (!CheckCurrentGroup())
			{
				currentGroup.y += 1;

				SetGroupTiles();
				UpdateGrid();
				SpawnNextGroup();

				if (!CheckCurrentGroup()) { GameEnd(); } // GAME END
			}
			else UpdateView();
		}
	}

	private void GameEnd()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	private void SetGroupTiles()
	{
		foreach (Vector2Int tile in currentGroup.GetCurrentTiles())
		{
			grid[tile.x, tile.y] = currentGroup.value;
		}
	}

	private void SpawnNextGroup()
	{
		currentGroup = spawner.SpawnNext();
		currentGroup.x = Mathf.RoundToInt(width / 2);
		currentGroup.y = spawnRow;
		UpdateView();
	}

	[Button("Update Grid")]
	public void UpdateGrid()
	{
		// For each row (bottom to top)
		for (int y = 0; y < height; y++)
		{
			// Check if row is full
			if (RowIsFull(y))
			{
				RemoveRow(y); // Remove row
				DecreaseRowsAbove(y + 1); // Decrease all above
				y -= 1; // Stay in the same row, since it's a new one
			}
		}

		UpdateView();
	}

	private void UpdateView()
	{
		int[,] clonedGrid = (int[,])grid.Clone();
		GridWithCurrentGroup(ref clonedGrid);

		gridView.UpdateView(clonedGrid);
	}

	private void GridWithCurrentGroup(ref int[,] grid)
	{
		foreach (Vector2Int tile in currentGroup.GetCurrentTiles())
		{
			grid[tile.x, tile.y] = currentGroup.value;
		}
	}

	private bool CheckCurrentGroup()
	{
		foreach (Vector2Int tile in currentGroup.GetCurrentTiles())
		{
			if (!IsValidPosition(tile.x, tile.y))
				return false;
		}
		return true;
	}

	private void ResetGrid()
	{
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				grid[x, y] = 0;
			}
		}

		gridView.UpdateView(grid);
	}

	private bool IsValidPosition(int x, int y)
	{
		if (!IsInsideGrid(x, y)) return false; // Outside grid
		if (IsCollision(x, y)) return false; // Already occupied
		return true;
	}

	private bool IsCollision(int x, int y)
	{
		return grid[x, y] != 0;
	}

	private static bool IsInsideGrid(int x, int y)
	{
		return x >= 0
			&& x < width
			&& y >= 0
			&& y < height;
	}

	private void RemoveRow(int y)
	{
		for (int x = 0; x < width; x++)
		{
			grid[x, y] = 0;
		}
	}

	private void DecreaseRowsAbove(int row)
	{
		for (int y = row; y < height; y++)
			DecreaseRow(y);
	}

	private void DecreaseRow(int y)
	{
		for (int x = 0; x < width; x++)
		{
			grid[x, y - 1] = grid[x, y]; // Assign block to under tile
			grid[x, y] = 0; // Clean tile
		}
	}

	private bool RowIsFull(int y)
	{
		for (int x = 0; x < width; x++)
			if (grid[x, y] == 0)
				return false;
		return true;
	}

}
