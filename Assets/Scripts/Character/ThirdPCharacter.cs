using UnityEngine;

/*This is a thrid person character script based on unity's built in script. It has been overhauled to work
 * with a new version of th third person control script. The built in animation control has been taken out 
 * and is being done differently. This script is required by the third person control script.
 * Darren Farr 11/08/2017 */

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class ThirdPCharacter : Character
{
    [SerializeField]
    float m_JumpPower = 10f;
    [Range(1f, 20f)]
    [SerializeField]
    float m_GravityMultiplier = 2f;
    [SerializeField]
    float m_MoveSpeedMultiplier = 0.08f;
    [SerializeField]
    float m_GroundCheckDistance = 0.175f;

    public GameObject charBody;
    public GameObject camera;

    Rigidbody m_Rigidbody;

    public bool m_IsGrounded;
    private bool m_Crouching;
    private bool frozen = false;

    private float turnMod;
    private float m_OrigGroundCheckDistance;

    Vector3 m_GroundNormal;
    Vector3 move;

    Quaternion charBodyRotation;
    Quaternion m_Rotation;
    Quaternion camRotation;

    //
    private CapstoneAnimation animator;

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
        kick_HorseKick

    }
    //target parameters
    [SerializeField]
    //private ThirdPCharacter currentTarget;
    private AICharacter currentTarget;


    //in combat parameters
    private bool inCombat;
    private float inCombatTimer;
    [SerializeField]
    private float inCombatDuration;


    //aimming parameters
    private bool isAimming;


    //move parameters
    private bool isMoving;
    private bool isDashing;


    //jump parameters
    private bool isJumping;
    [SerializeField]
    private float jumpUpTime;
    [SerializeField]
    private float jumpAirTime;
    [SerializeField]
    private float jumpDownTime;

    //attack parameters
    private bool isAttacking;
    private bool resetAttack;
    private float currentAttackTime;
    private float currentEffectTime;
    private float currentEffetDistance;
    private CombatDirection currentDirection;
    private HitPower currentPower;
    public HitPosition currentHitPos;

    [SerializeField]
    private float maxComboTime;

    private bool hasEffect;

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
    private Combat currentCombat;

    [SerializeField]
    private float comboTimer;


    //adjust parameters
    private bool isAdjusting;
    [SerializeField]
    private float adjustSpeed;
    [SerializeField]
    private float adjustMinDistance;

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


    //dodge parameters
    private bool isDodging;
    [SerializeField]
    private float dodgeTime;
    private int dodgeDirection;


    //roll parameters
    private bool isRolling;
    [SerializeField]
    private float rollTime;
    [SerializeField]
    private float rollSpeed;

    //hit parameters
    private bool isHit;
    private bool resetHit;
    [SerializeField]
    private float hitTime;
    [SerializeField]
    private float hitMaxWalkSpeed;
    private int hitAnimationInfo;


    //
    #region Bools
    private bool canMove
    {
        get
        {
            return !(isRolling || isHit || isAimming || isAttacking || isDodging || isAdjusting);
        }
    }

    private bool canJump
    {
        get
        {
            return !(isRolling || isJumping || isAimming || isAttacking || isDodging || isAdjusting);
        }
    }

    private bool canAttack
    {
        get
        {
            return !(isRolling || isJumping || isAttacking || isDodging || isAdjusting);
        }
    }

    private bool canSetNextAttack
    {
        get
        {
            return isAttacking;
        }
    }

    private bool canDodge
    {
        get
        {
            return !(isRolling || isJumping || isAimming || isAttacking || isDodging || isAdjusting);
        }
    }

    private bool canRoll
    {
        get
        {
            return !(isRolling || isJumping || isAimming || isAttacking || isDodging || isAdjusting);
        }
    }

    private bool canAim;

    private bool canShoot;
    #endregion



    [SerializeField]
    private CharacterState currentState;

    private CharacterState lastState;

    private float stateTimer;


    public CharacterState CurrentState
    {
        get
        {
            return currentState;
        }
    }

    //public enum CharacterState
    //{
    //    none,
    //    idle_OutCombat,
    //    idle_InCombat,
    //    run,
    //    jump_up,
    //    jump_air,
    //    jump_down,
    //    aim,
    //    shoot,
    //    attack,
    //    adjustPosition,
    //    hit,
    //    dodge,
    //    roll,

    //}

    // Use this for initialization
    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        m_OrigGroundCheckDistance = m_GroundCheckDistance;
        charBodyRotation = charBody.transform.rotation;
        m_Rotation = m_Rigidbody.transform.rotation;
        turnMod = 90.0f / 200.0f;

        //
        animator = GetComponentInChildren<CapstoneAnimation>();
        inCombatTimer = 0;

    }

    void Update()
    {
        UpdateState();

    }

    #region Movement
    /// <summary>
    /// Move the player character.
    /// </summary>
    /// <param name="vert">forward/backward motion</param>
    /// <param name="hori">side to side motion</param>
    /// <param name="charRotation">rotation of player</param>
    /// <param name="crouch">is player crouched</param>
    /// <param name="jump">should player jump</param>
    /// <param name="running">is the player running</param>
    /// <param name="dash">is the player dashing</param>
    public void Move(float vert, float hori, Quaternion camRot, bool crouch, bool jump, bool running, bool dash, bool isAI = false)
    {
        isMoving = false;
        isDashing = false;
        if (!canMove)
        {
            return;
        }
        if (vert != 0 || hori != 0)
        {
            isMoving = true;
            if (true)
            {

                Quaternion r;
                Vector3 temp, temp2;
                temp = camRot.eulerAngles;
                temp2 = charBodyRotation.eulerAngles;
                if (temp2.y > 360)
                { temp2.y -= 360; }
                temp.x = temp2.x;
                temp.z = temp2.z;

                if (vert < 0)
                {
                    temp.y = temp.y + 180f;
                    vert *= -1;
                    hori *= -1;
                }

                r = Quaternion.Euler(temp);
                charBody.transform.rotation = r;
                m_Rigidbody.transform.rotation = r;

                if (vert != 0 && hori != 0)
                {
                    if (vert > 0 && hori >= 0)
                    {
                        temp.y += (((hori - vert) + 1) * 100) * turnMod;
                    }

                    else if (vert > 0 && hori < 0)
                    {
                        temp.y += (((((-hori - vert) + 1) * 100) * turnMod)) * -1;
                    }

                    r = Quaternion.Euler(temp);
                    charBody.transform.rotation = r;
                }

                else if (hori != 0)
                {
                    temp.y = temp.y + 90f;

                    if (hori < 0)
                    {
                        temp.y = temp.y + 180f;
                    }

                    r = Quaternion.Euler(temp);
                    charBody.transform.rotation = r;
                }
            }
        }

        //calculate initial movement direction and force
        move = (vert * m_Rigidbody.transform.forward) + (hori * m_Rigidbody.transform.right);

        //check to see if the character is running or dashing and adjust modifier
        if (dash)
        {
            m_MoveSpeedMultiplier = 0.2f;
            isDashing = true;
        }
        else if (running && !crouch)
        {
            m_MoveSpeedMultiplier = 0.16f;
        }
        else if (crouch)
        {
            m_MoveSpeedMultiplier = 0.04f;
        }
        else
        {
            m_MoveSpeedMultiplier = 0.08f;
        }

        Vector3 customRight = new Vector3(0, 0, 0);
        customRight.x = m_Rigidbody.transform.forward.z;
        customRight.z = -m_Rigidbody.transform.forward.x;
        //Debug.Log(m_Rigidbody.transform.forward + " " + m_Rigidbody.transform.right + " " + customRight);

        //keep the rotation holders updated
        charBodyRotation = charBody.transform.rotation;
        m_Rotation = m_Rigidbody.transform.rotation;
        if (!isAI) { camRotation = camera.transform.rotation; }

        if (move.magnitude > 1f)
        {
            move.Normalize();
        }

        CheckGroundStatus();
        move = Vector3.ProjectOnPlane(move, m_GroundNormal);



        //m_Rigidbody.transform.RotateAround(m_Rigidbody.transform.position, m_Rigidbody.transform.up, charRotation);

        // control and velocity handling is different when grounded and airborne:
        if (m_IsGrounded)
        {
            HandleGroundedMovement(crouch, jump);
        }
        else
        {
            HandleAirborneMovement();
        }
        /*
        ScaleCapsuleForCrouching(crouch);
        PreventStandingInLowHeadroom();
        */

        //move the character
        if (m_IsGrounded && Time.deltaTime > 0)
        {
            Vector3 v = (move * m_MoveSpeedMultiplier) / Time.deltaTime;

            // we preserve the existing y part of the current velocity.
            //velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            v.y = m_Rigidbody.velocity.y;
            v.y = 0;
            m_Rigidbody.velocity = v;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Debug lines for character movement
        //Blue is m_Rigidbody forward, Red is velocity, just backwards
        //Debug.DrawLine(charPos, charPosFwd, Color.blue);
        //Debug.DrawLine(charPos, charVel, Color.red);

    }//end move

    public void Move(bool isMoving) {
        this.isMoving = isMoving;
    }


    /// <summary>
    /// freeze the character body rotation
    /// </summary>
    public void freezeChar()
    {
        charBody.transform.rotation = charBodyRotation;
        frozen = true;
    }

    /// <summary>
    /// unfreeze the character body rotation
    /// </summary>
    public void unFreezeChar()
    {
        if (frozen)
        {
            charBody.transform.rotation = m_Rotation;
            frozen = false;
        }
    }

    /// <summary>
    /// Handle movement on the ground
    /// </summary>
    /// <param name="crouch">Is the character crouched</param>
    /// <param name="jump">Is the character jumping</param>
    void HandleGroundedMovement(bool crouch, bool jump)
    {
        // check whether conditions are right to allow a jump:
        if (jump && !crouch && m_IsGrounded)
        {
            // jump!
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
            m_IsGrounded = false;

            //jump state
            isJumping = true;
            stateTimer = 0;
        }
    }//end ground movement

    //handle airborne movement
    void HandleAirborneMovement()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
        m_Rigidbody.AddForce(extraGravityForce);

        m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
    }//end airborne movement

    //check to see if player is on the ground and its status
    void CheckGroundStatus()
    {
        RaycastHit hitInfo;
        //Debug.Log(Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance));
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
        {
            m_GroundNormal = hitInfo.normal;
            m_IsGrounded = true;
        }
        else
        {
            m_IsGrounded = false;
            m_GroundNormal = Vector3.up;
        }
    }//end CheckGroundStatus
    #endregion

    void UpdateState()
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
        if (stateTimer >= 0)
        {
            stateTimer += Time.deltaTime;
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
        lastState = currentState;


        //is in combat
        if (isHit)
        {
            if (stateTimer < hitTime)
            {
                currentState = CharacterState.hit;
                animationParameter = hitAnimationInfo;
                int power = hitAnimationInfo % 10;
                if ((HitPower)power == HitPower.powerful)
                {
                    int hitDirection = (hitAnimationInfo / 10) % 10;
                    if (hitDirection >= 2)
                    {
                        ForceMove(hitMaxWalkSpeed * 0.5f, hitDirection);
                    }
                    else
                    {
                        ForceMove(hitMaxWalkSpeed, hitDirection);
                    }
                }
                else if ((HitPower)power == HitPower.ko)
                {

                }
                if (resetHit)
                {
                    lastState = CharacterState.none;
                    resetHit = false;
                }

            }
            else
            {
                isHit = false;
                stateTimer = -1;

                isAttacking = false;
                hasEffect = false;
                comboTimer = 0;
            }
        }
        else
        {
            if (isJumping)
            {
                if (stateTimer < jumpUpTime)
                {
                    currentState = CharacterState.jump_up;
                }
                else if (stateTimer >= jumpUpTime && stateTimer < jumpUpTime + jumpAirTime)
                {
                    currentState = CharacterState.jump_air;
                }
                else if (stateTimer >= jumpUpTime + jumpAirTime && stateTimer < jumpUpTime + jumpAirTime + jumpDownTime)
                {
                    currentState = CharacterState.jump_down;
                }
                else
                {
                    stateTimer = -1;
                    isJumping = false;
                }

            }
            else if (isDodging)
            {
                if (stateTimer < dodgeTime)
                {
                    currentState = CharacterState.dodge;
                    animationParameter = dodgeDirection;
                }
                else
                {
                    stateTimer = -1;
                    isDodging = false;
                }
            }
            else if (isRolling)
            {
                if (stateTimer < rollTime)
                {
                    currentState = CharacterState.roll;
                    ForceMove(rollSpeed, 1);
                }
                else
                {
                    stateTimer = -1;
                    isRolling = false;
                }
            }
            else if (isAttacking)
            {
                if (stateTimer < currentAttackTime)
                {
                    currentState = CharacterState.attack;
                    animationParameter = (int)currentCombat;
                    if (stateTimer >= currentEffectTime && !hasEffect)
                    {
                        hasEffect = true;
                        Effect();
                    }
                    if (resetAttack)
                    {
                        currentState = CharacterState.none;
                        resetAttack = false;
                    }
                }
                else
                {
                    stateTimer = -1;
                    isAttacking = false;
                    hasEffect = false;
                    comboTimer = 0;
                }

            }
            else if (isAdjusting)
            {
                if (CheckTarget())
                {
                    isAdjusting = false;
                    Attack();
                }
                else
                {
                    //look at target
                    charBody.transform.forward = currentTarget.transform.position - transform.position;
                    currentState = CharacterState.adjustPosition;
                    ForceMove(adjustSpeed, 1);
                }
            }
            else
            {

                if (isMoving)
                {
                    currentState = CharacterState.run;
                    if (isDashing)
                    {
                        animationParameter = 1;
                    }
                }
                else
                {
                    if (inCombat)
                    {
                        currentState = CharacterState.idle_InCombat;
                    }
                    else
                    {
                        currentState = CharacterState.idle_OutCombat;
                    }

                }
            }
        }



        if (lastState != currentState)
        {
            animator.Play(currentState, animationParameter);
        }


    }

    void ForceMove(float speed, int direction)
    {
        if (direction == 0)
        {
            transform.position -= charBody.transform.forward * speed * Time.deltaTime;
        }
        else if (direction == 1)
        {
            transform.position += charBody.transform.forward * speed * Time.deltaTime;
        }
        else if (direction == 2)
        {
            transform.position += charBody.transform.right * speed * Time.deltaTime;
        }
        else if (direction == 3)
        {
            transform.position -= charBody.transform.right * speed * Time.deltaTime;
        }
    }

    void ForceMove(float speed, Vector3 direction)
    {
        transform.position += direction * speed * Time.deltaTime;
    }
    public void Hit(HitPosition pos, HitDirection dir, HitPower power)
    {
        //
        isHit = true;
        stateTimer = 0;
        inCombat = true;
        inCombatTimer = inCombatDuration;
        hitAnimationInfo = (int)pos * 100 + (int)dir * 10 + (int)power;
        resetHit = true;
        //
    }


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

    public void Attack()
    {
        inCombat = true;
        inCombatTimer = inCombatDuration;
        isAttacking = true;
        stateTimer = 0;
        resetAttack = true;
    }

    void Effect()
    {

        if (currentTarget != null)
        {

            float distance = Vector3.Distance(charBody.transform.position, currentTarget.charBody.transform.position);
            HitDirection dir = HitDirection.forward;

            if (distance <= currentEffetDistance)
            {
                float angleFB = Vector3.Angle(currentTarget.charBody.transform.position - charBody.transform.position, currentTarget.charBody.transform.forward);
                float angleLR = Vector3.Angle(currentTarget.charBody.transform.position - charBody.transform.position, currentTarget.charBody.transform.right);
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

                currentTarget.Hit(currentTarget.currentHitPos, (AICharacter.HitDirection)dir, (AICharacter.HitPower)currentPower);
            }

        }
    }

    bool CheckTarget()
    {
        if (currentTarget == null)
        {
            return true;
        }
        else
        {
            float distance = Vector3.Distance(charBody.transform.position, currentTarget.charBody.transform.position);
            float angle = Vector3.Angle(currentTarget.charBody.transform.position - charBody.transform.position, charBody.transform.forward);

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
    }

    public void Roll()
    {
        if (!canRoll)
        {
            return;
        }
        stateTimer = 0;
        isRolling = true;
    }

    void NextCombat()
    {
        if (currentCombat == Combat.none)
        {
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
            currentCombat = Combat.kick_AxeKick;
            currentAttackTime = kick_AxeKick_AT;
            currentEffectTime = kick_AxeKick_ET;
            currentEffetDistance = kick_AxeKick_ED;
            currentDirection = kick_AxeKick_Dir;
            currentPower = kick_AxeKick_Power;
            currentHitPos = kick_AxeKick_Pos;
        }
    }
    /*
    void ScaleCapsuleForCrouching(bool crouch)
		{
			if (m_IsGrounded && crouch)
			{
				if (m_Crouching) return;
				m_Capsule.height = m_Capsule.height / 2f;
				m_Capsule.center = m_Capsule.center / 2f;
				m_Crouching = true;
			}
			else
			{
				Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
				float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
				if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength))
				{
					m_Crouching = true;
					return;
				}
				m_Capsule.height = m_CapsuleHeight;
				m_Capsule.center = m_CapsuleCenter;
				m_Crouching = false;
			}
		}

		void PreventStandingInLowHeadroom()
		{
			// prevent standing up in crouch-only zones
			if (!m_Crouching)
			{
				Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
				float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
				if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength))
				{
					m_Crouching = true;
				}
			}
		}
     */

}//end of class