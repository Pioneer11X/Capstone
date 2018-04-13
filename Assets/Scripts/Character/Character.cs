using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

abstract public class Character : MonoBehaviour
{
    public enum CharacterState
    {
        none,
        turning,
        idle_OutCombat,
        idle_InCombat,
        idle_Injured,
        walk,
        walk_Injured,
        walk_Turn_L,
        walk_Turn_R,
        run,
        run_Turn_L,
        run_Turn_R,
        run_Jump,
        jump_up,
        jump_air,
        jump_down,
        attack,
        adjustPosition,
        hit,
        dodge,
        roll,
        roll_Run,
        draw_Gun,
        aim_Idle,
        shoot,
        holster_Gun,
        aim_Move,
        dead
    }

    public AudioSource charAudio;
    public AudioSource charCombatAudio;
    [SerializeField] protected AudioClip footsteps1;
    [SerializeField] protected AudioClip footsteps2;
    [SerializeField] protected AudioClip footsteps3;
    [SerializeField] protected AudioClip footsteps4;
    [SerializeField] protected AudioClip jumpFX;
    [SerializeField] protected AudioClip landFX;

    [SerializeField]
    protected float m_JumpPower;
    [Range(1f, 20f)]
    [SerializeField]
    protected float m_GravityMultiplier = 2f;
    [SerializeField]
    protected float m_BaseSpeedMultiplier;
    
    [SerializeField]
    protected float m_SlopeSpeedMultiplier = 0.18f;
    [SerializeField]
    protected float m_GroundCheckDistance;
    [SerializeField]
    protected float m_GroundCheckRadius;


    protected float m_MoveSpeedMultiplier;
    protected float m_SprintSpeedMultiplier;
    protected float m_RunSpeedMultiplier;


    public GameObject charBody;
    public new GameObject camera;

    protected Humanoid humanoid;
    public CombatManager m_combat;
    protected CapstoneAnimation animator;

    protected Rigidbody m_Rigidbody;

    public bool switchingTargets;
    public bool inCombat;
    public bool m_IsGrounded;
    protected bool m_jump;
    protected bool m_sprinting;
    protected bool m_moving;
    protected bool frozen = false;
    private bool hasJumped = false;
    public bool isDead;
    protected bool isInjured;

    [SerializeField]
    public float turnSpeed = 100;

    protected float turnMod;
    protected float m_OrigGroundCheckDistance;

    protected int animationParameter;
    public int AnimationParameter
    { set { animationParameter = value; } }

    protected Vector3 m_GroundNormal;
    protected Vector3 move;

    protected Quaternion charBodyRotation;
    protected Quaternion m_Rotation;
    protected Quaternion camRotation;

    protected bool hasEffect;
    public bool HasEffect
    {
        get { return hasEffect; }
        set { hasEffect = value; }
    }


    [SerializeField]
    protected CharacterState currentState;
    protected CharacterState lastState;

    protected float stateTimer;
    public float StateTimer
    {
        get { return stateTimer; }
        set { stateTimer = value; }
    }

    public CharacterState CurrentState
    {
        get { return currentState; }
        set { currentState = value; }
    }
    public CharacterState LastState
    {
        get { return lastState; }
        set { lastState = value; }
    }

    private bool runningJump;
    protected bool turnAround;
    public bool TurnAround
    { set { turnAround = value; } }
    protected bool runTurnAround;
    public bool RunTurnAround
    { set { runTurnAround = value; } }

    private float initialY;

    // Use this for initialization
    virtual protected void Start ()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        m_OrigGroundCheckDistance = m_GroundCheckDistance;
        charBodyRotation = charBody.transform.rotation;
        m_Rotation = m_Rigidbody.transform.rotation;
        turnMod = 90.0f / 200.0f;

        animator = GetComponentInChildren<CapstoneAnimation>();
        m_combat = GetComponentInChildren<CombatManager>();
        humanoid = GetComponentInChildren<Humanoid>();

        m_JumpPower = humanoid.JumpPower;
        m_BaseSpeedMultiplier = humanoid.SpeedMove;
        m_SprintSpeedMultiplier = humanoid.SpeedRun;
        m_RunSpeedMultiplier = humanoid.SpeedRun;

        stateTimer = 0;
        m_MoveSpeedMultiplier = m_BaseSpeedMultiplier;

        inCombat = false;
        switchingTargets = false;
        runningJump = false;
        turnAround = false;
        runTurnAround = false;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    #region Movement
    /// <summary>
    /// Move the player character.
    /// </summary>
    /// <param name="vert">forward/backward motion</param>
    /// <param name="hori">side to side motion</param>
    /// <param name="charRotation">rotation of player</param>
    /// <param name="jump">should character jump</param>
    /// <param name="running">is the character running</param>
    /// <param name="sprinting">is the character sprinting</param>
    abstract public void Move(float vert, float hori, Quaternion camRot, bool jump, bool running, bool sprinting, bool aiming);

    /// <summary>
    /// Move AI Character
    /// </summary>
    /// <param name="_isMoving">Is the character moving?</param>
    virtual public void Move(bool _isMoving)
    {
        m_combat.IsMoving = _isMoving;
    }

    /// <summary>
    /// freeze the character body rotation
    /// </summary>
    public void freezeChar()
    {
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.constraints = RigidbodyConstraints.FreezePosition;
        unFreezeChar();

    }

    /// <summary>
    /// unfreeze the character body rotation
    /// </summary>
    protected void unFreezeChar()
    {
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    /// <summary>
    /// Handle movement on the ground
    /// </summary>
    /// <param name="jump">Is the character jumping</param>
    protected void HandleGroundedMovement(bool jump)
    {
        // check whether conditions are right to allow a jump:
        if (jump && m_IsGrounded)
        {
            // Set initial Y position
            initialY = transform.position.y;

            charAudio.Stop();
            charAudio.PlayOneShot(jumpFX);
            // jump!
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
            m_IsGrounded = false;

            //jump state
            m_combat.IsJumping = true;
            m_jump = true;
            stateTimer = 0;
        }
    }//end ground movement

    /// <summary>
    /// handle airborne movement
    /// </summary>
    protected void HandleAirborneMovement(float v, float h, Vector3 move)
    {
        // apply extra gravity from multiplier:
        if( !( transform.position.y > 3 + initialY) )
        {
            Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
            m_Rigidbody.AddForce(extraGravityForce);
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x + (move.x / 10), m_Rigidbody.velocity.y, m_Rigidbody.velocity.z + (move.z / 10));

            m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
        }
        

       
    }//end airborne movement

    /// <summary>
    /// check to see if player is on the ground and its status
    /// </summary>
    protected void CheckGroundStatus()
    {
        RaycastHit hitInfo;

        // Check to see if player is hiting a curb or collidier directly on and would have trouble navigating over it.
        //if (Physics.Raycast(transform.position + (Vector3.up * 0.08f), new Vector3(0, 0, 1f), 0.3f) ||
        //    Physics.Raycast(transform.position + (Vector3.up * 0.08f), new Vector3(0, 0, -1f), 0.3f) ||
        //    Physics.Raycast(transform.position + (Vector3.up * 0.08f), new Vector3(1, 0, 0f), 0.3f) ||
        //    Physics.Raycast(transform.position + (Vector3.up * 0.08f), new Vector3(-1, 0, 0f), 0.3f))
        //{
        //    // Bump character up a bit to overcome curb slopes
        //    transform.position = new Vector3(transform.position.x, transform.position.y + 0.08f, transform.position.z);
        //}

        if (Physics.SphereCast(transform.position + (Vector3.up * 0.1f), m_GroundCheckRadius, Vector3.down, out hitInfo, m_GroundCheckDistance))
        //if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
        {
            //Debug.Log(hitInfo.normal.x + "  " + hitInfo.normal.y + "  " + hitInfo.normal.z);
            //Debug.DrawLine(transform.position + (Vector3.up * 0.1f), hitInfo.point);
            if (hitInfo.normal.y < 0.9f)
            {
                m_MoveSpeedMultiplier = m_SlopeSpeedMultiplier;
            }
            m_GroundNormal = hitInfo.normal;

            if(!m_IsGrounded && hasJumped)
            {
                charAudio.Stop();
                charAudio.PlayOneShot(landFX);
                hasJumped = false;
            }
            m_IsGrounded = true;
        }
        else
        {
            m_IsGrounded = false;
            m_GroundNormal = Vector3.up;
        }
    }//end CheckGroundStatus
    #endregion

    /// <summary>
    /// Update the characters state, used for animations and state based stuff
    /// </summary>
    protected void UpdateState()
    {
        if(!(humanoid.Health / humanoid.BaseHealth < 0.4f) )
        {
            isInjured = false;
        }
        else
        {
            isInjured = true;
        }

        animationParameter = 0;

        if (stateTimer >= 0)
        {
            stateTimer += Time.deltaTime;
        }

        lastState = currentState;


        if (!m_combat.UpdateState(stateTimer))
        {
            if (m_jump)
            {
                if(currentState == CharacterState.run)
                {
                    runningJump = true;
                }
                if (!runningJump)
                {
                    if (stateTimer < m_combat.JumpUpTime)
                    {
                        hasJumped = true;
                        currentState = CharacterState.jump_up;
                    }
                    else if (stateTimer >= m_combat.JumpUpTime && stateTimer < m_combat.JumpUpTime + m_combat.JumpAirTime)
                    {
                        currentState = CharacterState.jump_air;
                    }
                    else if (stateTimer >= m_combat.JumpUpTime + m_combat.JumpAirTime && stateTimer < m_combat.JumpUpTime + m_combat.JumpAirTime + m_combat.JumpDownTime)
                    {
                        currentState = CharacterState.jump_down;
                    }
                    else
                    {
                        stateTimer = -1;
                        m_jump = false;
                        m_combat.IsJumping = false;
                    }
                }
                else if(runningJump)
                {
                    if (stateTimer < m_combat.JumpUpTime + 0.5f)
                    {
                        hasJumped = true;
                        currentState = CharacterState.run_Jump;
                    }
                    else
                    {
                        stateTimer = -1;
                        m_jump = false;
                        runningJump = false;
                        m_combat.IsJumping = false;
                    }
                }

            }
            else if (m_combat.IsDodging)
            {
                if (stateTimer < m_combat.DodgeTime)
                {
                    currentState = CharacterState.dodge;
                    animationParameter = m_combat.DodgeDirection;
                }
                else
                {
                    stateTimer = -1;
                    m_combat.IsDodging = false;
                }
            }
            else if (m_combat.IsRolling)
            {
                if (stateTimer < m_combat.RollTime && (currentState != CharacterState.run && currentState != CharacterState.roll_Run))
                {
                    currentState = CharacterState.roll;
                    if (!m_combat.IsHit)
                    {
                        ForceMove(m_combat.RollSpeed, 1);
                    }
                    else
                    {
                        ForceMove(m_combat.RollSpeed, 1);
                    }
                }
                else if (stateTimer < m_combat.RunRollTime && currentState != CharacterState.roll)
                {
                    currentState = CharacterState.roll_Run;
                    ForceMove(m_combat.RollSpeed * 1.25f, 1);
                }
                else
                {
                    stateTimer = -1;
                    m_combat.IsRolling = false;
                }
            }
            else if (m_combat.IsAttacking)
            {
                if (isDead)
                {
                    m_combat.IsAttacking = false;
                    return;
                }

                if (stateTimer < m_combat.CurrentAttackTime)
                {
                    currentState = CharacterState.attack;
                    inCombat = true;
                    animationParameter = (int)m_combat.CurrentCombat;
                    if (stateTimer >= m_combat.CurrentEffectTime && !hasEffect)
                    {
                        hasEffect = true;
                        m_combat.Effect();
                    }
                    if (m_combat.ResetAttack)
                    {
                        currentState = CharacterState.none;
                        m_combat.ResetAttack = false;
                    }
                }
                else
                {
                    stateTimer = -1;
                    m_combat.IsAttacking = false;
                    hasEffect = false;
                    m_combat.ComboTimer = 0;
                }

            }
            else if (m_combat.IsAdjusting)
            {
                // TODO: A better way to check if it is Player
                if (m_combat.CheckTarget() && ( "Player" == this.tag))
                {
                    m_combat.IsAdjusting = false;
                    m_combat.Attack();
                }
                else
                {

                    if ( "Enemy" == this.tag)
                    {
                        // Check for the distance.. and Move only when needed..
                        if ( Vector3.Distance(this.transform.position, m_combat.CurrentTarget.transform.position) > m_combat.GetAdjustMaxDistance())
                        {
                            //look at target
                            charBody.transform.forward = m_combat.CurrentTarget.transform.position - transform.position;
                            currentState = CharacterState.adjustPosition;
                            ForceMove(m_combat.AdjustSpeed, 1);
                        }else if (Vector3.Distance(this.transform.position, m_combat.CurrentTarget.transform.position) < m_combat.GetAdjustMinDistance())
                        {
                            //look at target
                            charBody.transform.forward = m_combat.CurrentTarget.transform.position - transform.position;
                            currentState = CharacterState.adjustPosition;
                            ForceMove(m_combat.AdjustSpeed, 0);
                        }
                        else
                        {
                            m_combat.IsAdjusting = false;
                            GetComponent<ActionSelector>().selectNextOption();
                        }
                    }
                    else
                    {
                        //look at target
                        charBody.transform.forward = m_combat.CurrentTarget.transform.position - transform.position;
                        currentState = CharacterState.adjustPosition;
                        if(Vector3.Distance(transform.position, m_combat.CurrentTarget.transform.position) < m_combat.CurrentAttackDistance )
                        {
                            ForceMove(m_combat.AdjustSpeed, 0);
                        }
                        else
                        {
                            ForceMove(m_combat.AdjustSpeed, 1);
                        }
                    }
                    
                }
            }
            else if (m_combat.IsAimming && m_combat.moveDir > -1)
            {
                animationParameter = m_combat.moveDir;
                currentState = CharacterState.aim_Move;
            }
            else if (m_combat.IsTurning)
            {
                currentState = CharacterState.turning;
            }
            else
            {
                if (m_moving)
                {
                    if (!isInjured)
                    {
                        currentState = CharacterState.walk;
                    }
                    else
                    {
                        currentState = CharacterState.walk_Injured;
                    }
                    if (m_sprinting)
                    {
                        currentState = CharacterState.run;
                    }
                }
                else
                {
                    turnAround = false;
                    runTurnAround = false;
                    if (m_combat.InCombat && !m_combat.IsAimming)
                    {
                        currentState = CharacterState.idle_InCombat;
                        inCombat = true;
                    }
                    else if(m_combat.IsAimming)
                    {
                        currentState = CharacterState.aim_Idle;
                        inCombat = true;
                    }
                    else
                    {
                        if (!isInjured)
                        {
                            currentState = CharacterState.idle_OutCombat;
                        }
                        else
                        {
                            currentState = CharacterState.idle_Injured;
                        }
                        inCombat = false;

                    }

                }
            }
        }


        if (isDead)
        {
            currentState = CharacterState.dead;

            if ( null != GetComponent<CustomNavigationAgent>())
            {
                GetComponent<CustomNavigationAgent>().SetIsStopped(true);
            }

        }

        if (lastState != currentState)
        {
            animator.Play(currentState, animationParameter);
        }


    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="speed"></param>
    /// <param name="direction"></param>
    public void ForceMove(float speed, int direction)
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="speed"></param>
    /// <param name="direction"></param>
    public void ForceMove(float speed, Vector3 direction)
    {
        transform.position += direction * speed * Time.deltaTime;
    }
}// End Character
