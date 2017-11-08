using UnityEngine;

/*This is a thrid person character script based on unity's built in script. It has been overhauled to work
 * with the third person control script. The built in animation control has been taken out and may be replaced
 * later or added as another script once the project moves that far along. This script is required by the third
 * person control script.
 * Darren Farr 09/12/2015 */

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class ThirdPCharacter : MonoBehaviour 
{
    [SerializeField] float m_JumpPower = 10f;
    [Range(1f, 20f)][SerializeField] float m_GravityMultiplier = 2f;
    [SerializeField] float m_MoveSpeedMultiplier = 0.08f;
    [SerializeField] float m_GroundCheckDistance = 0.175f;

    public GameObject charBody;

    Rigidbody m_Rigidbody;

    public bool m_IsGrounded;
    bool m_Crouching;
    bool frozen = false;

    float m_OrigGroundCheckDistance;
    
    Vector3 m_GroundNormal;
    Vector3 move;
    
    Quaternion charBodyRotation;
    Quaternion m_Rotation;
    
	// Use this for initialization
	void Start () 
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        m_OrigGroundCheckDistance = m_GroundCheckDistance;
        charBodyRotation = charBody.transform.rotation;
        m_Rotation = m_Rigidbody.transform.rotation;

	}

    /// <summary>
    /// Move the player character.
    /// </summary>
    /// <param name="vert">forward/backward motion</param>
    /// <param name="hori">side to side motion</param>
    /// <param name="charRotation">rotation of player</param>
    /// <param name="crouch">is player crouched</param>
    /// <param name="jump">should player jump</param>
    /// <param name="running">is the characte running</param>
    public void Move(float vert, float hori, float charRotation, bool crouch, bool jump, bool running)
    {
        //calculate initial movement direction and force
        move = (vert * m_Rigidbody.transform.forward) + (hori * m_Rigidbody.transform.right);

        //check to see if the character is running and adjust modifier
        if (running && !crouch && vert > 0f)
        {
            m_MoveSpeedMultiplier = 0.16f;
        }
        else if(crouch)
        {
            m_MoveSpeedMultiplier = 0.04f;
        }
        else
        {
            m_MoveSpeedMultiplier = 0.08f;
        }

        Vector3 customRight = new Vector3(0,0,0);
        customRight.x = m_Rigidbody.transform.forward.z;
        customRight.z = -m_Rigidbody.transform.forward.x;
        //Debug.Log(m_Rigidbody.transform.forward + " " + m_Rigidbody.transform.right + " " + customRight);
        
        //keep the rotation holders updated
        charBodyRotation = charBody.transform.rotation;
        m_Rotation = m_Rigidbody.transform.rotation;

        if (move.magnitude > 1f)
        {
            move.Normalize();
        }
        
        CheckGroundStatus();
        move = Vector3.ProjectOnPlane(move, m_GroundNormal);

        //rotate the character
        m_Rigidbody.transform.RotateAround(m_Rigidbody.transform.position, m_Rigidbody.transform.up, charRotation);
        
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
        /*
        Vector3 charPos = m_Rigidbody.transform.position;
        Vector3 charPosFwd = m_Rigidbody.transform.position + (m_Rigidbody.transform.forward *2);
        charPos = charPos + new Vector3(0, 1, 0);
        charPosFwd = charPosFwd + new Vector3(0, 1, 0);

        Vector3 charVel = (m_Rigidbody.transform.position - m_Rigidbody.velocity);

        Debug.DrawLine(charPos, charPosFwd, Color.blue);
        Debug.DrawLine(charPos, charVel, Color.red);    */

    }//end move

    

    /// <summary>
    /// freeze the character body rotation
    /// </summary>
    public void freezeChar()
    {
        charBody.transform.rotation = charBodyRotation;
        frozen = true;
    }

    //unfreeze the character rotation
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