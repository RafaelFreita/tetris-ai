using NaughtyAttributes;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class GridModel : Agent {

	public const int width = 6;
	public const int height = 12;
	public const int maxHeight = height + 5;
	public const int spawnRow = height + 1;
	private int[,] grid = new int[width, maxHeight]; // 0 is open - x > 1 is a tile

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
		AddReward(-0.01f);
		currentGroup.x += 1;
		if (!CheckCurrentGroup()) currentGroup.x -= 1;
		else UpdateView();
	}

	private void MoveLeft()
	{
		AddReward(-0.01f);
		currentGroup.x -= 1;
		if (!CheckCurrentGroup()) currentGroup.x += 1;
		else UpdateView();
	}

	private void RotateBlock()
	{
		AddReward(-0.01f);
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

	int framesPerUpdate = 5;
	private void FixedUpdate()
	{

		if (shouldMoveLeft)
		{
			shouldMoveLeft = false;
			MoveLeft();
		}

		if (shouldMoveRight)
		{
			shouldMoveRight = false;
			MoveRight();
		}

		if (shouldRotate)
		{
			shouldRotate = false;
			RotateBlock();
		}

		timer++;
		if (timer >= framesPerUpdate)
		{
			MoveGroupDown();
			timer = 0;
		}
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

			if (CheckGameEnd()) { GameReset(true); } // GAME END

			SpawnNextGroup();
		}
		UpdateView();
	}

	private bool CheckGameEnd()
	{
		for (int x = 0; x < width; x++)
		{
			if (grid[x, height] != 0)
				return true;
		}
		return false;
	}

	private void GameReset(bool endEpisode = false)
	{
		ResetGrid();
		SpawnNextGroup();

		if (endEpisode)
		{
			//SetReward(Mathf.Pow(removedRows * width, 2));
			//removedRows = 0;

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
	private float newBlockMul = 5f;
	private void SpawnNextGroup()
	{
		if (!nextGroup)
			nextGroup = spawner.SpawnNext();

		currentGroup = nextGroup;
		currentGroup.x = Mathf.RoundToInt(width / 2);
		currentGroup.y = spawnRow;

		nextGroup = spawner.SpawnNext();

		//AddReward(1f);

		UpdateView();
	}

	[Button("Update Grid")]
	public void UpdateGrid()
	{
		// For each row (bottom to top)
		for (int y = 0; y < maxHeight; y++)
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

	private bool CheckCurrentGroup(bool checkOver = false)
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
		for (int y = 0; y < maxHeight; y++)
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
			&& y < maxHeight;
	}

	private int removedRows = 0;
	private void RemoveRow(int y)
	{
		for (int x = 0; x < width; x++)
		{
			grid[x, y] = 0;
		}
		AddReward(1f);
	}

	private void DecreaseRowsAbove(int row)
	{
		for (int y = row; y < maxHeight; y++)
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

	private float holesMul = -3f;
	private float maxHeightMul = -2f;
	private float bumpMul = -3f;

	public override void CollectObservations(VectorSensor sensor)
	{
		// OBSERVATIONS

		// Grid -> height x 10 = 250
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				sensor.AddObservation(grid[x, y]);
			}
		}

		// Group -> 1
		sensor.AddObservation(currentGroup.id);

		// Group position -> 2
		sensor.AddObservation(currentGroup.x);
		sensor.AddObservation(currentGroup.y);

		// Group rotation -> 1
		sensor.AddObservation(currentGroup.currentRotation);

		// Total = 200 + 1 + 2 + 1 = 204
		// Smaller total = (5*4) + 4 = 20 + 4 = 24
	}

	private int GetTotalHoles()
	{
		int t = 0;
		for (int y = 0; y < 19; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (grid[x, y] == 0)
				{
					for (int cy = y; cy < height; cy++)
					{
						if (grid[x, cy] != 0)
							t++;
					}
				}
			}
		}
		return t;
	}

	private int GetMaxHeight()
	{
		int h = 0;
		for (int x = 0; x < width; x++)
		{
			int th = 0;
			for (int y = height; y >= 0; y--)
			{
				if (grid[x, y] != 0)
				{
					th = y + 1;
					break;
				}
			}
			h += th;
		}
		return h;
	}

	private int GetTotalBump()
	{
		int[] heights = new int[width];

		for (int x = 0; x < width; x++)
		{
			int th = 0;
			for (int y = height; y >= 0; y--)
			{
				if (grid[x, y] != 0)
				{
					th = y;
					break;
				}
			}
			heights[x] = th;
		}


		int t = 0;
		for (int x = 0; x < width - 1; x++)
		{
			t += Mathf.Abs(heights[x] - heights[x + 1]);
		}

		return t;
	}


	private bool shouldMoveLeft = false;
	private bool shouldMoveRight = false;
	private bool shouldRotate = false;

	public override void OnActionReceived(float[] vectorAction)
	{
		// Actions, size = 3
		float moveSide = vectorAction[0];
		float rotate = vectorAction[1];

		if (moveSide < -0.5) shouldMoveLeft = true; // MoveLeft();
		if (moveSide > 0.5) shouldMoveRight = true; // MoveRight();
		if (rotate > 0) shouldRotate = true; // RotateBlock();
	}


	private bool shouldPressRotate = false;
	private int xDelta = 0;
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.RightArrow))
			xDelta += 1;

		if (Input.GetKeyDown(KeyCode.LeftArrow))
			xDelta -= 1;

		if (Input.GetKeyDown(KeyCode.UpArrow))
			shouldPressRotate = true;
	}

	public override void Heuristic(float[] actionsOut)
	{
		actionsOut[0] = xDelta;
		actionsOut[1] = shouldPressRotate ? 1.0f : 0.0f;

		xDelta = 0;
		shouldPressRotate = false;
	}

	#endregion
}
