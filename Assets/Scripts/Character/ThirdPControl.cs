using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

/*This is a third person controller script built on the basis of unity's built in third person controller.
 *The script has been overhualed to work with a third person camera and act in a manner similar to most MMO
 *third person camera control. 
 *Darren Farr 11/08/2017 */

[RequireComponent(typeof(ThirdPCharacter))]

public class ThirdPControl : MonoBehaviour {

    public GameObject myCarmera;

    [SerializeField] private float mouseRotationFactor; //Set mouse rotation sensitivity
    [SerializeField] private float scrollFactor;        //Set scroll zoom sensitivity

    // Default unity names for mouse axes
    public string mouseHorizontalAxisName = "Mouse X";
    public string mouseVerticalAxisName = "Mouse Y";
    public string scrollAxisName = "Mouse ScrollWheel";

    private ThirdPCharacter m_Character; // A reference to the ThirdPersonCharacter on the object

    private Vector3 m_Move;              // the world-relative desired move direction, calculated from the camForward and user input.
    private Vector3 myCamFwd;

    private bool m_Jump;
    private bool running;
    private bool playing = false;

    private bool crouch = false;
    private bool attacking = false;
    private bool rolling = false;

    private int combatCounter = 0;
    private int rollCounter = 0;
    private int waitTime = 45;

    private float v = 0;
    private float h = 0;
    private float rotationY = 0;
    private float rotationX = 0;
    private float charRotationX = 0;
    private float zoom = 0;

    //animation controller
    private Animator anim;



    private void Start()
    {
        // get the third person character ( this should never be null due to require component )
        m_Character = GetComponent<ThirdPCharacter>();
        anim = GetComponent<ThirdPCharacter>().charBody.GetComponent<Animator>();
    }//end start
    
    private void Update()
    {
        //check to see if the character should jump
        if (!m_Jump)
        {
            //m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_Jump = true;
            }
            /*else if (Input.GetMouseButtonDown(4))   //foward mouse button
            {
                m_Jump = true;
            }*/
        }
    }//end update

    // Fixed update is called in sync with physics
    // This controls the character's and camera's movement
    private void FixedUpdate()
    {
        v = 0;
        h = 0;
        rotationY = 0;
        rotationX = 0;
        charRotationX = 0;
        running = false;

        // Need to start working on adding multiple types of input devices
        //rotate camera
        rotationY = Input.GetAxis(mouseVerticalAxisName) * mouseRotationFactor;
        rotationX = Input.GetAxis(mouseHorizontalAxisName) * mouseRotationFactor;

        //character movement (forward/backward motion) (rotate left/right)
        v = CrossPlatformInputManager.GetAxis("Vertical");
        h = CrossPlatformInputManager.GetAxis("Horizontal");

        //scroll for zoom
        zoom = Input.GetAxis(scrollAxisName) * scrollFactor;

        #region Specific Mouse Input
        /*//left mouse button
        if (Input.GetMouseButton(0)) {}//end if mouse 0

        //right mouse button
        if (Input.GetMouseButton(1)){}//end if mouse 0

        //middle mouse button
        if (Input.GetMouseButton(2)) { Debug.Log("I am mouse button 2"); }

        //back mouse button
        if (Input.GetMouseButton(3)) { Debug.Log("I am mouse button 3"); }

        //foward mouse button
        if (Input.GetMouseButton(4)) { Debug.Log("I am mouse button 4"); } */
        #endregion



        //if character is moving forward or backward, unfreeze any rotation
        if (v != 0)
        {
            m_Character.unFreezeChar();
        }

        //character crouch
        if(Input.GetKeyDown(KeyCode.C))
        {
            crouch = !crouch;
        }


        //----------------------------------------------------------------------------------------
        //------------------------------COMBAT----------------------------------------------------
        //----------------------------------------------------------------------------------------
        //attack
        //if (Input.GetKeyDown(KeyCode.Alpha1) && !gameObject.GetComponent<BaseCharacter>().a1OnCoolDown && !attacking)
        //{
        //    gameObject.GetComponent<BaseCharacter>().a1OnCoolDown = true;
        //    attacking1 = true;
        //    attacking = true;
        //    waitTime = 40;
        //    gameManager.playerAttacking = true;
        //}


        //----------------------------------------------------------------------------------------
        // walk speed multiplier
        if (Input.GetKey(KeyCode.LeftShift) && v > 0 && !crouch)
        {
            //if (gameObject.GetComponent<BaseCharacter>().StaminaPoints > 0)
            //{
            running = true;
            //    gameObject.GetComponent<BaseCharacter>().useStamina(0.5f);
            //    gameObject.GetComponent<BaseCharacter>().RegenStamina = false;
            //}
        }
        //else if (Input.GetKeyUp(KeyCode.LeftShift))
        //{
        //    if(!gameObject.GetComponent<BaseCharacter>().InCombat)
        //    {
        //        gameObject.GetComponent<BaseCharacter>().RegenStamina = true;
        //    }
        //}

        //charcter animation based on movement direction
        charAnimation();

        // pass all parameters to the controling scripts
        myCarmera.GetComponent<ThirdPCamera>().moveCamera(rotationX, rotationY, zoom);
        m_Character.Move(v, h, myCarmera.GetComponent<ThirdPCamera>().transform.rotation, crouch, m_Jump, running);
        m_Jump = false;
    }

   /// <summary>
   /// Chooses which animation should be played based on how the character is moving.
   /// </summary>
    private void charAnimation()
    {
        if (m_Character.m_IsGrounded)
        {
            playing = false;

            
        }//end check if grounded

        //jump animation
        if (m_Jump && !playing)
        {
            playing = true;
            
        }

        if(combatCounter > waitTime)
        {
            attacking = false;
            combatCounter = 0;
        }
        if(rollCounter > 45)
        {
            rolling = false;
            rollCounter = 0;
        }
    }
}//end ThirdPControl

