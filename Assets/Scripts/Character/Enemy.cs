using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.AI;

public class Enemy : Humanoid
{
    public bool isBoss;
    public bool isTutorial;
    public bool isDummy = false;
    public GameObject BossBar;
    public Slider LifeBar;
    private GameObject level_Manager;
    public Material defaultMat;
    public Material highlightMat;
    public float initialHealth;
    protected bool isDead;
    public bool IsDead
    {
        get { return IsDead; }
    }

    protected override void Start()
    {
        isDead = false;

        base.Start();

        if (isBoss)
        {
            BossBar = GameObject.FindGameObjectWithTag("BossBar");
            BossBar.transform.GetChild(0).gameObject.SetActive(true);
            BossBar.transform.GetChild(1).gameObject.SetActive(true);
            LifeBar = BossBar.GetComponentInChildren<Slider>();
            level_Manager = GameObject.FindGameObjectWithTag("LevelManager");
        }
        initialHealth = health;
        if (!isDummy)
        {
            this.GetComponent<ActionSelector>().PreventAttack = false;
            
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
        isDead = true;
        this.GetComponent<ActionSelector>().PreventAttack = true;

        if (isBoss )
        {
            LifeBar.value = 0;
        }
        StartCoroutine(WaitToDestroy());
    }

    IEnumerator WaitToDestroy()
    {
        yield return new WaitForSeconds(4);

        if (isBoss)
        {
            BossBar.SetActive(false);
            level_Manager.GetComponent<LevelManager>().End();
        }
        if (!isTutorial)
        {
            Destroy(gameObject);
        }
        else
        {
            health = 1000;
            gameObject.SetActive(false);
        }
        
    }

    
}
