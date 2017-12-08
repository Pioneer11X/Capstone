using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public GameObject spawnedObjectPrefab;  // The object that you want to spawn.

    public GameObject finalBossPrefab;      // The prefab instance for the final Enemy.

    public float spawnWeight = 0.0f;        // This variable is used to determine the priority for spwning at this point. The igher the weight, more priority should be given.

    // TODO: Condense into one function.
    public void SpawnBoss()
    {
        // Spawn at this location.
        Instantiate<GameObject>(finalBossPrefab, transform);
    }

    public void Spawn()
    {
        // Spawn at this location.
        Instantiate<GameObject>(spawnedObjectPrefab, transform);
    }

    public void Update()
    {
        spawnWeight += Time.deltaTime;
    }
}