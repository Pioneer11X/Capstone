using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCharacter : MonoBehaviour
{
    [SerializeField] int health = 100;
    [SerializeField] bool isPlayer;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TakeDamag(int dmg)
    {
        if(dmg > 0)
        {
            health -= dmg;
        }
        if(health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isPlayer)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
