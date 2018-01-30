using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : Humanoid
{
    public bool isBoss;
    public bool isTutorial;
    public GameObject BossBar;
    public Slider LifeBar;
    private GameObject level_Manager;

    private int counter;

    protected override void Start()
    {
        base.Start();

        if (isBoss)
        {
            counter = 0;
            BossBar = GameObject.FindGameObjectWithTag("BossBar");
            BossBar.transform.GetChild(0).gameObject.SetActive(true);
            BossBar.transform.GetChild(1).gameObject.SetActive(true);
            LifeBar = BossBar.GetComponentInChildren<Slider>();
            level_Manager = GameObject.FindGameObjectWithTag("LevelManager");
        }        
    }

    protected override void Update()
    {
        base.Update();

        if (isBoss)
        {
            LifeBar.value = this.health;
        }
    }

    /// <summary>
    /// Kill Enemey, remove them from the scene.
    /// </summary>
    protected override void Die()
    {
        if (!isBoss && !isTutorial)
        {
            Destroy(gameObject);
            
        }
        else if (isBoss )
        {
            LifeBar.value = 0;
            StartCoroutine(WaitToDestroy());
        }
        else if(isTutorial)
        {
            health = 1000;
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Assert(false, "Invalid enemy type called Die function");
        }
    }

    IEnumerator WaitToDestroy()
    {
        while (counter < 60)
        {
            counter++;
            yield return null;
        }

        BossBar.SetActive(false);
        level_Manager.GetComponent<LevelManager>().End();
        Destroy(gameObject);
        
    }

    
}
