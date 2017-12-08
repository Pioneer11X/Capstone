using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour {

    public List<Spawner> spawners;

    public float spawnRate;        // The number of enemies you are supposed to spawn per a minute.

    public int maximumEnemies = 5;

    public int totalEnemiesForPlaytest = 15;    // The last enemy would be different.

    private bool spawn = false;

    private bool didJustSpawn = true;   // This is set to true when we spawn a character and have to wait for a period before we can spawn again.
    private float timer = 0.0f;         // A temporary timer used to go through the time to spawn.
    private int enemiesSpawned = 0;     // Counter

    // Use this for initialization
    void Start () {


	}
	
	// Update is called once per frame
	void Update () {

        // Check to see if the timer allows you to spawn, and if you already spawned the boss.
        if (!didJustSpawn && (enemiesSpawned < totalEnemiesForPlaytest))
        {
            // Check to see if the maximum limit allows you to spawn.
            if (canSpawn())
            {

                Spawner bestSpawner = null;  // The best spawner to spawn the enemy at.
                float bestWeight = 0.0f;        // The best weight so far.

                // Check to see if the Spawner is in the Camera view.
                // Go through all the list of spawners and find out which ones are off screen and spawn at the first one. No fancy random thingy.
                for (int i = 0; i < spawners.Count; i++)
                {
                    Spawner spawnerObject = spawners[i];
                    if (!spawnerObject.GetComponent<Renderer>().isVisible)
                    {
                        // If the object is invisible, Compare the weights with the current highest and update the highest.
                        if (spawnerObject.spawnWeight > bestWeight) {
                            bestSpawner = spawnerObject;
                            bestWeight = spawnerObject.spawnWeight;
                        }
                    }
                }

                // One final check.
                if ( null != bestSpawner && !bestSpawner.GetComponent<Renderer>().isVisible)
                {
                    Debug.Assert(enemiesSpawned <= totalEnemiesForPlaytest);
                    if ( enemiesSpawned < totalEnemiesForPlaytest)
                    {
                        bestSpawner.Spawn();
                        enemiesSpawned++;
                        bestSpawner.spawnWeight = 0.0f; // Reset the Weight. It just spawned.
                        didJustSpawn = true;
                        timer = 0.0f;
                    }
                    else if ( enemiesSpawned == totalEnemiesForPlaytest)
                    {
                        bestSpawner.SpawnBoss();
                        enemiesSpawned++;
                        bestSpawner.spawnWeight = 0.0f; // Reset the Weight. It just spawned.
                        didJustSpawn = true;
                        timer = 0.0f;
                    }
                    
                }
            }
        }
        else
        {
            if (timer > (60.0f / spawnRate))
            {
                didJustSpawn = false;
            }
        }

        timer += Time.deltaTime;

    }

    // Can the Enemy spawn. Is the number of enemies higher than maximum allowed?
    bool canSpawn()
    {
        return (GameObject.FindGameObjectsWithTag("Enemy").Length < maximumEnemies);
    }
}
