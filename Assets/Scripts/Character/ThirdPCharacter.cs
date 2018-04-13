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
    private float vert;
    private float hori;
    private float vertLerp;
    private float horiLerp;
    private float footStepsMod;
    [SerializeField] float lerpMod;

    /// <summary>
    /// Initialization
    /// </summary>
    override protected void Start()
    {
        base.Start();

        m_combat.SetChar(this);
        vertLerp = 0.5f;
        horiLerp = 0.5f;
        footStepsMod = 0;
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
    override public void Move(float _vert, float _hori, Quaternion camRot, bool jump, bool running, bool sprinting, bool aiming)
    {
        // Smooth buildup of movement
        if( _vert > 0 )
        {
            vert = Mathf.Lerp(0.2f, 1.0f, vertLerp);

            if (vertLerp < 1.0f)
            {
                vertLerp += lerpMod;
            }
        }
        else if( _vert < 0 )
        {
            vert = -Mathf.Lerp(0.2f, 1.0f, vertLerp);

            if (vertLerp < 1.0f)
            {
                vertLerp += lerpMod;
            }
        }
        else
        {
            vertLerp = 0.2f;
            vert = 0.0f;
        }

        if (_hori > 0)
        {
            hori = Mathf.Lerp(0.2f, 1.0f, horiLerp);

            if (horiLerp < 1.0f)
            {
                horiLerp += lerpMod;
            }
        }
        else if (_hori < 0)
        {
            hori = -Mathf.Lerp(0.2f, 1.0f, horiLerp);

            if (horiLerp < 1.0f)
            {
                horiLerp += lerpMod;
            }
        }
        else
        {
            horiLerp = 0.2f;
            hori = 0.0f;
        }



        m_combat.IsMoving = m_moving = false;
        m_combat.IsDashing = m_sprinting  = false;

        if (!m_combat.canMove)
        {
            return;     // Early out if the player is not allowed to move
        }

        if((vert != 0 || hori != 0))
        {
            m_combat.IsMoving = true;
            m_moving = true;
            footStepsMod++;
        }

        // Character Rotation && !turned
        if (!aiming && (vert != 0 || hori != 0) )
        {
            if (!charAudio.isPlaying && m_IsGrounded && footStepsMod > 15)
            {
                charAudio.PlayOneShot(footsteps4);
                footStepsMod = 0;
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
        else if(aiming && !m_moving) // Rotate Character to face camera direction
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
        else if (aiming && m_moving) // Rotate Character to face camera direction
        {
            Quaternion r;
            Vector3 temp, temp2;
            temp = camRot.eulerAngles;
            temp2 = charBodyRotation.eulerAngles;
            //temp.y = temp.y + 7;
            temp.y = temp.y - 45;
            if (temp2.y > 360)
            { temp2.y -= 360; }
            temp.x = temp2.x;
            temp.z = temp2.z;

            r = Quaternion.Euler(temp);

            charBody.transform.rotation = r;
            //m_Rigidbody.transform.rotation = r;
        }


        //calculate initial movement direction and force
        move = (vert * m_Rigidbody.transform.forward) + (hori * m_Rigidbody.transform.right);

        //check to see if the character is sprinting and adjust modifier
        if (sprinting)
        {
            m_MoveSpeedMultiplier = m_SprintSpeedMultiplier; //0.2
            m_combat.IsDashing = true;
            m_sprinting = true;
        }
        //else if (running)
        //{
        //    m_MoveSpeedMultiplier = m_RunSpeedMultiplier; //0.16
        //    m_combat.IsDashing = true;
        //    m_sprinting = true;
        //}
        else
        {
            m_MoveSpeedMultiplier = m_BaseSpeedMultiplier; //0.08
        }

        if(isInjured && !sprinting)
        {
            m_MoveSpeedMultiplier = m_MoveSpeedMultiplier / 2.0f;
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

}//end of class