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
        // HT - Hit Time.
        // AD - Attack Distance.

        // పేరు ఉత్తినే పెడతాము మనం గుర్తుపదతానికి.
        // Name doesn't matter but is the key.
        public string name;
        public float AT;
        public float AD;
        public float ET;
        public float ED;
        public float HT;
        public float Dmg;
        [Range(0.1f, 2.5f)]
        public float StrikeDistance;
        public HitPosition Pos;
        public CombatDirection Dir;
        public HitPower Power;
        public AudioClip CombatSFX;
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
    private float currentHitTime;
    private float currentAttackDistance;
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

    // Dummy Parameter to check.
    private float currentDistanceToTarget;


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
    [SerializeField] private float rollTime;
    [SerializeField] private float runRollTime;
    [SerializeField] private float rollSpeed;
    public bool IsRolling
    { get { return isRolling; }
      set { isRolling = value; }
    }
    public float RollTime
    { get { return rollTime; } }
    public float RunRollTime
    { get { return runRollTime; } }
    public float RollSpeed
    { get { return rollSpeed; } }

    // Turning Parameters.
    [SerializeField]
    private bool isTurning = false;
    public bool IsTurning {
        get { return isTurning; }
        set { isTurning = value; }
    }
    [SerializeField]
    private Quaternion targetRotation;

    //hit parameters
    private bool isHit;
    private bool resetHit;
    [SerializeField]
    public float timeBetweenAttacks;
    public float TimeBetweenAttacks
    {
        get { return timeBetweenAttacks; }
    }

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
        set { hitTime = value; }
    }

    //adjust parameters
    private bool isAdjusting;
    [SerializeField]
    private float adjustSpeed;
    [SerializeField]
    public float adjustMinDistance;
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
    public float adjustMaxDistance;

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
            companion.GetComponent<Companion>().inCombat = m_char.inCombat;

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


    public void Hit(HitPosition pos, HitDirection dir, HitPower power, float hitTime, float dmg, float delay = 0, bool dmgDelay = false)
    {
        //
        isHit = true;
        m_char.StateTimer = 0;
        inCombat = true;
        inCombatTimer = inCombatDuration;
        hitAnimationInfo = (int)pos * 100 + (int)dir * 10 + (int)power;
        HitTime = hitTime;
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

        isAttacking = false;
        isRolling = false;
        isAdjusting = false;
        isJumping = false;
        isMoving = false;

    }

    public void Hit(CombatMoveDetails combatMoveDetails, float delay = 0, bool dmgDelay = false)
    {
        isHit = true;
        m_char.StateTimer = 0;
        inCombat = true;
        inCombatTimer = inCombatDuration;
        hitAnimationInfo = (int)combatMoveDetails.Pos * 100 + (int)combatMoveDetails.Dir * 10 + (int)combatMoveDetails.Power;
        HitTime = hitTime;
        resetHit = true;

        // Does damage based on what attack
        if (!dmgDelay)
        {
            this.GetComponent<Humanoid>().TakeDamag(combatMoveDetails.Dmg);
        }
        else
        {
            StartCoroutine(DelayBeforeDamage(delay, combatMoveDetails.Dmg));
        }

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
        if (!canAttack || !isAimming || aimTarget == null)
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
            //Debug.Log(currentCombat + " " + currentMoveDetails.name);
            currentAttackTime = currentMoveDetails.AT;
            currentEffectTime = currentMoveDetails.ET;
            currentEffetDistance = currentMoveDetails.ED;
            currentDmgAmount = currentMoveDetails.Dmg;
            currentDirection = currentMoveDetails.Dir;
            currentPower = currentMoveDetails.Power;
            currentHitPos = currentMoveDetails.Pos;
            currentHitTime = currentMoveDetails.HT;

            attackDuration = currentAttackTime;
            Shoot();

            // TODO Modify hit dir later
            aimTarget.m_combat.Hit(currentMoveDetails, 0.5f, true);
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
            m_char.GetComponent<PC>().UseSpecial(25);
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
            m_char.GetComponent<PC>().UseSpecial(50);
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
        m_char.freezeChar();
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
        m_char.GetComponent<PC>().UseSpecial(50);
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
                            obj.GetComponent<CombatManager>().Hit(currentTarget.m_combat.currentHitPos, dir, currentPower, currentHitTime, currentDmgAmount, 0.5f, true);
                        }
                    }
                }
                else
                {
                    currentTarget.m_combat.Hit(currentTarget.m_combat.currentHitPos, dir, currentPower, currentHitTime, currentDmgAmount, 0.5f, true);
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

    public bool Dodge(int dir)
    {
        if (!canDodge)
        {
            return false;
        }
        dodgeDirection = dir;
        m_char.StateTimer = 0;
        isDodging = true;

        return true;
    }

    public bool Roll()
    {
        if (!canRoll)
        {
            return false;
        }
        m_char.StateTimer = 0;
        isRolling = true;
        return true;
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
    public void PerformAction(Action _action)
    {

        Combat _input = _action.combat;

        if ( Combat.none == _input)
        {
            // Perform the Action even though the Combat is set to None.
            switch (_action.name)
            {
                case "Face_Towards_The_Enemy":
                    isTurning = true;
                    return;
                default:
                    return;
            }

        }

        currentCombat = _input;

        // ఇందులో ఒకటి తక్కువ పెట్టాలి. ఎందుకంటే, పైన లిస్ట్లో ఒకటి none అని వుంది.
        // Remove 1 from the current combat because of the None in the Enum.
        CombatMoveDetails currentMoveDetails = allMoves[(int)currentCombat - 1];
        currentAttackTime = currentMoveDetails.AT;
        currentEffectTime = currentMoveDetails.ET;
        currentEffetDistance = currentMoveDetails.ED;
        currentDmgAmount = currentMoveDetails.Dmg;
        currentDirection = currentMoveDetails.Dir;
        currentPower = currentMoveDetails.Power;
        currentHitPos = currentMoveDetails.Pos;
        currentHitTime = currentMoveDetails.HT;


        // Move according to the Attack Distance.

        // Variable for the Sound
        combatAudio.PlayOneShot(currentMoveDetails.CombatSFX);
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
            combatAudio.PlayOneShot(kickFX);
            currentCombat = Combat.kick_Straight_Mid_R;
        }
        else if (currentCombat == Combat.kick_Straight_Mid_R)
        {
            combatAudio.PlayOneShot(punchFX);
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
        currentHitTime = currentMoveDetails.HT;

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
        currentHitTime = currentMoveDetails.HT;
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
        currentHitTime = currentMoveDetails.HT;

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
        currentHitTime = currentMoveDetails.HT;

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
