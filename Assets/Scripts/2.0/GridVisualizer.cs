using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridVisualizer : MonoBehaviour
{

	private int width;
	private int height;
    private int[,] grid;

    public void UpdateView(int[,] grid)
	{
        this.grid = grid;
		width = grid.GetLength(0);
		height = grid.GetLength(1);
	}

	private void OnDrawGizmos()
	{
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (grid[x, y] == 0)
					Gizmos.color = Color.white;
				else
					Gizmos.color = Color.green;

				Gizmos.DrawCube(transform.position + new Vector3(x + 0.5f, y + 0.5f), Vector3.one);
			}
		}
	}
}
