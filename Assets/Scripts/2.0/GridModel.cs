using NaughtyAttributes;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class GridModel : Agent {

	public const int width = 10;
	public const int height = 25;
	public const int spawnRow = 21;
	private int[,] grid = new int[width, height]; // 0 is open - x > 1 is a tile

	[SerializeField]
	private float timePerUpdate = 1f;
	private float timer = 0;

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

	private int linesFinished = 0;

	private bool rightInput = false;
	private bool leftInput = false;
	private bool rotateInput = false;
	private bool downInput = false;

	#region Debug
	[Button("Debug Set Tile")]
	public void DebugSetTile()
	{
		SetTile(debugX, debugY, debugValue);
	}
	#endregion

	#region Unity
	private void Awake()
	{
		if (spawner == null)
			spawner = FindObjectOfType<BlockGroupSpawner>();
		if (gridView == null)
			gridView = FindObjectOfType<GridVisualizer>();
	}

	private void Start()
	{
		GameReset();
	}

	private void MoveRight()
	{
		currentGroup.x += 1;
		if (!CheckCurrentGroup()) currentGroup.x -= 1;
		else UpdateView();
	}

	private void MoveLeft()
	{
		currentGroup.x -= 1;
		if (!CheckCurrentGroup()) currentGroup.x += 1;
		else UpdateView();
	}

	private void RotateBlock()
	{
		currentGroup.RotateCW();
		if (!CheckCurrentGroup()) currentGroup.RotateCWW();
		else UpdateView();
		rotateInput = false;
	}

	private void ForceBlockDown()
	{
		MoveGroupDown();
		timer = 0;
	}

	int framesPerUpdate = 2;
	private void FixedUpdate()
	{
		timer++;
		if (timer >= framesPerUpdate)
		{
			MoveGroupDown();
			timer = 0;
		}

		//timer += Mathf.Lerp(Time.fixedDeltaTime, Time.fixedDeltaTime / 30, linesFinished / 1000f);
	}
	#endregion

	#region Tetris Logic
	public void SetTile(int x, int y, int value)
	{
		grid[x, y] = value;
	}

	private void MoveGroupDown()
	{
		currentGroup.y -= 1;

		if (!CheckCurrentGroup())
		{
			currentGroup.y += 1;

			SetGroupTiles();
			UpdateGrid();
			SpawnNextGroup();

			if (!CheckCurrentGroup()) { GameReset(true); } // GAME END
		}
		else UpdateView();
	}

	private void GameReset(bool endEpisode = false)
	{
		ResetGrid();
		SpawnNextGroup();

		if (endEpisode)
		{

			//for (int y = 0; y < height; y++)
			//{
			//	for (int x = 0; x < width; x++)
			//	{
			//		if (grid[x, y] != 0)
			//			AddReward(1.0f);
			//	}
			//}

			AddReward(Mathf.Pow(removedRows * width, 2));

			EndEpisode();
		}
	}

	private void SetGroupTiles()
	{
		foreach (Vector2Int tile in currentGroup.GetCurrentTiles())
		{
			grid[tile.x, tile.y] = currentGroup.value;
		}
	}

	private BlocksGroup nextGroup;
	private void SpawnNextGroup()
	{
		if (!nextGroup)
			nextGroup = spawner.SpawnNext();

		currentGroup = nextGroup;
		currentGroup.x = Mathf.RoundToInt(width / 2);
		currentGroup.y = spawnRow;

		nextGroup = spawner.SpawnNext();

		AddReward(1f);

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
				linesFinished++;
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

	private int removedRows = 0;
	private void RemoveRow(int y)
	{
		for (int x = 0; x < width; x++)
		{
			grid[x, y] = 0;
		}
		removedRows++;
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
	#endregion

	#region MLAgents

	public override void OnEpisodeBegin()
	{
		GameReset();
	}


	public override void CollectObservations(VectorSensor sensor)
	{
		// Grid -> 25 x 10 = 250
		//for (int y = 0; y < height; y++)
		//{
		//	for (int x = 0; x < width; x++)
		//	{
		//		sensor.AddObservation(grid[x, y]);
		//	}
		//}

		// For each column, observe height
		for (int x = 0; x < width; x++)
		{
			int height = 0;
			for (int y = height; y > 0; y--)
			{
				if(grid[x,y] != 0)
				{
					height = y;
					break;
				}
			}
			sensor.AddObservation(height);
		}

		// Group -> 1
		sensor.AddObservation(currentGroup.id);

		// Group position -> 2
		sensor.AddObservation(currentGroup.x);
		sensor.AddObservation(currentGroup.y);

		// Group rotation -> 1
		sensor.AddObservation(currentGroup.currentRotation);

		// Old Total = 250 + 1 + 2 + 1 = 254
		// Total = 10 + 1 + 2 + 1 = 14
	}

	public override void OnActionReceived(float[] vectorAction)
	{
		// Actions, size = 3
		float moveSide = vectorAction[0];
		float rotate = vectorAction[1];

		if (moveSide < -0.5) MoveLeft();
		if (moveSide > 0.5) MoveRight();
		if (rotate > 0) RotateBlock();
	}

	public override void Heuristic(float[] actionsOut)
	{
		actionsOut[0] = Input.GetAxis("Horizontal");
		actionsOut[1] = Input.GetKeyDown(KeyCode.UpArrow) ? 1.0f : 0.0f;
	}

	#endregion
}
