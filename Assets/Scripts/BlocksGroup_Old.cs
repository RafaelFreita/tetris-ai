using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocksGroup_Old : MonoBehaviour
{
    public void Move(Vector3 deltaPosition)
	{
		transform.position += deltaPosition;
	}

	public void RotateCW()
	{
		transform.Rotate(0, 0, -90);
	}

	public void RotateCCW()
	{
		transform.Rotate(0, 0, 90);
	}

	public Vector3[] GetPositions()
	{
		List<Vector3> positions = new List<Vector3>();
		foreach (Transform child in transform)
		{
			positions.Add(child.position);
		}
		return positions.ToArray();
	}

	public Transform[] GetChildren()
	{
		List<Transform> children = new List<Transform>();
		foreach (Transform child in transform)
		{
			children.Add(child);
		}
		return children.ToArray();
	}
}
