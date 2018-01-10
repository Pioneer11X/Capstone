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
        GameObject newEmeney = Instantiate<GameObject>(finalBossPrefab, transform);
        ThirdPCharacter player = GameObject.FindGameObjectWithTag("Player").GetComponent<ThirdPCharacter>();
        Debug.Assert(null != player);
        player.m_combat.enemyList.Add(newEmeney);
    }

    public void Spawn()
    {
        // Spawn at this location.

        Vector3 pos = new Vector3(0, 0, 0);
        Transform tempTrans = transform;
        tempTrans.position = pos;

        GameObject newEmeney = Instantiate<GameObject>(spawnedObjectPrefab, tempTrans);
        ThirdPCharacter player = GameObject.FindGameObjectWithTag("Player").GetComponent<ThirdPCharacter>();
        Debug.Assert(null != player);
        player.m_combat.enemyList.Add(newEmeney);
    }

    public void Update()
    {
        spawnWeight += Time.deltaTime;
    }
}