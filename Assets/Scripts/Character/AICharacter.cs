using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

/*This is an AI version of the third person character script. 
 * This script is required by the third person control script.
 * Darren Farr 11/08/2017 */

public class AICharacter : Character
{
    // Use this for initialization
    override protected void Start()
    {
        base.Start();

        m_combat.SetChar(this);
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
    override public void Move(float vert, float hori, Quaternion camRot, bool crouch, bool jump, bool running, bool dash, bool aiming)
    {}//end move

}//end of class
