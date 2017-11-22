using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for Player, enemies, and NPCs
/// </summary>
abstract public class Humanoid : MonoBehaviour
{
    // Base set of stats
    [SerializeField] protected float health = 100;
    [SerializeField] protected float energy;
    [SerializeField] protected float baseAttackPower;
    [SerializeField] protected float speedMove;
    [SerializeField] protected float speedDash;
    [SerializeField] protected float speedRun;
    [SerializeField] protected float speedCrouch;
    [SerializeField] protected float jumpPower;
    [SerializeField] protected float stamina;
    [SerializeField] protected float senseRadius;
    
    // Properties for stats
    public float Health
    {
        get { return health; }
        set { health = value; }
    }
    public float Energy
    {
        get { return energy; }
        set { energy = value; }
    }
    public float Stamina
    {
        get { return stamina; }
        set { stamina = value; }
    }
    public float BaseAttackPower
    { get { return baseAttackPower; } }
    public float SpeedMove
    { get { return speedMove; } }
    public float SpeedDash
    { get { return speedDash; } }
    public float SpeedRun
    { get { return speedRun; } }
    public float SpeedCrouch
    { get { return speedCrouch; } }
    public float JumpPower
    { get { return jumpPower; } }
    public float SenseRadius
    { get { return senseRadius; } }

    // Use this for initialization
    virtual protected void Start() { }

    // Update is called once per frame
    virtual protected void Update() { }

    /// <summary>
    /// Deal damage.
    /// </summary>
    /// <param name="dmg">Amount of damage dealt</param>
    public void TakeDamag(int dmg)
    {
        if (dmg > 0)
        {
            health -= dmg;
        }
        if (health <= 0)
        {
            Die();
        }
    }

    abstract protected void Die();

}
