using UnityEngine;

/*This is a thrid person character script based on unity's built in script. It has been overhauled to work
 * with a new version of th third person control script. The built in animation control has been taken out 
 * and is being done differently. This script is required by the third person control script.
 * Darren Farr 11/08/2017 */

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class ThirdPCharacter : MonoBehaviour
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

    public enum HitPower
    {
        weak,
        powerful
    }

    public enum Combat
    {
        punch,

    }
    //target parameters
    [SerializeField]
    private ThirdPCharacter currentTarget;


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
    [SerializeField]
    private float attackTime;
    [SerializeField]
    private float effectTime;
    private bool hasEffect;
    [SerializeField]
    private float effetDistance;

    //adjust parameters
    private bool isAdjusting;
    [SerializeField]
    private float adjustSpeed;
    [SerializeField]
    private float adjustMinDistance;
    [SerializeField]
    private float adjustMaxDistance;
    [SerializeField]
    private float adjustAgle;



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
    [SerializeField]
    private float hitTime;
    [SerializeField]
    private float hitMaxWalkSpeed;
    private int hitDirection;




    //
    #region Bools
    private bool canMove
    {
        get
        {
            return !(isRolling || isAimming || isAttacking || isDodging || isAdjusting);
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

    public enum CharacterState
    {
        idle_OutCombat,
        idle_InCombat,
        run,
        jump_up,
        jump_air,
        jump_down,
        aim,
        shoot,
        attack,
        adjustPosition,
        hit,
        dodge,
        roll,

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
        inCombatTimer = 0;

    }

    void Update()
    {
        UpdateState();

    }

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
    public void Move(float vert, float hori, Quaternion camRot, bool crouch, bool jump, bool running, bool dash)
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
        camRotation = camera.transform.rotation;

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
        lastState = currentState;


        //is in combat

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
            if (stateTimer < attackTime)
            {
                currentState = CharacterState.attack;
                if (stateTimer >= effectTime && !hasEffect)
                {
                    hasEffect = true;
                    Effect();
                }
            }
            else
            {
                stateTimer = -1;
                isAttacking = false;
                hasEffect = false;
            }
        }
        else if (isAdjusting)
        {
            if (CheckTarget())
            {
                isAdjusting = false;
                Attack(Combat.punch);
            }
            else
            {
                //look at target
                charBody.transform.forward = currentTarget.transform.position - transform.position;
                currentState = CharacterState.adjustPosition;
                ForceMove(adjustSpeed, 1);
            }
        }
        else if (isHit) {
            if (stateTimer < hitTime)
            {
                currentState = CharacterState.hit;
                animationParameter = hitDirection;
                if (hitDirection >= 2)
                {
                    ForceMove(hitMaxWalkSpeed * 0.5f, hitDirection);
                }
                else {
                    ForceMove(hitMaxWalkSpeed, hitDirection);
                }
                
            }
            else {
                stateTimer = -1;
                isHit = false;
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
        hitDirection = (int)dir;
        //
    }

    public void PrepareAttack(Combat combat)
    {
        if (!canAttack)
        {
            return;
        }
        if (CheckTarget())
        {
            Attack(combat);
        }
        else
        {
            Adjust();
        }
    }

    public void Attack(Combat combat)
    {
        inCombat = true;
        inCombatTimer = inCombatDuration;
        isAttacking = true;
        stateTimer = 0;
    }

    void Effect()
    {
        if (currentTarget != null)
        {
            float distance = Vector3.Distance(charBody.transform.position, currentTarget.charBody.transform.position);
            HitDirection dir = HitDirection.forward;

            if (distance <= effetDistance)
            {
                float angleFB = Vector3.Angle(currentTarget.charBody.transform.position - charBody.transform.position, currentTarget.charBody.transform.forward);
                float angleLR = Vector3.Angle(currentTarget.charBody.transform.position - charBody.transform.position, currentTarget.charBody.transform.right);
                if (angleFB <= 45)
                {
                    dir = HitDirection.backward;
                }
                else if (angleFB >= 135) {
                    dir = HitDirection.forward;
                }
                else
                {
                    if (angleLR <= 45) {
                        dir = HitDirection.left;
                    }
                    else if (angleLR >= 135) {
                        dir = HitDirection.right;
                    }
                }
                currentTarget.Hit(HitPosition.high, dir, HitPower.powerful);
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