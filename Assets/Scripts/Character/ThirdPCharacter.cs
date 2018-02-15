using UnityEngine;
using System.Collections;

// Darren Farr

/*This is a thrid person character script based on unity's built in script. It has been overhauled to work
 * with a new version of the third person control script. The built in animation control has been taken out 
 * and is being done separately. This script is required by the third person control script.
 * First Edited 09/12/2015
 * Latest edit start: 11/08/2017 */

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

/// <summary>
/// Third Person Character class to move a character.
/// </summary>
public class ThirdPCharacter : Character
{
    /// <summary>
    /// Initialization
    /// </summary>
    override protected void Start()
    {
        base.Start();

        m_combat.SetChar(this);
    }

    /// <summary>
    /// Update
    /// </summary>
    void Update()
    {
        UpdateState();

        // If character goes through the floor, pop them back up
        // TODO Update for efficiency, should run less often
        if (transform.position.y < -0.1f)
        {
            transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
        }
    }


    /// <summary>
    /// Move the player character.
    /// </summary>
    /// <param name="vert">forward/backward motion</param>
    /// <param name="hori">side to side motion</param>
    /// <param name="charRotation">rotation of player</param>
    /// <param name="jump">should player jump</param>
    /// <param name="running">is the player running</param>
    /// <param name="sprinting">is the player sprinting</param>
    override public void Move(float vert, float hori, Quaternion camRot, bool jump, bool running, bool sprinting, bool aiming)
    {
        m_combat.IsMoving = m_moving = false;
        m_combat.IsDashing = m_dashing  = false;
        if (!m_combat.canMove)
        {
            return;
        }
        if (!aiming && (vert != 0 || hori != 0) )
        {
            m_combat.IsMoving = true;
            m_moving = true;
            if (!charAudio.isPlaying && m_IsGrounded)
            {
                charAudio.PlayOneShot(footsteps4);
            }
            if (m_IsGrounded)
            {

                Quaternion r;
                Vector3 temp, temp2;
                temp = camRot.eulerAngles;
                temp2 = charBodyRotation.eulerAngles;
                if (temp2.y > 360)
                { temp2.y -= 360; }
                temp.x = 0.0f;
                temp.z = 0.0f;

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
        else if(aiming) // Rotate Character to face camera direction
        {
            Quaternion r;
            Vector3 temp, temp2;
            temp = camRot.eulerAngles;
            temp2 = charBodyRotation.eulerAngles;
            temp.y = temp.y + 7;
            if (temp2.y > 360)
            { temp2.y -= 360; }
            temp.x = temp2.x;
            temp.z = temp2.z;

            r = Quaternion.Euler(temp);

            Quaternion smoothR = Quaternion.Lerp(transform.rotation, r, Time.deltaTime * 2);

            charBody.transform.rotation = smoothR;
            m_Rigidbody.transform.rotation = smoothR;

            //charBody.transform.rotation = r;
            //m_Rigidbody.transform.rotation = r;
        }
        

        //calculate initial movement direction and force
        move = (vert * m_Rigidbody.transform.forward) + (hori * m_Rigidbody.transform.right);

        //check to see if the character is sprinting and adjust modifier
        if (sprinting)
        {
            m_MoveSpeedMultiplier = m_SprintSpeedMultiplier; //0.2
            m_combat.IsDashing = true;
            m_dashing = true;
        }
        //else if (running)
        //{
        //    m_MoveSpeedMultiplier = m_RunSpeedMultiplier; //0.16
        //    m_combat.IsDashing = true;
        //    m_dashing = true;
        //}
        else
        {
            m_MoveSpeedMultiplier = m_BaseSpeedMultiplier; //0.08
        }

        Vector3 customRight = new Vector3(0, 0, 0);
        customRight.x = m_Rigidbody.transform.forward.z;
        customRight.z = -m_Rigidbody.transform.forward.x;

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

        // control and velocity handling is different when grounded and airborne:
        if (m_IsGrounded)
        {
            HandleGroundedMovement(jump);
        }
        else
        {
            HandleAirborneMovement(vert, hori, move);
        }

        //move the character
        if (m_IsGrounded && Time.deltaTime > 0)
        {
            Vector3 v = (move * m_MoveSpeedMultiplier) / Time.deltaTime;

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
    /// Perform a smooth rotation from target to target
    /// </summary>
    /// <param name="rot"></param>
    public void SmoothRotate(Quaternion rot)
    {
        
        Quaternion r;
        Vector3 temp, temp2;
        temp = rot.eulerAngles;
        temp2 = charBodyRotation.eulerAngles;
        temp.y = temp.y + 7;
        if (temp2.y > 360)
        { temp2.y -= 360; }
        temp.x = temp2.x;
        temp.z = temp2.z;

        r = Quaternion.Euler(temp);
        if (!switchingTargets)
        {
            switchingTargets = true;
            StartCoroutine(RotateToTarget(r));
        }
        
    }

    IEnumerator RotateToTarget(Quaternion rot)
    {
        // Rotate until the difference between the two rotations is too small to care
        while (Quaternion.Angle(transform.rotation, rot) > 0.01)
        {
            // Lerp the quaternion
            Quaternion smoothR = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * 2);

            charBody.transform.rotation = smoothR;
            m_Rigidbody.transform.rotation = smoothR;
            yield return null;
        }
        switchingTargets = false;
    }

}//end of class