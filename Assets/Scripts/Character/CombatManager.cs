using System.Collections;
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
    public GameObject companion;
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
        Gun_Shoot,
        Number_Of_Items
    }

    // A New Struct to store the details of each Combat move.
    // దీనికి ఒక కొత్త Struct తయారుచెయ్యాలి.
    // దీనిలో ఒక Combat Move కి సంబంధించిన వివరాలు వుంటాయి. 

    [System.Serializable]
    public struct CombatMoveDetails
    {
        // --------------------------------------------------------------------------------------------
        // AT - Attack Time
        // ET - Effect Time
        // ED - Effect Distance
        // Dmg = Damage Amount

        // పేరు ఉత్తినే పెడతాము మనం గుర్తుపదతానికి.
        // Name doesn't matter but is good for recognising.
        public string name;
        public float AT;
        public float ET;
        public float ED;
        public float Dmg;
        public HitPosition Pos;
        public CombatDirection Dir;
        public HitPower Power;
        
    }

    // ఆటలో వున్నా అన్ని Actions కి ఇది ఒక నిఘంటువు. ఇందిలో వున్నా Actions మాత్రమే వాడాలి.
    // ఈ నిఘంటువుని Serialize చెయ్యలేక మనం ఒక కొత్త లిస్టు తయరు చేసుకొంటున్నాము.

    // A List with all the Actions and their parameters. ( Dictionary has to made serializable manually by writing a new class and stuff ).
    // The index is the enum value of the combat. (The evaluated integer)  
    [SerializeField]
    // public List<CombatMoveDetails> allMoves = new List<CombatMoveDetails>((int)Combat.Number_Of_Items);
    public List<CombatMoveDetails> allMoves;


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

    public int moveDir;
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
    private float currentDmgAmount;
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
    // Dmg = Damage Amount

    // ఇవి మనకింక అవసరం లేదు.
    // We do not need these anymore.
    //[SerializeField]
    //private float punch_Jab_L_AT;
    //[SerializeField]
    //private float punch_Jab_L_ET;
    //[SerializeField]
    //private float punch_Jab_L_ED;
    //[SerializeField]
    //private HitPosition punch_Jab_L_Pos;
    //[SerializeField]
    //private CombatDirection punch_Jab_L_Dir;
    //[SerializeField]
    //private HitPower punch_Jab_L_Power;
    //[SerializeField]
    //private float punch_Jab_L_Dmg;

    //[SerializeField]
    //private float punch_Jab_R_AT;
    //[SerializeField]
    //private float punch_Jab_R_ET;
    //[SerializeField]
    //private float punch_Jab_R_ED;
    //[SerializeField]
    //private HitPosition punch_Jab_R_Pos;
    //[SerializeField]
    //private CombatDirection punch_Jab_R_Dir;
    //[SerializeField]
    //private HitPower punch_Jab_R_Power;
    //[SerializeField]
    //private float punch_Jab_R_Dmg;

    //[SerializeField]
    //private float punch_Hook_L_AT;
    //[SerializeField]
    //private float punch_Hook_L_ET;
    //[SerializeField]
    //private float punch_Hook_L_ED;
    //[SerializeField]
    //private HitPosition punch_Hook_L_Pos;
    //[SerializeField]
    //private CombatDirection punch_Hook_L_Dir;
    //[SerializeField]
    //private HitPower punch_Hook_L_Power;
    //[SerializeField]
    //private float punch_Hook_L_Dmg;

    //[SerializeField]
    //private float punch_Hook_R_AT;
    //[SerializeField]
    //private float punch_Hook_R_ET;
    //[SerializeField]
    //private float punch_Hook_R_ED;
    //[SerializeField]
    //private HitPosition punch_Hook_R_Pos;
    //[SerializeField]
    //private CombatDirection punch_Hook_R_Dir;
    //[SerializeField]
    //private HitPower punch_Hook_R_Power;
    //[SerializeField]
    //private float punch_Hook_R_Dmg;


    //[SerializeField]
    //private float punch_UpperCut_L_AT;
    //[SerializeField]
    //private float punch_UpperCut_L_ET;
    //[SerializeField]
    //private float punch_UpperCut_L_ED;
    //[SerializeField]
    //private HitPosition punch_UpperCut_L_Pos;
    //[SerializeField]
    //private CombatDirection punch_UpperCut_L_Dir;
    //[SerializeField]
    //private HitPower punch_UpperCut_L_Power;
    //[SerializeField]
    //private float punch_UpperCut_L_Dmg;

    //[SerializeField]
    //private float punch_UpperCut_R_AT;
    //[SerializeField]
    //private float punch_UpperCut_R_ET;
    //[SerializeField]
    //private float punch_UpperCut_R_ED;
    //[SerializeField]
    //private HitPosition punch_UpperCut_R_Pos;
    //[SerializeField]
    //private CombatDirection punch_UpperCut_R_Dir;
    //[SerializeField]
    //private HitPower punch_UpperCut_R_Power;
    //[SerializeField]
    //private float punch_UpperCut_R_Dmg;

    //[SerializeField]
    //private float kick_Straight_Mid_R_AT;
    //[SerializeField]
    //private float kick_Straight_Mid_R_ET;
    //[SerializeField]
    //private float kick_Straight_Mid_R_ED;
    //[SerializeField]
    //private HitPosition kick_Straight_Mid_R_Pos;
    //[SerializeField]
    //private CombatDirection kick_Straight_Mid_R_Dir;
    //[SerializeField]
    //private HitPower kick_Straight_Mid_R_Power;
    //[SerializeField]
    //private float kick_Straight_Mid_R_Dmg;

    //[SerializeField]
    //private float kick_AxeKick_AT;
    //[SerializeField]
    //private float kick_AxeKick_ET;
    //[SerializeField]
    //private float kick_AxeKick_ED;
    //[SerializeField]
    //private HitPosition kick_AxeKick_Pos;
    //[SerializeField]
    //private CombatDirection kick_AxeKick_Dir;
    //[SerializeField]
    //private HitPower kick_AxeKick_Power;
    //[SerializeField]
    //private float kick_AxeKick_Dmg;

    //[SerializeField]
    //private float kick_HorseKick_AT;
    //[SerializeField]
    //private float kick_HorseKick_ET;
    //[SerializeField]
    //private float kick_HorseKick_ED;
    //[SerializeField]
    //private HitPosition kick_HorseKick_Pos;
    //[SerializeField]
    //private CombatDirection kick_HorseKick_Dir;
    //[SerializeField]
    //private HitPower kick_HorseKick_Power;
    //[SerializeField]
    //private float kick_HorseKick_Dmg;

    //[SerializeField]
    //private float sword_Attack_R_AT;
    //[SerializeField]
    //private float sword_Attack_R_ET;
    //[SerializeField]
    //private float sword_Attack_R_ED;
    //[SerializeField]
    //private HitPosition sword_Attack_R_Pos;
    //[SerializeField]
    //private CombatDirection sword_Attack_R_Dir;
    //[SerializeField]
    //private HitPower sword_Attack_R_Power;
    //[SerializeField]
    //private float sword_Attack_R_Dmg;

    //[SerializeField]
    //private float sword_Attack_RL_AT;
    //[SerializeField]
    //private float sword_Attack_RL_ET;
    //[SerializeField]
    //private float sword_Attack_RL_ED;
    //[SerializeField]
    //private HitPosition sword_Attack_RL_Pos;
    //[SerializeField]
    //private CombatDirection sword_Attack_RL_Dir;
    //[SerializeField]
    //private HitPower sword_Attack_RL_Power;
    //[SerializeField]
    //private float sword_Attack_RL_Dmg;

    //[SerializeField]
    //private float sword_Attack_Sp_U_AT;
    //[SerializeField]
    //private float sword_Attack_Sp_U_ET;
    //[SerializeField]
    //private float sword_Attack_Sp_U_ED;
    //[SerializeField]
    //private HitPosition sword_Attack_Sp_U_Pos;
    //[SerializeField]
    //private CombatDirection sword_Attack_Sp_U_Dir;
    //[SerializeField]
    //private HitPower sword_Attack_Sp_U_Power;
    //[SerializeField]
    //private float sword_Attack_Sp_U_Dmg;

    //[SerializeField]
    //private float Sword_Attack_Combo_LL_AT;
    //[SerializeField]
    //private float Sword_Attack_Combo_LL_ET;
    //[SerializeField]
    //private float Sword_Attack_Combo_LL_ED;
    //[SerializeField]
    //private HitPosition Sword_Attack_Combo_LL_Pos;
    //[SerializeField]
    //private CombatDirection Sword_Attack_Combo_LL_Dir;
    //[SerializeField]
    //private HitPower Sword_Attack_Combo_LL_Power;
    //[SerializeField]
    //private float Sword_Attack_Combo_LL_Dmg;

    //[SerializeField]
    //private float KB_Gun_AT;
    //[SerializeField]
    //private float KB_Gun_ET;
    //[SerializeField]
    //private float KB_Gun_ED;
    //[SerializeField]
    //private HitPosition KB_Gun_Pos;
    //[SerializeField]
    //private CombatDirection KB_Gun_Dir;
    //[SerializeField]
    //private HitPower KB_Gun_Power;
    //[SerializeField]
    //private float KB_Gun_Dmg;
    //// --------------------------------------------------------------------------------------------

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

    private int listCounter;
    // Use this for initialization
    void Start ()
    {
        inCombatTimer = 0;

        if (isPlayer)
        {
            listCounter = 0;
            moveDir = -1;
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
            listCounter++;
            if(listCounter > 240)
            {
                enemyList.Clear();
                enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
                for (int i = 0; i < enemyArray.Length; i++)
                {
                    enemyList.Add(enemyArray[i]);
                }
                listCounter = 0;
            }


            if(attackDuration > -0.5f)
            {
                attackDuration -= Time.deltaTime;
            }
            if(attackDuration <= -0.5f && swordGunAttack)
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
            else if(!isAimming && !companion.activeSelf && !swordGunAttack)
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


    public void Hit(HitPosition pos, HitDirection dir, HitPower power, float dmg, float delay = 0, bool dmgDelay = false)
    {
        //
        isHit = true;
        m_char.StateTimer = 0;
        inCombat = true;
        inCombatTimer = inCombatDuration;
        hitAnimationInfo = (int)pos * 100 + (int)dir * 10 + (int)power;
        resetHit = true;

        // Does damage based on what attack
        if(!dmgDelay)
        {
            this.GetComponent<Humanoid>().TakeDamag(dmg);
        }
        else
        {
            StartCoroutine(DelayBeforeDamage(delay, dmg));
        }
        
        Debug.Log(gameObject.name + ": Takes " + dmg + " damage");

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
            //gun.SetActive(true);
            combatAudio.clip = gunShotFX;
            combatAudio.PlayDelayed(0.5f);
            currentCombat = Combat.Gun_Shoot;

            // ఇందులో ఒకటి తక్కువ పెట్టాలి. ఎందుకంటే, పైన లిస్ట్లో ఒకటి none అని వుంది.
            // Because of None in the Enum, subtract by 1.
            CombatMoveDetails currentMoveDetails = allMoves[(int)currentCombat - 1];
            Debug.Log(currentCombat + " " + currentMoveDetails.name);
            currentAttackTime = currentMoveDetails.AT;
            currentEffectTime = currentMoveDetails.ET;
            currentEffetDistance = currentMoveDetails.ED;
            currentDmgAmount = currentMoveDetails.Dmg;
            currentDirection = currentMoveDetails.Dir;
            currentPower = currentMoveDetails.Power;
            currentHitPos = currentMoveDetails.Pos;

            attackDuration = currentAttackTime;
            Shoot();

            // TODO Modify hit dir later
            aimTarget.m_combat.Hit(aimTarget.m_combat.currentHitPos, HitDirection.backward, currentPower, currentDmgAmount, 0.5f, true);
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

        if (currentTarget != null && currentTarget.tag != "Ghost")
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

                // If attack is a sword attack, hit multiple enemies
                if(currentCombat == Combat.Sword_Attack_R || currentCombat == Combat.Sword_Attack_RL ||
                    currentCombat == Combat.Sword_Attack_Combo_LL)
                {
                    foreach(GameObject obj in enemyList)
                    {
                        if(Vector3.Distance(obj.transform.position, gameObject.transform.position) <
                            allMoves[(int)currentCombat - 1].ED )
                        {
                            obj.GetComponent<CombatManager>().Hit(currentTarget.m_combat.currentHitPos, dir, currentPower, currentDmgAmount, 0.5f, true);
                        }
                    }
                }
                else
                {
                    currentTarget.m_combat.Hit(currentTarget.m_combat.currentHitPos, dir, currentPower, currentDmgAmount, 0.5f, true);
                }
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

    public void AimMove(int dir)
    {
        if(!canMove)
        {
            return;
        }
        moveDir = dir;
    }

    // బయటినుండి ఇచ్చిన combat ని చెయ్యి
    // Function to perform the combat that was passed.
    public void PerformAction(Combat _input)
    {
        currentCombat = _input;
        // సౌన్డ్లకి కూడా ఒక variable కావాలి.
        // TODO: A Variable for the Sound in the CombatMoveDetails Struct.
        combatAudio.PlayOneShot(punchFX);

        // ఇందులో ఒకటి తక్కువ పెట్టాలి. ఎందుకంటే, పైన లిస్ట్లో ఒకటి none అని వుంది.
        // Remove 1 from the current combat because of the None in the Enum.
        CombatMoveDetails currentMoveDetails = allMoves[(int)currentCombat - 1];
        Debug.Log(currentCombat + " " + currentMoveDetails.name);
        currentAttackTime = currentMoveDetails.AT;
        currentEffectTime = currentMoveDetails.ET;
        currentEffetDistance = currentMoveDetails.ED;
        currentDmgAmount = currentMoveDetails.Dmg;
        currentDirection = currentMoveDetails.Dir;
        currentPower = currentMoveDetails.Power;
        currentHitPos = currentMoveDetails.Pos;

        Attack();

    }

    void NextCombat()
    {
        if (currentCombat == Combat.none)
        {
            combatAudio.PlayOneShot(punchFX);
            currentCombat = Combat.punch_Jab_L;
        }
        else if (currentCombat == Combat.punch_Jab_L)
        {
            combatAudio.PlayOneShot(punchFX);
            currentCombat = Combat.punch_Jab_R;
        }
        else if (currentCombat == Combat.punch_Jab_R)
        {
            combatAudio.PlayOneShot(punchFX);
            currentCombat = Combat.punch_Hook_L;
        }
        else if (currentCombat == Combat.punch_Hook_L)
        {
            combatAudio.PlayOneShot(punchFX);
            currentCombat = Combat.punch_Hook_R;
        }
        else if (currentCombat == Combat.punch_Hook_R)
        {
            combatAudio.PlayOneShot(punchFX);
            currentCombat = Combat.kick_Straight_Mid_R;
        }
        else if (currentCombat == Combat.kick_Straight_Mid_R)
        {
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.punch_Jab_L;
        }
        else
        {
            combatAudio.PlayOneShot(punchFX);
            currentCombat = Combat.punch_Jab_L;
        }

        // ఇందులో ఒకటి తక్కువ పెట్టాలి. ఎందుకంటే, పైన లిస్ట్లో ఒకటి none అని వుంది.
        // Because of None in the Enum, subtract by 1.
        CombatMoveDetails currentMoveDetails = allMoves[(int)currentCombat - 1];
        //Debug.Log(currentCombat + " " + currentMoveDetails.name);
        currentAttackTime = currentMoveDetails.AT;
        currentEffectTime = currentMoveDetails.ET;
        currentEffetDistance = currentMoveDetails.ED;
        currentDmgAmount = currentMoveDetails.Dmg;
        currentDirection = currentMoveDetails.Dir;
        currentPower = currentMoveDetails.Power;
        currentHitPos = currentMoveDetails.Pos;
        
    }

    void NextSpecial()
    {
        if (currentCombat == Combat.none)
        {
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.kick_AxeKick;
        }
        else if (currentCombat == Combat.punch_Jab_L)
        {
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.kick_AxeKick;
        }
        else if (currentCombat == Combat.punch_Jab_R)
        {
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.kick_AxeKick;
        }
        else if (currentCombat == Combat.punch_Hook_L)
        {
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.kick_AxeKick;
        }
        else if (currentCombat == Combat.punch_Hook_R)
        {
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.kick_AxeKick;
        }
        else if (currentCombat == Combat.kick_Straight_Mid_R)
        {
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.kick_HorseKick;
        }
        else
        {
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.kick_AxeKick;
        }

        // ఇందులో ఒకటి తక్కువ పెట్టాలి. ఎందుకంటే, పైన లిస్ట్లో ఒకటి none అని వుంది.
        // Again reduce by 1 because of enum definition.
        CombatMoveDetails currentMoveDetails = allMoves[(int)currentCombat - 1];

        currentAttackTime = currentMoveDetails.AT;
        currentEffectTime = currentMoveDetails.ET;
        currentEffetDistance = currentMoveDetails.ED;
        currentDmgAmount = currentMoveDetails.Dmg;
        currentDirection = currentMoveDetails.Dir;
        currentPower = currentMoveDetails.Power;
        currentHitPos = currentMoveDetails.Pos;
    }

    // --------------------------------------------------------------------------------------------
    // Sword Attack
    void NextSwordCombat()
    {
        if (currentCombat == Combat.none)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_R;
        }
        else if (currentCombat == Combat.Sword_Attack_R)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_RL;
        }
        else
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_R;
        }

        // ఇందులో ఒకటి తక్కువ పెట్టాలి. ఎందుకంటే, పైన లిస్ట్లో ఒకటి none అని వుంది.
        // Reduce by 1 because of enum.
        CombatMoveDetails currentMoveDetails = allMoves[(int)currentCombat - 1];

        currentAttackTime = currentMoveDetails.AT;
        currentEffectTime = currentMoveDetails.ET;
        currentEffetDistance = currentMoveDetails.ED;
        currentDmgAmount = currentMoveDetails.Dmg;
        currentDirection = currentMoveDetails.Dir;
        currentPower = currentMoveDetails.Power;
        currentHitPos = currentMoveDetails.Pos;

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
        }
        else if (currentCombat == Combat.punch_Jab_L)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_Sp_U;
        }
        else if (currentCombat == Combat.punch_Jab_R)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_Sp_U;
        }
        else if (currentCombat == Combat.punch_Hook_L)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_Sp_U;
        }
        else if (currentCombat == Combat.punch_Hook_R)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_Sp_U;
        }
        else if (currentCombat == Combat.kick_Straight_Mid_R)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_Sp_U;
        }
        else if (currentCombat == Combat.Sword_Attack_RL)
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_Sp_U;
        }
        else
        {
            combatAudio.PlayOneShot(swordFX);
            currentCombat = Combat.Sword_Attack_Combo_LL;
        }

        // ఇందులో ఒకటి తక్కువ పెట్టాలి. ఎందుకంటే, పైన లిస్ట్లో ఒకటి none అని వుంది.
        CombatMoveDetails currentMoveDetails = allMoves[(int)currentCombat - 1];

        currentAttackTime = currentMoveDetails.AT;
        currentEffectTime = currentMoveDetails.ET;
        currentEffetDistance = currentMoveDetails.ED;
        currentDmgAmount = currentMoveDetails.Dmg;
        currentDirection = currentMoveDetails.Dir;
        currentPower = currentMoveDetails.Power;
        currentHitPos = currentMoveDetails.Pos;

        sword.SetActive(true);
        compPos = companion.transform.position;
        companion.SetActive(false);
        swordGunAttack = true;
        attackDuration = currentAttackTime;
    }
    // --------------------------------------------------------------------------------------------


    IEnumerator DelayBeforeDamage(float delay, float dmg)
    {
        yield return new WaitForSeconds(delay);

        this.GetComponent<Humanoid>().TakeDamag(dmg);
    }

}// End of Combat Manager
