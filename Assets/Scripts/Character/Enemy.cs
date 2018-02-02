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
    public Material defaultMat;
    public Material highlightMat;

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
