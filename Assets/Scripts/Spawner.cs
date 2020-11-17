using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    [SerializeField]
    private List<GameObject> groups = new List<GameObject>();

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			SpawnNext();
		}
	}

	private GameObject SpawnNext()
	{
        int index = Random.Range(0, groups.Count);

        GameObject spawnedGroup = Instantiate(groups[index], transform.position, Quaternion.identity);
        return spawnedGroup;
	}

}
