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
    float m_SlopeSpeedMultiplier = 0.18f;
    [SerializeField]
    float m_GroundCheckDistance;
    [SerializeField]
    float m_GroundCheckRadius;

    public GameObject charBody;
    public GameObject camera;
    public CombatManager m_combat;

    Rigidbody m_Rigidbody;

    public bool m_IsGrounded;
    private bool m_Crouching;
    private bool m_jump;
    private bool m_dashing;
    private bool m_moving;
    private bool frozen = false;

    private float turnMod;
    private float m_OrigGroundCheckDistance;

    private int animationParameter;
    public int AnimationParameter
    { set { animationParameter = value; } }

    Vector3 m_GroundNormal;
    Vector3 move;

    Quaternion charBodyRotation;
    Quaternion m_Rotation;
    Quaternion camRotation;

    //
    private CapstoneAnimation animator;


    private bool hasEffect;
    public bool HasEffect
    { get { return hasEffect; }
      set { hasEffect = value; }
    }


    [SerializeField]
    private CharacterState currentState;

    private CharacterState lastState;

    private float stateTimer;
    public float StateTimer
    { get { return stateTimer; }
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
        m_combat = GetComponentInChildren<CombatManager>();
        stateTimer = 0;
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
        m_combat.IsMoving = m_moving = false;
        m_combat.IsDashing = m_dashing  = false;
        if (!m_combat.canMove)
        {
            return;
        }
        if (vert != 0 || hori != 0)
        {
            m_combat.IsMoving = true;
            m_moving = true;
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
            m_combat.IsDashing = true;
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

    public void Move(bool _isMoving) {
        m_combat.IsMoving = _isMoving;
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
            m_combat.IsJumping = true;
            m_jump = true;
            stateTimer = 0;
        }
    }//end ground movement

    /// <summary>
    /// handle airborne movement
    /// </summary>
    void HandleAirborneMovement()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
        m_Rigidbody.AddForce(extraGravityForce);

        m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
    }//end airborne movement

    /// <summary>
    /// check to see if player is on the ground and its status
    /// </summary>
    void CheckGroundStatus()
    {
        RaycastHit hitInfo;

        // Check to see if player is hiting a curb or collidier directly on and would have trouble navigating over it.
        if(Physics.Raycast(transform.position + (Vector3.up * 0.08f), new Vector3(0, 0, 1f), 0.3f) ||
            Physics.Raycast(transform.position + (Vector3.up * 0.08f), new Vector3(0, 0, -1f), 0.3f) ||
            Physics.Raycast(transform.position + (Vector3.up * 0.08f), new Vector3(1, 0, 0f), 0.3f) ||
            Physics.Raycast(transform.position + (Vector3.up * 0.08f), new Vector3(-1, 0, 0f), 0.3f) )
        {
            // Bump character up a bit to overcome curb slopes
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.08f, transform.position.z);
        }

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
        animationParameter = 0;
        
        if (stateTimer >= 0)
        {
            stateTimer += Time.deltaTime;
        }

        lastState = currentState;


        if( !m_combat.UpdateState(stateTimer) )
        {
            if (m_jump)
            {
                if (stateTimer < m_combat.JumpUpTime)
                {
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
                if (stateTimer < m_combat.RollTime)
                {
                    currentState = CharacterState.roll;
                    ForceMove(m_combat.RollSpeed, 1);
                }
                else
                {
                    stateTimer = -1;
                    m_combat.IsRolling = false;
                }
            }
            else if (m_combat.IsAttacking)
            {
                if (stateTimer < m_combat.CurrentAttackTime)
                {
                    currentState = CharacterState.attack;
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
                if (m_combat.CheckTarget())
                {
                    m_combat.IsAdjusting = false;
                    m_combat.Attack();
                }
                else
                {
                    //look at target
                    charBody.transform.forward = m_combat.CurrentTarget.transform.position - transform.position;
                    currentState = CharacterState.adjustPosition;
                    ForceMove(m_combat.AdjustSpeed, 1);
                }
            }
            else
            {

                if (m_moving)
                {
                    currentState = CharacterState.run;
                    if (m_dashing)
                    {
                        animationParameter = 1;
                    }
                }
                else
                {
                    if (m_combat.InCombat)
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

    void ForceMove(float speed, Vector3 direction)
    {
        transform.position += direction * speed * Time.deltaTime;
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