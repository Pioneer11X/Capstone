using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Player class, extends from PC base class
/// </summary>
public class PC : Humanoid
{
    [SerializeField] private float specialRegenRate;
    [SerializeField] private float bulletRegenRate;
    [SerializeField] protected Slider lifeBar;
    [SerializeField] protected Slider visionBar;
    [SerializeField] protected Slider specialBar;
    [SerializeField] protected Slider bulletBar;

    private int counter;

    public float SpecialBar
    { get { return specialBar.value; } }

    public float BulletBar
    { get { return bulletBar.value; } }

    /// <summary>
    /// Initialize
    /// </summary>
    protected override void Start()
    {
        base.Start();

        counter = 0;

    }

    /// <summary>
    /// Update Loop
    /// </summary>
    protected override void Update()
    {
        base.Update();

        lifeBar.value = this.health;
    }

    protected void FixedUpdate()
    {
        // Special Rege
        if (specialBar.value < 100)
        {
            specialBar.value += specialRegenRate;
        }
        if(specialBar.value > 100)
        {
            specialBar.value = 100;
        }

        // Bullet Regen
        if (bulletBar.value < 600)
        {
            bulletBar.value += bulletRegenRate;
        }
        if (bulletBar.value > 600)
        {
            bulletBar.value = 600;
        }
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
    /// Subtract from Special UI Bar
    /// </summary>
    /// <param name="value">How much special to use (Light 25, Heavy 50)</param>
    /// <param name="light">Light attack?</param>
    public void UseSpecial(int value, bool light)
    {
        if (specialBar.value > 24.9999f && light)
        {
            specialBar.value -= value;
        }
        else if(specialBar.value > 49.9999f && !light)
        {
                specialBar.value -= value;
        }
        if(specialBar.value < 0)
        {
            specialBar.value = 0;
        }
    }

    /// <summary>
    /// Reduce number of available bullets
    /// </summary>
    public void Shoot()
    {
        if(bulletBar.value > 99.9999f)
        {
            bulletBar.value -= 100;
        }
        if (bulletBar.value < 0)
        {
            bulletBar.value = 0;
        }
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
