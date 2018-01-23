using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Combat Class
/// Contains and runs combat. Combos and single attacks.
/// </summary>
public class CombatManager : MonoBehaviour
{
    //*******************************************************************
    //Temporary
    public List<GameObject> enemyList;
    private GameObject[] enemyArray;
    public bool isPlayer;
    //*******************************************************************

    private Character m_char;
    public AudioSource combatAudio;
    public AudioClip punchFX;
    public AudioClip kickFX;
    public AudioClip swordFX;
    public AudioClip gunShotFX;

    //---------------------------------------------------------------------------------------------
    // Special Combat Variables
    public GameObject gun;
    public GameObject sword;
    public GameObject wrist;
    private GameObject companion;
    private Vector3 compPos;
    //---------------------------------------------------------------------------------------------

    public enum HitPosition
    {
        high,
        mid,
        low
    }

    public enum HitDirection
    {
        forward,
        backward,
        left,
        right
    }

    public enum CombatDirection
    {
        forward,
        left,
        right
    }

    public enum HitPower
    {
        weak,
        powerful,
        ko
    }

    public enum Combat
    {
        none,
        punch_Jab_L,
        punch_Jab_R,
        punch_Hook_L,
        punch_Hook_R,
        punch_UpperCut_L,
        punch_UpperCut_R,
        kick_Straight_Mid_R,
        kick_AxeKick,
        kick_HorseKick,
        Sword_Attack_R,
        Sword_Attack_RL,
        Sword_Attack_Sp_U,
        Sword_Attack_Combo_LL,
        KB_Gun
    }

    //target parameters
    [SerializeField]
    private Character currentTarget;
    public Character CurrentTarget
    { get { return currentTarget; } set { currentTarget = value; } }

    [SerializeField]
    private Character aimTarget;
    public Character AimTarget
    { get { return aimTarget; } set { aimTarget = value; } }

    //in combat parameters
    private bool inCombat;
    private float inCombatTimer;
    [SerializeField]
    private float inCombatDuration;
    public bool InCombat
    { get { return inCombat; } }

    //aimming parameters
    private bool isAimming;
    public bool IsAimming
    {
        get { return isAimming;}
        set { isAimming = value;}
    }

    [SerializeField] private float maxShotDistance;
    [SerializeField] private float minShotDistance;

    //move parameters
    private bool isMoving;
    private bool isDashing;
    public bool IsMoving
    { get { return isMoving; }
        set { isMoving = value; }
    }
    public bool IsDashing
    { get { return isDashing; }
      set { isDashing = value; }
    }

    //jump parameters
    public bool isJumping;
    [SerializeField]
    private float jumpUpTime;
    [SerializeField]
    private float jumpAirTime;
    [SerializeField]
    private float jumpDownTime;
    public bool IsJumping
    {
        get { return isJumping; }
        set { isJumping = value; }
    }
    public float JumpUpTime
    { get { return jumpUpTime; } }
    public float JumpAirTime
    { get { return jumpAirTime; } }
    public float JumpDownTime
    { get { return jumpDownTime; } }

    //attack parameters
    private bool isAttacking;
    private bool resetAttack;
    private float attackDuration;
    private float currentAttackTime;
    private float currentEffectTime;
    private float currentEffetDistance;
    private CombatDirection currentDirection;
    private HitPower currentPower;
    public HitPosition currentHitPos;
    public bool IsAttacking
    {
        get { return isAttacking; }
        set { isAttacking = value; }
    }
    public float CurrentAttackTime
    {
        get { return currentAttackTime; }
    }
    public float CurrentEffectTime
    {
        get { return currentEffectTime; }
    }
    public bool ResetAttack
    {
        get { return resetAttack; }
        set { resetAttack = value; }
    }

    //dodge parameters
    private bool isDodging;
    [SerializeField]
    private float dodgeTime;
    private int dodgeDirection;
    public bool IsDodging
    { get { return isDodging; }
      set { isDodging = value; }
    }
    public float DodgeTime
    { get { return dodgeTime; } }
    public int DodgeDirection
    { get { return dodgeDirection; } }


    //roll parameters
    private bool isRolling;
    [SerializeField]
    private float rollTime;
    [SerializeField]
    private float rollSpeed;
    public bool IsRolling
    { get { return isRolling; }
      set { isRolling = value; }
    }
    public float RollTime
    { get { return rollTime; } }
    public float RollSpeed
    { get { return rollSpeed; } }

    //hit parameters
    private bool isHit;
    private bool resetHit;
    [SerializeField]
    private float hitTime;
    [SerializeField]
    private float hitMaxWalkSpeed;
    private int hitAnimationInfo;
    public bool IsHit
    {
        get { return isHit; }
    }
    public float HitTime
    {
        get { return hitTime; }
    }

    //adjust parameters
    private bool isAdjusting;
    [SerializeField]
    private float adjustSpeed;
    [SerializeField]
    private float adjustMinDistance;
    public bool IsAdjusting
    {
        get { return isAdjusting; }
        set { isAdjusting = value; }
    }
    public float AdjustSpeed
    { get { return adjustSpeed; } }

    public float GetAdjustMinDistance()
    {
        return adjustMinDistance;
    }

    [SerializeField]
    private float adjustMaxDistance;

    public float GetAdjustMaxDistance()
    {
        return adjustMaxDistance;
    }

    [SerializeField]
    private float adjustAgle;
    public float GetAdjustAngle()
    {
        return adjustAgle;
    }

    [SerializeField]
    private Combat currentCombat;
    public Combat CurrentCombat
    { get { return currentCombat; } }

    [SerializeField]
    private float comboTimer;
    public float ComboTimer
    {
        get { return comboTimer; }
        set { comboTimer = value; }
    }

    [SerializeField]
    private float maxComboTime;

    // --------------------------------------------------------------------------------------------
    // AT - Attack Time
    // ET - Effect Time
    // ED - Effect Distance
    [SerializeField]
    private float punch_Jab_L_AT;
    [SerializeField]
    private float punch_Jab_L_ET;
    [SerializeField]
    private float punch_Jab_L_ED;
    [SerializeField]
    private HitPosition punch_Jab_L_Pos;
    [SerializeField]
    private CombatDirection punch_Jab_L_Dir;
    [SerializeField]
    private HitPower punch_Jab_L_Power;

    [SerializeField]
    private float punch_Jab_R_AT;
    [SerializeField]
    private float punch_Jab_R_ET;
    [SerializeField]
    private float punch_Jab_R_ED;
    [SerializeField]
    private HitPosition punch_Jab_R_Pos;
    [SerializeField]
    private CombatDirection punch_Jab_R_Dir;
    [SerializeField]
    private HitPower punch_Jab_R_Power;

    [SerializeField]
    private float punch_Hook_L_AT;
    [SerializeField]
    private float punch_Hook_L_ET;
    [SerializeField]
    private float punch_Hook_L_ED;
    [SerializeField]
    private HitPosition punch_Hook_L_Pos;
    [SerializeField]
    private CombatDirection punch_Hook_L_Dir;
    [SerializeField]
    private HitPower punch_Hook_L_Power;

    [SerializeField]
    private float punch_Hook_R_AT;
    [SerializeField]
    private float punch_Hook_R_ET;
    [SerializeField]
    private float punch_Hook_R_ED;
    [SerializeField]
    private HitPosition punch_Hook_R_Pos;
    [SerializeField]
    private CombatDirection punch_Hook_R_Dir;
    [SerializeField]
    private HitPower punch_Hook_R_Power;

    [SerializeField]
    private float punch_UpperCut_L_AT;
    [SerializeField]
    private float punch_UpperCut_L_ET;
    [SerializeField]
    private float punch_UpperCut_L_ED;
    [SerializeField]
    private HitPosition punch_UpperCut_L_Pos;
    [SerializeField]
    private CombatDirection punch_UpperCut_L_Dir;
    [SerializeField]
    private HitPower punch_UpperCut_L_Power;

    [SerializeField]
    private float punch_UpperCut_R_AT;
    [SerializeField]
    private float punch_UpperCut_R_ET;
    [SerializeField]
    private float punch_UpperCut_R_ED;
    [SerializeField]
    private HitPosition punch_UpperCut_R_Pos;
    [SerializeField]
    private CombatDirection punch_UpperCut_R_Dir;
    [SerializeField]
    private HitPower punch_UpperCut_R_Power;

    [SerializeField]
    private float kick_Straight_Mid_R_AT;
    [SerializeField]
    private float kick_Straight_Mid_R_ET;
    [SerializeField]
    private float kick_Straight_Mid_R_ED;
    [SerializeField]
    private HitPosition kick_Straight_Mid_R_Pos;
    [SerializeField]
    private CombatDirection kick_Straight_Mid_R_Dir;
    [SerializeField]
    private HitPower kick_Straight_Mid_R_Power;

    [SerializeField]
    private float kick_AxeKick_AT;
    [SerializeField]
    private float kick_AxeKick_ET;
    [SerializeField]
    private float kick_AxeKick_ED;
    [SerializeField]
    private HitPosition kick_AxeKick_Pos;
    [SerializeField]
    private CombatDirection kick_AxeKick_Dir;
    [SerializeField]
    private HitPower kick_AxeKick_Power;

    [SerializeField]
    private float kick_HorseKick_AT;
    [SerializeField]
    private float kick_HorseKick_ET;
    [SerializeField]
    private float kick_HorseKick_ED;
    [SerializeField]
    private HitPosition kick_HorseKick_Pos;
    [SerializeField]
    private CombatDirection kick_HorseKick_Dir;
    [SerializeField]
    private HitPower kick_HorseKick_Power;

    [SerializeField]
    private float sword_Attack_R_AT;
    [SerializeField]
    private float sword_Attack_R_ET;
    [SerializeField]
    private float sword_Attack_R_ED;
    [SerializeField]
    private HitPosition sword_Attack_R_Pos;
    [SerializeField]
    private CombatDirection sword_Attack_R_Dir;
    [SerializeField]
    private HitPower sword_Attack_R_Power;

    [SerializeField]
    private float sword_Attack_RL_AT;
    [SerializeField]
    private float sword_Attack_RL_ET;
    [SerializeField]
    private float sword_Attack_RL_ED;
    [SerializeField]
    private HitPosition sword_Attack_RL_Pos;
    [SerializeField]
    private CombatDirection sword_Attack_RL_Dir;
    [SerializeField]
    private HitPower sword_Attack_RL_Power;

    [SerializeField]
    private float sword_Attack_Sp_U_AT;
    [SerializeField]
    private float sword_Attack_Sp_U_ET;
    [SerializeField]
    private float sword_Attack_Sp_U_ED;
    [SerializeField]
    private HitPosition sword_Attack_Sp_U_Pos;
    [SerializeField]
    private CombatDirection sword_Attack_Sp_U_Dir;
    [SerializeField]
    private HitPower sword_Attack_Sp_U_Power;

    [SerializeField]
    private float Sword_Attack_Combo_LL_AT;
    [SerializeField]
    private float Sword_Attack_Combo_LL_ET;
    [SerializeField]
    private float Sword_Attack_Combo_LL_ED;
    [SerializeField]
    private HitPosition Sword_Attack_Combo_LL_Pos;
    [SerializeField]
    private CombatDirection Sword_Attack_Combo_LL_Dir;
    [SerializeField]
    private HitPower Sword_Attack_Combo_LL_Power;

    [SerializeField]
    private float KB_Gun_AT;
    [SerializeField]
    private float KB_Gun_ET;
    [SerializeField]
    private float KB_Gun_ED;
    [SerializeField]
    private HitPosition KB_Gun_Pos;
    [SerializeField]
    private CombatDirection KB_Gun_Dir;
    [SerializeField]
    private HitPower KB_Gun_Power;
    // --------------------------------------------------------------------------------------------

    //
    #region Bools
    private bool swordGunAttack;

    public bool canMove
    {
        get
        {
            return !(isRolling || isHit || isAttacking || isDodging || isAdjusting);
        }
    }

    public bool canJump
    {
        get
        {
            return !(isRolling || isJumping || isAimming || isAttacking || isDodging || isAdjusting);
        }
    }

    public bool canAttack
    {
        get
        {
            return !(isRolling || isJumping || isAttacking || isDodging || isAdjusting);
        }
    }

    public bool canSetNextAttack
    {
        get
        {
            return isAttacking;
        }
    }

    public bool canDodge
    {
        get
        {
            return !(isRolling || isJumping || isAttacking || isDodging || isAdjusting);
        }
    }

    public bool canRoll
    {
        get
        {
            return !(isRolling || isJumping || isAimming || isAttacking || isDodging || isAdjusting);
        }
    }

    private bool canAim;

    private bool canShoot;
    #endregion


    // Use this for initialization
    void Start ()
    {
        inCombatTimer = 0;

        if (isPlayer)
        {
            swordGunAttack = false;
            //sword = GameObject.FindGameObjectWithTag("Sword");
            sword.SetActive(false);
            gun.SetActive(false);

            companion = GameObject.FindGameObjectWithTag("Companion");

            //*******************************************************************
            //temp code
            enemyList = new List<GameObject>();
            enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
            for (int i = 0; i < enemyArray.Length; i++)
            {
                enemyList.Add(enemyArray[i]);
            }
            //*******************************************************************
        }
    }

    // Update is called once per frame
    void Update ()
    {
        //*******************************************************************
        if (isPlayer)
        {
            if(attackDuration > 0)
            {
                attackDuration -= Time.deltaTime;
            }
            if(attackDuration <= 0 && swordGunAttack)
            {
                swordGunAttack = false;
                sword.SetActive(false);
                //gun.SetActive(false);
                companion.SetActive(true);
                companion.transform.position = compPos;
            }

            float dist = float.MaxValue;
            foreach (GameObject obj in enemyList)
            {
                
                if (obj != null)
                {
                    float distTo = (transform.position - obj.transform.position).magnitude;
                    if (distTo < dist)
                    {
                        dist = distTo;
                        currentTarget = obj.GetComponentInChildren<Character>();
                    }
                }
            }

            if(isAimming && companion.activeSelf)
            {
                compPos = companion.transform.position;
                companion.SetActive(false);
            }
            else if(!isAimming && !companion.activeSelf)
            {
                companion.SetActive(true);
            }
        }
        //*******************************************************************
    }

    public void SetChar(Character _char)
    {
        m_char = _char;
    }

    public bool UpdateState(float stateTimer)
    {
        int animationParameter = 0;

        if (inCombatTimer > 0)
        {
            inCombatTimer -= Time.deltaTime;
            if (inCombatTimer <= 0)
            {
                inCombat = false;
            }
        }

        if (comboTimer >= 0 && !(isAttacking || isAdjusting))
        {
            comboTimer += Time.deltaTime;
            if (comboTimer > maxComboTime)
            {
                comboTimer = -1;
                currentCombat = Combat.none;
            }
        }

        //is in combat
        if (isHit)
        {
            if (stateTimer < hitTime)
            {
                m_char.CurrentState = Character.CharacterState.hit;
                animationParameter = hitAnimationInfo;
                int power = hitAnimationInfo % 10;
                if ((HitPower)power == HitPower.powerful)
                {
                    int hitDirection = (hitAnimationInfo / 10) % 10;
                    if (hitDirection >= 2)
                    {
                        m_char.ForceMove(hitMaxWalkSpeed * 0.5f, hitDirection);
                    }
                    else
                    {
                        m_char.ForceMove(hitMaxWalkSpeed, hitDirection);
                    }
                }
                else if ((HitPower)power == HitPower.ko)
                {

                }
                if (resetHit)
                {
                    m_char.LastState = Character.CharacterState.none;
                    resetHit = false;
                }
                m_char.AnimationParameter = animationParameter;
            }
            else
            {
                isHit = false;
                stateTimer = -1;

                isAttacking = false;
                m_char.HasEffect = false;
                comboTimer = 0;
            }

            return true;
        }

        return false;
    }


    public void Hit(HitPosition pos, HitDirection dir, HitPower power)
    {
        //
        isHit = true;
        m_char.StateTimer = 0;
        inCombat = true;
        inCombatTimer = inCombatDuration;
        hitAnimationInfo = (int)pos * 100 + (int)dir * 10 + (int)power;
        resetHit = true;

        // TODO
        // Needs to be redone for how much damage based on what attack
        this.GetComponent<Humanoid>().TakeDamag(10);
        //

        isAttacking = false;
        isRolling = false;
        isAdjusting = false;
        isJumping = false;
        isMoving = false;

    }

    // Basic Attack Combos
    public void BasicCombo()
    {
        //if can attack
        //if with in combat timer
        //NextCombat()
        if (!canAttack)
        {
            return;
        }
        NextCombat();
        if (CheckTarget())
        {
            Attack();
        }
        else
        {
            Adjust();
        }
    }

    public void SpecialCombat()
    {
        if (!canAttack)
        {
            return;
        }
        NextSpecial();
        if (CheckTarget())
        {
            Attack();
        }
        else
        {
            Adjust();
        }

    }

    // Gun Shooting
    public void GunShot()
    {
        if (!canAttack || !isAimming)
        {
            return;
        }

        if (CheckRangeTarget())
        {
            swordGunAttack = true;
            gun.SetActive(true);
            combatAudio.clip = gunShotFX;
            combatAudio.PlayDelayed(0.5f);
            currentCombat = Combat.KB_Gun;
            currentAttackTime = KB_Gun_AT;
            currentEffectTime = KB_Gun_ET;
            currentEffetDistance = KB_Gun_ED;
            currentDirection = KB_Gun_Dir;
            currentPower = KB_Gun_Power;
            currentHitPos = KB_Gun_Pos;
            attackDuration = currentAttackTime;
            Shoot();
        }
    }

    // Sword Attack Combos
    public void SwordCombo()
    {
        //if can attack
        //if with in combat timer
        //NextCombat()
        if (!canAttack)
        {
            return;
        }
        NextSwordCombat();
        if (CheckTarget())
        {
            Attack();
            m_char.GetComponent<PC>().UseSpecial(25, true);
        }
        else
        {
            Adjust();
        }
    }

    public void SwordSpecialCombat()
    {
        if (!canAttack)
        {
            return;
        }
        NextSwordSpecial();
        if (CheckTarget())
        {
            Attack();
            m_char.GetComponent<PC>().UseSpecial(50, true);
        }
        else
        {
            Adjust();
        }

    }

    /// <summary>
    /// Melee Attacks
    /// </summary>
    public void Attack()
    {
        inCombat = true;
        inCombatTimer = inCombatDuration;
        isAttacking = true;
        m_char.StateTimer = 0;
        resetAttack = true;
    }

    /// <summary>
    /// Ranged Attack
    /// </summary>
    public void Shoot()
    {
        inCombat = true;
        inCombatTimer = inCombatDuration;
        isAttacking = true;
        m_char.StateTimer = 0;
        resetAttack = true;

        // Update UI
        m_char.GetComponent<PC>().Shoot();
    }

    public void Effect()
    {

        if (currentTarget != null)
        {

            float distance = Vector3.Distance(m_char.charBody.transform.position, currentTarget.charBody.transform.position);
            HitDirection dir = HitDirection.forward;

            if (distance <= currentEffetDistance)
            {
                float angleFB = Vector3.Angle(currentTarget.charBody.transform.position - m_char.charBody.transform.position, currentTarget.charBody.transform.forward);
                float angleLR = Vector3.Angle(currentTarget.charBody.transform.position - m_char.charBody.transform.position, currentTarget.charBody.transform.right);
                if (angleFB <= 45)
                {
                    dir = HitDirection.backward;
                }
                else if (angleFB >= 135)
                {
                    dir = HitDirection.forward;
                }
                else
                {
                    if (angleLR <= 45)
                    {
                        dir = HitDirection.left;
                    }
                    else if (angleLR >= 135)
                    {
                        dir = HitDirection.right;
                    }
                }
                if (currentDirection == CombatDirection.right)
                {
                    if (dir == HitDirection.forward)
                    {
                        dir = HitDirection.left;
                    }
                    else if (dir == HitDirection.backward)
                    {
                        dir = HitDirection.right;
                    }
                    else if (dir == HitDirection.left)
                    {
                        dir = HitDirection.backward;
                    }
                    else if (dir == HitDirection.right)
                    {
                        dir = HitDirection.forward;
                    }
                }
                else if (currentDirection == CombatDirection.left)
                {
                    if (dir == HitDirection.forward)
                    {
                        dir = HitDirection.right;
                    }
                    else if (dir == HitDirection.backward)
                    {
                        dir = HitDirection.left;
                    }
                    else if (dir == HitDirection.left)
                    {
                        dir = HitDirection.forward;
                    }
                    else if (dir == HitDirection.right)
                    {
                        dir = HitDirection.backward;
                    }
                }

                currentTarget.m_combat.Hit(currentTarget.m_combat.currentHitPos, dir, currentPower);
            }

        }
    }

    public bool CheckTarget()
    {
        if (currentTarget == null)
        {
            return true;
        }
        else
        {
            float distance = Vector3.Distance(m_char.charBody.transform.position, currentTarget.charBody.transform.position);
            float angle = Vector3.Angle(currentTarget.charBody.transform.position - m_char.charBody.transform.position, m_char.charBody.transform.forward);

            if (distance < adjustMinDistance && angle < adjustAgle)
            {
                return true;
            }
            else if (distance > adjustMaxDistance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }

    /// <summary>
    /// Check to see if the player has a valid ranged target
    /// </summary>
    /// <returns>If player should shoot</returns>
    public bool CheckRangeTarget()
    {
        if (aimTarget == null)
        {
            return false;
        }
        else
        {
            float distance = Vector3.Distance(m_char.charBody.transform.position, aimTarget.charBody.transform.position);
            //float angle = Vector3.Angle(currentTarget.charBody.transform.position - m_char.charBody.transform.position, m_char.charBody.transform.forward);

            if (distance < minShotDistance)
            {
                return false;
            }
            else if (distance > maxShotDistance)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    void Adjust()
    {
        isAdjusting = true;
    }

    public void Dodge(int dir)
    {
        if (!canDodge)
        {
            return;
        }
        dodgeDirection = dir;
        m_char.StateTimer = 0;
        isDodging = true;
    }

    public void Roll()
    {
        if (!canRoll)
        {
            return;
        }
        m_char.StateTimer = 0;
        isRolling = true;
    }

    void NextCombat()
    {
        if (currentCombat == Combat.none)
        {
            combatAudio.PlayOneShot(punchFX);
            currentCombat = Combat.punch_Jab_L;
            currentAttackTime = punch_Jab_L_AT;
            currentEffectTime = punch_Jab_L_ET;
            currentEffetDistance = punch_Jab_L_ED;
            currentDirection = punch_Jab_L_Dir;
            currentPower = punch_Jab_L_Power;
            currentHitPos = punch_Jab_L_Pos;
        }
        else if (currentCombat == Combat.punch_Jab_L)
        {
            combatAudio.PlayOneShot(punchFX);
            currentCombat = Combat.punch_Jab_R;
            currentAttackTime = punch_Jab_R_AT;
            currentEffectTime = punch_Jab_R_ET;
            currentEffetDistance = punch_Jab_R_ED;
            currentDirection = punch_Jab_R_Dir;
            currentPower = punch_Jab_R_Power;
            currentHitPos = punch_Jab_R_Pos;
        }
        else if (currentCombat == Combat.punch_Jab_R)
        {
            combatAudio.PlayOneShot(punchFX);
            currentCombat = Combat.punch_Hook_L;
            currentAttackTime = punch_Hook_L_AT;
            currentEffectTime = punch_Hook_L_ET;
            currentEffetDistance = punch_Hook_L_ED;
            currentDirection = punch_Hook_L_Dir;
            currentPower = punch_Hook_L_Power;
            currentHitPos = punch_Hook_L_Pos;
        }
        else if (currentCombat == Combat.punch_Hook_L)
        {
            combatAudio.PlayOneShot(punchFX);
            currentCombat = Combat.punch_Hook_R;
            currentAttackTime = punch_Hook_R_AT;
            currentEffectTime = punch_Hook_R_ET;
            currentEffetDistance = punch_Hook_R_ED;
            currentDirection = punch_Hook_R_Dir;
            currentPower = punch_Hook_R_Power;
            currentHitPos = punch_Hook_R_Pos;
        }
        else if (currentCombat == Combat.punch_Hook_R)
        {
            combatAudio.PlayOneShot(punchFX);
            currentCombat = Combat.kick_Straight_Mid_R;
            currentAttackTime = kick_Straight_Mid_R_AT;
            currentEffectTime = kick_Straight_Mid_R_ET;
            currentEffetDistance = kick_Straight_Mid_R_ED;
            currentDirection = kick_Straight_Mid_R_Dir;
            currentPower = kick_Straight_Mid_R_Power;
            currentHitPos = kick_Straight_Mid_R_Pos;
        }
        else if (currentCombat == Combat.kick_Straight_Mid_R)
        {
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.punch_Jab_L;
            currentAttackTime = punch_Jab_L_AT;
            currentEffectTime = punch_Jab_L_ET;
            currentEffetDistance = punch_Jab_L_ED;
            currentDirection = punch_Jab_L_Dir;
            currentPower = punch_Jab_L_Power;
            currentHitPos = punch_Jab_L_Pos;
        }
        else
        {
            combatAudio.PlayOneShot(punchFX);
            currentCombat = Combat.punch_Jab_L;
            currentAttackTime = punch_Jab_L_AT;
            currentEffectTime = punch_Jab_L_ET;
            currentEffetDistance = punch_Jab_L_ED;
            currentDirection = punch_Jab_L_Dir;
            currentPower = punch_Jab_L_Power;
            currentHitPos = punch_Jab_L_Pos;
        }

    }

    void NextSpecial()
    {
        if (currentCombat == Combat.none)
        {
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.kick_AxeKick;
            currentAttackTime = kick_AxeKick_AT;
            currentEffectTime = kick_AxeKick_ET;
            currentEffetDistance = kick_AxeKick_ED;
            currentDirection = kick_AxeKick_Dir;
            currentPower = kick_AxeKick_Power;
            currentHitPos = kick_AxeKick_Pos;
        }
        else if (currentCombat == Combat.punch_Jab_L)
        {
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.kick_AxeKick;
            currentAttackTime = kick_AxeKick_AT;
            currentEffectTime = kick_AxeKick_ET;
            currentEffetDistance = kick_AxeKick_ED;
            currentDirection = kick_AxeKick_Dir;
            currentPower = kick_AxeKick_Power;
            currentHitPos = kick_AxeKick_Pos;
        }
        else if (currentCombat == Combat.punch_Jab_R)
        {
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.kick_AxeKick;
            currentAttackTime = kick_AxeKick_AT;
            currentEffectTime = kick_AxeKick_ET;
            currentEffetDistance = kick_AxeKick_ED;
            currentDirection = kick_AxeKick_Dir;
            currentPower = kick_AxeKick_Power;
            currentHitPos = kick_AxeKick_Pos;
        }
        else if (currentCombat == Combat.punch_Hook_L)
        {
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.kick_AxeKick;
            currentAttackTime = kick_AxeKick_AT;
            currentEffectTime = kick_AxeKick_ET;
            currentEffetDistance = kick_AxeKick_ED;
            currentDirection = kick_AxeKick_Dir;
            currentPower = kick_AxeKick_Power;
            currentHitPos = kick_AxeKick_Pos;
        }
        else if (currentCombat == Combat.punch_Hook_R)
        {
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.kick_AxeKick;
            currentAttackTime = kick_AxeKick_AT;
            currentEffectTime = kick_AxeKick_ET;
            currentEffetDistance = kick_AxeKick_ED;
            currentDirection = kick_AxeKick_Dir;
            currentPower = kick_AxeKick_Power;
            currentHitPos = kick_AxeKick_Pos;
        }
        else if (currentCombat == Combat.kick_Straight_Mid_R)
        {
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.kick_HorseKick;
            currentAttackTime = kick_HorseKick_AT;
            currentEffectTime = kick_HorseKick_ET;
            currentEffetDistance = kick_HorseKick_ED;
            currentDirection = kick_HorseKick_Dir;
            currentPower = kick_HorseKick_Power;
            currentHitPos = kick_AxeKick_Pos;
        }
        else
        {
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.kick_AxeKick;
            currentAttackTime = kick_AxeKick_AT;
            currentEffectTime = kick_AxeKick_ET;
            currentEffetDistance = kick_AxeKick_ED;
            currentDirection = kick_AxeKick_Dir;
            currentPower = kick_AxeKick_Power;
            currentHitPos = kick_AxeKick_Pos;
        }
    }

    // --------------------------------------------------------------------------------------------
    // Sword Attack
    void NextSwordCombat()
    {
        if (currentCombat == Combat.none)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_R;
            currentAttackTime = sword_Attack_R_AT;
            currentEffectTime = sword_Attack_R_ET;
            currentEffetDistance = sword_Attack_R_ED;
            currentDirection = sword_Attack_R_Dir;
            currentPower = sword_Attack_R_Power;
            currentHitPos = sword_Attack_R_Pos;
        }
        else if (currentCombat == Combat.Sword_Attack_R)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_RL;
            currentAttackTime = sword_Attack_RL_AT;
            currentEffectTime = sword_Attack_RL_ET;
            currentEffetDistance = sword_Attack_RL_ED;
            currentDirection = sword_Attack_RL_Dir;
            currentPower = sword_Attack_RL_Power;
            currentHitPos = sword_Attack_RL_Pos;
        }
        else
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_R;
            currentAttackTime = sword_Attack_R_AT;
            currentEffectTime = sword_Attack_R_ET;
            currentEffetDistance = sword_Attack_R_ED;
            currentDirection = sword_Attack_R_Dir;
            currentPower = sword_Attack_R_Power;
            currentHitPos = sword_Attack_R_Pos;
        }
        sword.SetActive(true);
        compPos = companion.transform.position;
        companion.SetActive(false);
        swordGunAttack = true;
        attackDuration = currentAttackTime;
    }

    void NextSwordSpecial()
    {
        if (currentCombat == Combat.none)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_Combo_LL;
            currentAttackTime = Sword_Attack_Combo_LL_AT;
            currentEffectTime = Sword_Attack_Combo_LL_ET;
            currentEffetDistance = Sword_Attack_Combo_LL_ED;
            currentDirection = Sword_Attack_Combo_LL_Dir;
            currentPower = Sword_Attack_Combo_LL_Power;
            currentHitPos = Sword_Attack_Combo_LL_Pos;
        }
        else if (currentCombat == Combat.punch_Jab_L)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_Sp_U;
            currentAttackTime = sword_Attack_Sp_U_AT;
            currentEffectTime = sword_Attack_Sp_U_ET;
            currentEffetDistance = sword_Attack_Sp_U_ED;
            currentDirection = sword_Attack_Sp_U_Dir;
            currentPower = sword_Attack_Sp_U_Power;
            currentHitPos = sword_Attack_Sp_U_Pos;
        }
        else if (currentCombat == Combat.punch_Jab_R)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_Sp_U;
            currentAttackTime = sword_Attack_Sp_U_AT;
            currentEffectTime = sword_Attack_Sp_U_ET;
            currentEffetDistance = sword_Attack_Sp_U_ED;
            currentDirection = sword_Attack_Sp_U_Dir;
            currentPower = sword_Attack_Sp_U_Power;
            currentHitPos = sword_Attack_Sp_U_Pos;
        }
        else if (currentCombat == Combat.punch_Hook_L)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_Sp_U;
            currentAttackTime = sword_Attack_Sp_U_AT;
            currentEffectTime = sword_Attack_Sp_U_ET;
            currentEffetDistance = sword_Attack_Sp_U_ED;
            currentDirection = sword_Attack_Sp_U_Dir;
            currentPower = sword_Attack_Sp_U_Power;
            currentHitPos = sword_Attack_Sp_U_Pos;
        }
        else if (currentCombat == Combat.punch_Hook_R)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_Sp_U;
            currentAttackTime = sword_Attack_Sp_U_AT;
            currentEffectTime = sword_Attack_Sp_U_ET;
            currentEffetDistance = sword_Attack_Sp_U_ED;
            currentDirection = sword_Attack_Sp_U_Dir;
            currentPower = sword_Attack_Sp_U_Power;
            currentHitPos = sword_Attack_Sp_U_Pos;
        }
        else if (currentCombat == Combat.kick_Straight_Mid_R)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_Sp_U;
            currentAttackTime = sword_Attack_Sp_U_AT;
            currentEffectTime = sword_Attack_Sp_U_ET;
            currentEffetDistance = sword_Attack_Sp_U_ED;
            currentDirection = sword_Attack_Sp_U_Dir;
            currentPower = sword_Attack_Sp_U_Power;
            currentHitPos = sword_Attack_Sp_U_Pos;
        }
        else if (currentCombat == Combat.Sword_Attack_RL)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_Sp_U;
            currentAttackTime = sword_Attack_Sp_U_AT;
            currentEffectTime = sword_Attack_Sp_U_ET;
            currentEffetDistance = sword_Attack_Sp_U_ED;
            currentDirection = sword_Attack_Sp_U_Dir;
            currentPower = sword_Attack_Sp_U_Power;
            currentHitPos = sword_Attack_Sp_U_Pos;
        }
        else
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_Combo_LL;
            currentAttackTime = Sword_Attack_Combo_LL_AT;
            currentEffectTime = Sword_Attack_Combo_LL_ET;
            currentEffetDistance = Sword_Attack_Combo_LL_ED;
            currentDirection = Sword_Attack_Combo_LL_Dir;
            currentPower = Sword_Attack_Combo_LL_Power;
            currentHitPos = Sword_Attack_Combo_LL_Pos;
        }
        sword.SetActive(true);
        compPos = companion.transform.position;
        companion.SetActive(false);
        swordGunAttack = true;
        attackDuration = currentAttackTime;
    }
    // --------------------------------------------------------------------------------------------

}// End of Combat Manager
