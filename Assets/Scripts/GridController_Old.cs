using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridController_Old : MonoBehaviour {

	[SerializeField]
	private BlocksGroup_Old currentGroup;

	[SerializeField]
	private float updateTime = 1f;

	private Spawner spawner;

	private const int width = 10;
	private const int height = 20;
	private Transform[,] grid = new Transform[width, height];

	private void Awake()
	{
		spawner = FindObjectOfType<Spawner>();
	}

	private void Start()
	{
		GameObject newGroup = spawner.SpawnNext();
		currentGroup = newGroup.GetComponent<BlocksGroup_Old>();
	}

	private void Update()
	{
		if (currentGroup == null) return;

		int xDelta = 0;

		if (Input.GetKeyDown(KeyCode.RightArrow))
			xDelta += 1;

		if (Input.GetKeyDown(KeyCode.LeftArrow))
			xDelta -= 1;

		// Try moving horizontally
		if (xDelta != 0)
		{
			Vector3 xMovement = new Vector3(xDelta, 0, 0);
			currentGroup.Move(xMovement);
			if (!CheckCurrentGroup())
				currentGroup.Move(-xMovement);
		}

		if (Input.GetKeyDown(KeyCode.UpArrow)) // Rotate
		{
			currentGroup.RotateCW();

			if (!CheckCurrentGroup())
				currentGroup.RotateCCW();
		}

		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			Debug.Log("Move");
			currentGroup.Move(-Vector3.up);

			if (!CheckCurrentGroup()) // When movement is not valid
			{
				currentGroup.Move(Vector3.up); // Move group back up
				SetGroupTiles(); // Set tiles
				UpdateGrid(); // Update grid

				GameObject newGroup = spawner.SpawnNext();
				currentGroup = newGroup.GetComponent<BlocksGroup_Old>();
				if (!CheckCurrentGroup())
				{
					GameEnd();
				}
			}
		}

		// Timer
	}

	private void SetGroupTiles()
	{
		foreach (Transform child in currentGroup.GetChildren())
		{
			Vector3 position = child.position;
			Vector2 pos = new Vector2(position.x, position.y);
			pos.Round();

			grid[(int)pos.x, (int)pos.y] = child;
		}
	}

	private void GameEnd()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	private bool CheckCurrentGroup()
	{
		foreach (Transform child in currentGroup.GetChildren())
		{
			Vector3 position = child.position;
			Vector2 pos = new Vector2(position.x, position.y);
			pos.Round();

			Debug.Log($"Checking at ({(int)pos.x}, {(int)pos.y})");

			if (!IsValidPosition((int)pos.x, (int)pos.y))
				return false;
		}
		return true;
	}

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
	}

	public bool IsValidPosition(int x, int y)
	{
		if (!IsInsideGrid(x, y)) { Debug.Log($"Failed at ({x}, {y})"); return false; } // Outside grid
		if (IsCollision(x, y)) { Debug.Log($"Failed at ({x}, {y})"); return false; } // Already occupied
		return true;
	}

	private bool IsCollision(int x, int y)
	{
		return grid[x, y] != null;
	}

	private static bool IsInsideGrid(int x, int y)
	{
		return x >= 0
			&& x < width
			&& y >= 0;
	}

	private void RemoveRow(int y)
	{
		for (int x = 0; x < width; x++)
		{
			if (grid[x, y] != null)
			{
				Destroy(grid[x, y].gameObject); // TODO: Could add some fancy particles here
				grid[x, y] = null;
			}
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
			if (grid[x, y] == null) continue; // Skip tile if empty

			grid[x, y - 1] = grid[x, y]; // Assign block to under tile
			grid[x, y] = null; // Clean tile

			grid[x, y - 1].position -= Vector3.up; // Move block down
		}
	}

	private bool RowIsFull(int y)
	{
		for (int x = 0; x < width; x++)
			if (grid[x, y] == null)
				return false;
		return true;
	}

}
