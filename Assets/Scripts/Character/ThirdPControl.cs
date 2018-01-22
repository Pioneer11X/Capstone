using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections.Generic;

/*This is a third person controller script built on the basis of unity's built in third person controller.
 *The script has been overhualed to work with a third person camera and act in a manner similar to most MMO
 *third person camera control. 
 *Darren Farr 11/08/2017 */

[RequireComponent(typeof(Character))]

public class ThirdPControl : MonoBehaviour
{

    public GameObject myCarmera;

    [SerializeField]
    private float mouseRotationFactor; //Set mouse rotation sensitivity

    [SerializeField]
    private float crossRotationFactor; //Set mouse rotation sensitivity

    [SerializeField]
    private float scrollFactor;        //Set scroll zoom sensitivity
    [SerializeField]
    private int dashMod = 60;

    // Default unity names for mouse axes
    public string mouseHorizontalAxisName = "Mouse X";
    public string mouseVerticalAxisName = "Mouse Y";
    public string scrollAxisName = "Mouse ScrollWheel";

    private ThirdPCharacter m_Character; // A reference to the ThirdPersonCharacter on the object

    private Vector3 m_Move;              // the world-relative desired move direction, calculated from the camForward and user input.
    private Vector3 myCamFwd;

    private bool m_Jump;
    private bool m_running;
    private bool m_dashing;
    private bool m_useDash;
    private bool m_playing = false;

    private bool m_crouch = false;
    private bool m_attacking = false;
    private bool m_rolling = false;
    private bool m_aiming = false;
    private bool m_usedConAim = false;

    private int combatCounter = 0;
    private int dashCounter = 0;
    private int rollCounter = 0;
    private int jumpCount = 0;
    private int waitTime = 45;

    private float lt;
    private float v = 0;
    private float h = 0;
    private float rotationY = 0;
    private float rotationX = 0;
    private float charRotationX = 0;
    private float zoom = 0;

    private float attackButtonTimer = 0;
    private bool attackButtonDown;

    private float specialButtonTimer = 0;
    private bool specialButtonDown;

    [SerializeField]
    private float minHoldTime;

    [SerializeField]
    private float maxHoldTime;

    private Pause pause;

    private List<GameObject> enemies;
    public GameObject[] enemyArray;
    private int aimTargetIndex;
    private int aimCoolDown;

    //animation controller
    //private Animator anim;



    private void Start()
    {
        // get the third person character ( this should never be null due to require component )
        m_Character = GetComponent<ThirdPCharacter>();
        //anim = GetComponent<ThirdPCharacter>().charBody.GetComponent<Animator>();

        pause = Pause.Instance;

        enemies = new List<GameObject>();

        aimCoolDown = 60;
    }//end start

    private void Update()
    {
        if(!pause.IsPaused)
        {
            Time.timeScale = 1;
            myCarmera.SetActive(true);
        }
        else // Is Paused
        {
            Time.timeScale = 0;
            myCarmera.SetActive(false);
        }

        // Primary Attack
        if (attackButtonDown) {
            attackButtonTimer += Time.deltaTime;
        }
        if (CrossPlatformInputManager.GetButtonDown("Attack")) //Button 2
        {
            attackButtonDown = true;
            attackButtonTimer = 0;            
        }
        if (CrossPlatformInputManager.GetButtonUp("Attack") && attackButtonTimer<= minHoldTime) {
            m_Character.m_combat.BasicCombo();
            attackButtonDown = false;
        }
        if ((CrossPlatformInputManager.GetButtonUp("Attack") && attackButtonDown && attackButtonTimer > minHoldTime) ||(attackButtonTimer>=maxHoldTime && attackButtonDown))
        {
            m_Character.m_combat.SpecialCombat();
            attackButtonDown = false;
        }

        // Special (Sword) Attack
        if (specialButtonDown)
        {
            specialButtonTimer += Time.deltaTime;
        }
        if (CrossPlatformInputManager.GetButtonDown("Sword")) //Button 3
        {
            specialButtonDown = true;
            specialButtonTimer = 0;
        }
        // Button Press Attack
        if (CrossPlatformInputManager.GetButtonUp("Sword") && specialButtonTimer <= minHoldTime)
        {
            m_Character.m_combat.SwordCombo();
            specialButtonDown = false;
        }
        // Button Hold Attack
        if ((CrossPlatformInputManager.GetButtonUp("Sword") && specialButtonDown && specialButtonTimer > minHoldTime) || (specialButtonTimer >= maxHoldTime && specialButtonDown))
        {
            m_Character.m_combat.SwordSpecialCombat();
            specialButtonDown = false;
        }


        if (CrossPlatformInputManager.GetButtonDown("Dodge")) //Button 1
        {
            if (m_Character.CurrentState == ThirdPCharacter.CharacterState.aim)
            {
                if (h < 0)
                {
                    m_Character.m_combat.Dodge(0);
                }
                else if (h > 0)
                {
                    m_Character.m_combat.Dodge(1);
                }
                else
                {
                    // Add for block
                }
            }
            else
            {
                m_Character.m_combat.Roll();
            }
        }

        if (CrossPlatformInputManager.GetButtonDown("Hack")) //Button 4
        {
            Debug.Log("Vision Hack");
        }
        if (CrossPlatformInputManager.GetButtonDown("Interact")) //Button 5
        {
            Debug.Log("Interact");
        }
        if (CrossPlatformInputManager.GetButtonDown("Pause") && !pause.IsPaused) //Button 7
        {
            Debug.Log("Pause");
            pause.IsPaused = true;
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadSceneAsync("Pause", LoadSceneMode.Additive);
        }
        if (CrossPlatformInputManager.GetButtonDown("Submit")) //Button 0
        {
            //Debug.Log("Submit");
        }
        if (CrossPlatformInputManager.GetButtonDown("Cancel")) //Button 1 or 6
        {
            //Debug.Log("Cancel");
        }

        //check to see if the character should jump
        if (!m_Jump)
        {
            m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            //jumpCount++;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) && !m_aiming)
        {
            m_Character.CurrentState = ThirdPCharacter.CharacterState.aim;
            m_aiming = true;
            AimList();
        }
        if (Input.GetKeyUp(KeyCode.LeftControl) && m_aiming)
        {
            m_aiming = false;
            myCarmera.GetComponent<ThirdPCamera>().SetAimState(false);
            enemies.Clear();
        }

    }//end update

    // Fixed update is called in sync with physics
    // This controls the character's and camera's movement
    private void FixedUpdate()
    {
        lt = 0;
        v = 0;
        h = 0;
        rotationY = 0;
        rotationX = 0;
        charRotationX = 0;
        m_running = false;

        // Axis Input

        // Rotate camera
        rotationX = CrossPlatformInputManager.GetAxis("HorizontalJoystick") * crossRotationFactor;
        rotationY = (CrossPlatformInputManager.GetAxis("VerticalJoystick") * -1) * crossRotationFactor;
        if (rotationX == 0) { rotationX = Input.GetAxis(mouseHorizontalAxisName) * mouseRotationFactor; }
        if (rotationY == 0) { rotationY = Input.GetAxis(mouseVerticalAxisName) * mouseRotationFactor; }

        // Character movement (forward/backward motion) (rotate left/right)
        v = CrossPlatformInputManager.GetAxis("Vertical");
        h = CrossPlatformInputManager.GetAxis("Horizontal");

        // Triggers
        float rt = CrossPlatformInputManager.GetAxis("Dash");
        if (!m_useDash && rt != 0)
        {
            m_useDash = true;
            m_dashing = true;
            dashCounter = dashMod;

        }

        lt = CrossPlatformInputManager.GetAxis("Aim");
        if (lt != 0)    // May need a threshold
        {
            if (!m_aiming)
            {
                Debug.Log("Aim");
                m_Character.CurrentState = ThirdPCharacter.CharacterState.aim;
                m_aiming = true;

                // TODO
                // Find nearest enemy in front of the player and set as aim target
                AimList();

                m_usedConAim = true;
            }
        }
        else if(m_aiming && m_usedConAim) // TODO change to work better with controller and some form of error range
        {
            Debug.Log("Stop Aim");
            m_aiming = false;
            myCarmera.GetComponent<ThirdPCamera>().SetAimState(false);
            enemies.Clear();
        }

        // D-Pad goes here
        if (CrossPlatformInputManager.GetAxis("dpX") > 0)
        { Debug.Log("D-Pad Right"); }
        if (CrossPlatformInputManager.GetAxis("dpX") < 0)
        { Debug.Log("D-Pad Left"); }
        if (CrossPlatformInputManager.GetAxis("dpY") > 0)
        { Debug.Log("D-Pad Up"); }
        if (CrossPlatformInputManager.GetAxis("dpY") < 0)
        { Debug.Log("D-Pad Down"); }

        // Face Buttons


        //scroll for zoom
        //zoom = Input.GetAxis(scrollAxisName) * scrollFactor;

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
        //if (v != 0)
        //{
        //    m_Character.unFreezeChar();
        //}

        //character crouch
        if (Input.GetKeyDown(KeyCode.C))
        {
            m_crouch = !m_crouch;
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
        if (Input.GetKey(KeyCode.LeftShift) && !m_crouch)
        {
            //if (gameObject.GetComponent<BaseCharacter>().StaminaPoints > 0)
            //{
            m_running = true;
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
        if (!m_aiming)
        {
            myCarmera.GetComponent<ThirdPCamera>().moveCamera(rotationX, rotationY, zoom);
        }
        else
        {
            // Change aim target if one is available based on joystick/mouse movement.
            if(rotationX > 1 && aimCoolDown > 29)
            {
                aimTargetIndex++;
                if(aimTargetIndex == enemyArray.Length)
                {
                    aimTargetIndex--;
                }
                myCarmera.GetComponent<ThirdPCamera>().SetAimState(true, enemyArray[aimTargetIndex]);
            }
            else if(rotationX < -1 && aimCoolDown > 29)
            {
                aimTargetIndex--;
                if (aimTargetIndex < 0)
                {
                    aimTargetIndex = 0;
                }
                myCarmera.GetComponent<ThirdPCamera>().SetAimState(true, enemyArray[aimTargetIndex]);
            }
            aimCoolDown--;
            if(aimCoolDown < 1)
            {
                aimCoolDown = 30;
            }
        }

        m_Character.Move(v, h, myCarmera.GetComponent<ThirdPCamera>().transform.rotation,
            m_crouch, m_Jump, m_running, m_dashing, m_aiming);
        m_Jump = false;
        if (m_useDash)
        {
            dashCounter--;
            if (dashCounter <= (dashMod / 4) * 3)
            { m_dashing = false; }
            if (dashCounter <= 0)
            { m_useDash = false; }
        }
    }

    /// <summary>
    /// Chooses which animation should be played based on how the character is moving.
    /// </summary>
    private void charAnimation()
    {
        if (m_Character.m_IsGrounded)
        {
            m_playing = false;


        }//end check if grounded

        //jump animation
        if (m_Jump && !m_playing)
        {
            m_playing = true;

        }

        if (combatCounter > waitTime)
        {
            m_attacking = false;
            combatCounter = 0;
        }
        if (rollCounter > 45)
        {
            m_rolling = false;
            rollCounter = 0;
        }
    }

    // Make a list of enemy targets for aiming
    private void AimList()
    {
        enemies.Clear();
        bool done = false;
        enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject aimTarget = null;

        // Get the enemies in front of me
        for (int i = 0; i < enemyArray.Length; i++)
        {
            float fwdDot = Vector3.Dot((transform.position - enemyArray[i].transform.position), transform.forward);
            if (fwdDot < -17)
            {
                enemies.Add(enemyArray[i]);
                //Debug.Log(fwdDot);
            }
        }

        enemyArray = enemies.ToArray();

        // Sort enemies from left to right
        while(!done)
        {
            done = true;
            for (int i = 0; i < enemyArray.Length - 1; i++)
            {
                float distTo = Vector3.Dot((transform.position - enemyArray[i].transform.position), transform.right);
                float distTo2 = Vector3.Dot((transform.position - enemyArray[i + 1].transform.position), transform.right);
                
                if (distTo2 > distTo)
                {
                    done = false;
                    GameObject temp = enemyArray[i + 1];
                    enemyArray[i + 1] = enemyArray[i];
                    enemyArray[i] = temp;
                }
            }
        }

        float tempDot = 0;
        // Set the aim target
        // Use the enemy most centered in the view
        for(int i = 0; i < enemyArray.Length; i++)
        {
            
            float dot = Vector3.Dot((transform.position - enemyArray[i].transform.position), transform.forward);
            if(dot < tempDot && dot > -25)
            {
                tempDot = dot;
                aimTargetIndex = i;
                aimTarget = enemyArray[i].transform.GetChild(0).gameObject;
            }
        }

        myCarmera.GetComponent<ThirdPCamera>().SetAimState(true, aimTarget);
    }
}//end ThirdPControl

