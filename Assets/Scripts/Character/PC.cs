using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Player class, extends from PC base class
/// </summary>
public class PC : Humanoid
{
    [SerializeField] private float minAttackHoldTime;
    [SerializeField] private float maxAttackHoldTime;
    [SerializeField] private float specialRegenRate;
    [SerializeField] private float staminaRegenRate;
    [SerializeField] private float sprintCost;
    [SerializeField] private float dodgeCost;
    [SerializeField] private float lightSwordCost;
    [SerializeField] private float heavySwordCost;
    [SerializeField] private float gunShootCost;


    [SerializeField] protected Slider lifeBar;
    [SerializeField] protected Slider staminaBar;
    [SerializeField] protected Slider specialBar;
    

    private bool specialInUse;
    private bool staminaInUse;

    public float GunShootCost
    {
        get { return gunShootCost; }
    }

    public float LightSwordCost
    {
        get { return lightSwordCost; }
    }
    public float HeavySwordCost
    {
        get { return heavySwordCost; }
    }

    public float MinHoldTime
    {
        get { return minAttackHoldTime; }
    }

    public float MaxHoldTime
    {
        get { return maxAttackHoldTime; }
    }

    public float SprintCost
    {
        get { return sprintCost; }
    }

    public float DodgeCost
    {
        get { return dodgeCost; }
    }

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

        specialInUse = false;
        staminaInUse = false;
    }

    /// <summary>
    /// Update Loop
    /// </summary>
    protected override void Update()
    {
        base.Update();
        if (lifeBar != null && this.tag == "Player")
        {
            lifeBar.value = this.health;
        }
    }


    /// <summary>
    /// Runs with physics
    /// </summary>
    protected void FixedUpdate()
    {
        // Special Regen
        if (specialBar != null && !specialInUse)
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

        // Stamina Regen
        if (staminaBar != null && !staminaInUse)
        {
            if (staminaBar.value < 100)
            {
                staminaBar.value += staminaRegenRate;
            }
            if (staminaBar.value > 100)
            {
                staminaBar.value = 100;
            }
        }

        if(specialInUse) { specialInUse = false; }
        if(staminaInUse) { staminaInUse = false; }
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
    /// <param name="value">How much special to use (Light 24, Heavy 49)</param>
    public void UseSpecial(string attackType)
    {
        int cost = 0;
        if(attackType == "light")
        {
            cost = (int)lightSwordCost;
        }
        else if(attackType == "heavy")
        {
            cost = (int)heavySwordCost;
        }
        else
        {
            Debug.Log("Bad attack type for special use");
        }

        if (specialBar.value > cost)
        {
            specialBar.value -= cost;
            specialInUse = true;
        }

        if (specialBar.value < 0)
        {
            specialBar.value = 0;
        }
    }

    /// <summary>
    /// Subtract from Stamina UI Bar
    /// </summary>
    /// <param name="value">How much stamina to use</param>
    public void UseStamina(float value)
    {
        if (staminaBar.value > (value + 0.01f) )
        {
            staminaBar.value -= value;
            staminaInUse = true;
        }
        if (staminaBar.value < 0)
        {
            staminaBar.value = 0;
        }
    }

    /// <summary>
    /// Do some stuff before exiting to death scene
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitToEnd()
    {
        yield return new WaitForSeconds(4);

        SceneManager.LoadSceneAsync("Death");
    }

    /// <summary>
    /// If player hits a kill plane, kill them
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "KillPlane")
        {
            Die();
        }
    }
}
