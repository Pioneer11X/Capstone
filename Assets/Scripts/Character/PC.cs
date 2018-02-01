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
    [SerializeField] protected Slider staminaBar;
    [SerializeField] protected Slider specialBar;

    private int counter;

    public float SpecialBar
    {
        get { return specialBar.value; }
        set { specialBar.value = value; }
    }

    public float StaminaBar
    {
        get { return staminaBar.value; }
        set { staminaBar.value = value; }
    }

    /// <summary>
    /// Initialize
    /// </summary>
    protected override void Start()
    {
        base.Start();

        lifeBar = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<Slider>();
        staminaBar = GameObject.FindGameObjectWithTag("StaminaBar").GetComponent<Slider>();
        specialBar = GameObject.FindGameObjectWithTag("SpecialBar").GetComponent<Slider>();

        counter = 0;

    }

    /// <summary>
    /// Update Loop
    /// </summary>
    protected override void Update()
    {
        base.Update();
        if (lifeBar != null)
        {
            lifeBar.value = this.health;
        }
    }


    /// <summary>
    /// Runs with physics
    /// </summary>
    protected void FixedUpdate()
    {
        // Special Rege
        if (specialBar != null)
        {
            if (specialBar.value < 100)
            {
                specialBar.value += specialRegenRate;
            }
            if (specialBar.value > 100)
            {
                specialBar.value = 100;
            }
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
        else if (specialBar.value > 49.9999f && !light)
        {
            specialBar.value -= value;
        }
        if (specialBar.value < 0)
        {
            specialBar.value = 0;
        }
    }

    /// <summary>
    /// Reduce number of available bullets
    /// </summary>
    public void Shoot()
    {
        if (specialBar.value > 33.9999f)
        {
            specialBar.value -= 33;
        }
        if (specialBar.value < 0)
        {
            specialBar.value = 0;
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

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "KillPlane")
        {
            Die();
        }
    }
}
