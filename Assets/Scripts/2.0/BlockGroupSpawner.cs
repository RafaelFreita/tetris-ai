using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGroupSpawner : MonoBehaviour
{

    [SerializeField]
    private List<BlocksGroup> groups = new List<BlocksGroup>();

    public BlocksGroup SpawnNext()
	{
        return Instantiate(groups[Random.Range(0, groups.Count)]);
	}

}
