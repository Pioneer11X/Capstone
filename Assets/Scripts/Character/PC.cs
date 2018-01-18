using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Player class, extends from PC base class
/// </summary>
public class PC : Humanoid
{
    [SerializeField] protected Slider lifeBar;
    [SerializeField] protected Slider visionBar;
    [SerializeField] protected Slider specialBar;
    [SerializeField] protected Slider bulletBar;

    private int counter;

    protected override void Start()
    {
        base.Start();

        counter = 0;

    }

    protected override void Update()
    {
        base.Update();

        lifeBar.value = this.health;
    }

    /// <summary>
    /// Kill player, stop game.
    /// </summary>
    protected override void Die()
    {
        lifeBar.value = 0;
        StartCoroutine(WaitToEnd());
        
    }
  
    /// <summary>
    /// Do some stuff before exiting to death scene
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitToEnd()
    {
        while (counter < 120)
        {
            counter++;
            yield return null;
        }

        SceneManager.LoadSceneAsync("Death");
    }
}
