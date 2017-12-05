using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public GameObject spawnedObjectPrefab;  // The object that you want to spawn.

    public void Spawn()
    {
        // Spawn at this location.
        Instantiate<GameObject>(spawnedObjectPrefab, transform);
    }
}