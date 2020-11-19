using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Block", menuName = "Tetris Blocks")]
public class BlocksGroup : ScriptableObject
{
    public int value;
    [HideInInspector] public int x;
    [HideInInspector] public int y;

    public List<Vector2IntArray> positions = new List<Vector2IntArray>();
    private int currentPosition = 0;

    public void RotateCW()
	{
        currentPosition++;
        if (currentPosition >= positions.Count) currentPosition = 0;
	}
    public void RotateCWW()
    {
        currentPosition--;
        if (currentPosition < 0) currentPosition = positions.Count - 1;
    }

    public Vector2Int[] GetCurrentTiles()
	{
        Vector2Int[] tiles = new Vector2Int[4];
		for (int i = 0; i < 4; i++)
		{
            Vector2Int current = positions[currentPosition].values[i];
            tiles[i] = new Vector2Int(current.x + x, current.y + y);
        }
        return tiles;
    }
}

[System.Serializable]
public class Vector2IntArray {
    public Vector2Int[] values = new Vector2Int[4];
}

[System.Serializable]
public class Vector2Int {
    public int x;
    public int y;

    public Vector2Int(int x, int y)
	{
        this.x = x;
        this.y = y;
	}
}