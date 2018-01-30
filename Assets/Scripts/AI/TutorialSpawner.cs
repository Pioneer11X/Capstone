using UnityEngine;

/// <summary>
/// Handles enemies for the tutorial combat areas
/// Number of enemies and respawn points must be the same
/// </summary>
public class TutorialSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] Enemy;
    [SerializeField] GameObject[] RespawnPos;

    private bool activeEnemy;

	// Use this for initialization
	void Start ()
    {
        activeEnemy = true;
        Debug.Assert(Enemy.Length == RespawnPos.Length, "Number of respawn points does not match the number of enemies");
	}
	
	// Update is called once per frame
	void Update ()
    {
        activeEnemy = false;
        for (int i = 0; i < Enemy.Length; i++)
        {
            if (Enemy[i].activeSelf)
            {
                activeEnemy = true;
            }
        }
	}


    private void EnemyReset()
    {
        for (int i = 0; i < Enemy.Length; i++)
        {
            Enemy[i].SetActive(true);
            Enemy[i].GetComponent<Enemy>().Health = 20;
            Enemy[i].transform.position = RespawnPos[i].transform.position;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player" && !activeEnemy)
        {
            activeEnemy = true;
            EnemyReset();
        }
    }
}
