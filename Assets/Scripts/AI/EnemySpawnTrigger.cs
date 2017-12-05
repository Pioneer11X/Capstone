using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour {

    public List<Spawner> spawners;

    public float spawnRate;        // The number of enemies you are supposed to spawn per a minute.


    private bool spawn = false;

    private bool didJustSpawn = true; // This is set to true when we spawn a character and have to wait for a period before we can spawn again.
    private float timer = 0.0f;         // A temporary timer used to go through the time to spawn.

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        // Check to see if the Spawner is in the Camera view.
        if (!didJustSpawn)
        {

            // Go through all the list of spawners and find out which ones are off screen
            foreach ( Spawner spawnerObject in spawners)
            {

                if (!spawnerObject.GetComponent<Renderer>().isVisible)
                {
                    spawnerObject.Spawn();
                    didJustSpawn = true;
                    timer = 0.0f;
                    break;
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
}
