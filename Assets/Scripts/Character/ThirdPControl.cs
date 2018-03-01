using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;
using System.Collections.Generic;

/*This is a third person controller script built on the basis of unity's built in third person controller.
 *The script has been overhualed to work with a third person camera and act in a manner similar to most MMO
 *third person camera control. 
 *Darren Farr 11/08/2017 */

[RequireComponent(typeof(Character))]

public class ThirdPControl : MonoBehaviour
{

    public GameObject myCarmera;
    private GameObject gun;

    [SerializeField] private float mouseRotationFactor; //Set mouse rotation sensitivity

    [SerializeField] private float crossRotationFactor; //Set joystick rotation sensitivity

    //[SerializeField] private float scrollFactor;        //Set scroll zoom sensitivity

    // Default unity names for mouse axes
    public string mouseHorizontalAxisName = "Mouse X";
    public string mouseVerticalAxisName = "Mouse Y";
    //public string scrollAxisName = "Mouse ScrollWheel";

    private ThirdPCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
    private PC playerCharacter;

    private Vector3 m_Move;              // the world-relative desired move direction, calculated from the camForward and user input.
    private Vector3 myCamFwd;

    private bool m_Jump;
    private bool m_running;
    private bool m_sprinting;
    private bool m_hacking;
    private bool sprintCoolDown;

    private bool m_aiming = false;
    private bool m_usedConAim = false;

    //private int jumpCount = 0;

    private float lt;
    private float v = 0;
    private float h = 0;
    private float rotationY = 0;
    private float rotationX = 0;
    private float zoom = 0;

    private float attackButtonTimer = 0;
    private bool attackButtonDown;

    private float specialButtonTimer = 0;
    private bool specialButtonDown;

    private Pause pause;

    private List<GameObject> enemies;
    public GameObject[] enemyArray;
    private int aimTargetIndex;
    private int aimCoolDown;

    private float visionHackCDTimer;
    //[SerializeField] private float visionHackCD;
    //[SerializeField] private float visionHackTime;


    public Transform target;
    //public Transform aimTarget;
    public GameObject aimTarget;
    public GameObject gunTarget;



    public GameObject ghostPrefab;
    //animation controller
    //private Animator anim;



    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // get the third person character ( this should never be null due to require component )
        m_Character = GetComponent<ThirdPCharacter>();
        playerCharacter = GetComponent<PC>();
        //anim = GetComponent<ThirdPCharacter>().charBody.GetComponent<Animator>();

        pause = Pause.Instance;

        enemies = new List<GameObject>();

        aimCoolDown = 0;

        //visionHackCDTimer = visionHackCD;

        sprintCoolDown = false;
        m_hacking = false;

        gun = gameObject.GetComponent<CombatManager>().gun;
    }//end start

    private void Update()
    {
        //if (visionHackCDTimer < visionHackCD)
        //{
        //    visionHackCDTimer += Time.deltaTime;
        //}
        //else {
        //    visionHackCDTimer = visionHackCD;
        //}

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

        if (!m_aiming)
        {
            // Primary Attack
            if (attackButtonDown)
            {
                attackButtonTimer += Time.deltaTime;
            }
            if (CrossPlatformInputManager.GetButtonDown("Attack")) //Button 2
            {
                attackButtonDown = true;
                attackButtonTimer = 0;
            }
            if (CrossPlatformInputManager.GetButtonUp("Attack") && attackButtonTimer <= playerCharacter.MinHoldTime)
            {
                m_Character.m_combat.BasicCombo();
                attackButtonDown = false;
            }
            if ((CrossPlatformInputManager.GetButtonUp("Attack") && attackButtonDown && attackButtonTimer > playerCharacter.MinHoldTime) 
                || (attackButtonTimer >= playerCharacter.MaxHoldTime && attackButtonDown))
            {
                m_Character.m_combat.SpecialCombat();
                attackButtonDown = false;
            }

            // Special (Sword) Attack
            if (specialButtonDown)
            {
                specialButtonTimer += Time.deltaTime;
            }
            if (CrossPlatformInputManager.GetButtonDown("Sword") && playerCharacter.SpecialBar > playerCharacter.LightSwordCost) //Button 3
            {
                specialButtonDown = true;
                specialButtonTimer = 0;
            }
            // Button Press Attack
            if (CrossPlatformInputManager.GetButtonUp("Sword") 
                && specialButtonTimer <= playerCharacter.MinHoldTime && playerCharacter.SpecialBar > playerCharacter.LightSwordCost)
            {
                m_Character.m_combat.SwordCombo();
                specialButtonDown = false;
            }
            // Button Hold Attack
            if (((CrossPlatformInputManager.GetButtonUp("Sword") && specialButtonDown && specialButtonTimer > playerCharacter.MinHoldTime)
                || (specialButtonTimer >= playerCharacter.MaxHoldTime && specialButtonDown)) 
                && playerCharacter.SpecialBar > playerCharacter.HeavySwordCost)
            {
                m_Character.m_combat.SwordSpecialCombat();
                specialButtonDown = false;
            }
        }
        else if(m_aiming)
        {
            if(!gun.activeSelf)
            {
                gun.SetActive(true);
            }
            // Gun Attack
            if (attackButtonDown)
            {
                attackButtonTimer += Time.deltaTime;
            }
            if (CrossPlatformInputManager.GetButtonDown("Attack")) //Button 2
            {
                attackButtonDown = true;
                attackButtonTimer = 0;
            }
            if (CrossPlatformInputManager.GetButtonUp("Attack") && playerCharacter.SpecialBar > playerCharacter.GunShootCost)
            {
                playerCharacter.UseSpecial(playerCharacter.GunShootCost);
                m_Character.m_combat.GunShot();
                attackButtonDown = false;
            }
        }


        if (CrossPlatformInputManager.GetButtonDown("Dodge") && playerCharacter.StaminaBar > playerCharacter.DodgeCost) //Button 1
        {
            if (!m_aiming)
            {
                // TODO
                // If can roll
                if (m_Character.m_combat.Roll())
                {
                    // Remove stamina
                    playerCharacter.UseStamina(playerCharacter.DodgeCost);
                }
            }
            else
            {
                if (h < 0)  // Left
                {
                    if(m_Character.m_combat.Dodge(0))
                    {
                        // Remove stamina
                        playerCharacter.UseStamina(playerCharacter.DodgeCost);
                    }
                }
                else if (h > 0) // Right
                {
                    if(m_Character.m_combat.Dodge(1))
                    {
                        // Remove stamina
                        playerCharacter.UseStamina(playerCharacter.DodgeCost);
                    }
                }
                else if (v < 0)  // Back
                {
                    if(m_Character.m_combat.Dodge(2))
                    {
                        // Remove stamina
                        playerCharacter.UseStamina(playerCharacter.DodgeCost);
                    }
                }
                else if (v > 0)  // Foward
                {
                    if(m_Character.m_combat.Dodge(3))
                    {
                        // Remove stamina
                        playerCharacter.UseStamina(playerCharacter.DodgeCost);
                    }
                }
                else
                {
                    Debug.Log("Why is roll / dodge falling into this else statement???");
                }
            }
        }

        if (CrossPlatformInputManager.GetButtonDown("Hack")) //Button 4
        {
            //if (visionHackCDTimer == visionHackCD)
            if (playerCharacter.SpecialBar > 20 && !m_hacking)
            {
                Debug.Log("Vision Hack");
                m_hacking = true;
                StartVisionHack();
            }
            else {
                Debug.Log("Cooling Down");
            }
            
        }
        else
        {
            m_hacking = false;
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

        if (Input.GetKeyDown(KeyCode.B) && !m_aiming)
        {
            if(AimList())
            {
                m_Character.CurrentState = ThirdPCharacter.CharacterState.draw_Gun;
                m_aiming = true;
                m_Character.m_combat.IsAimming = true;
                gun.SetActive(true);
            }
            
            
        }
        if (Input.GetKeyUp(KeyCode.B) && m_aiming)
        {
            m_aiming = false;
            m_Character.m_combat.IsAimming = false;
            myCarmera.GetComponent<ThirdPCamera>().SetAimState(false);
            enemies.Clear();
            m_Character.CurrentState = ThirdPCharacter.CharacterState.holster_Gun;
            UnHighlightEnemies();
            StartCoroutine(GunHolsterDelay());
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
        if( v > 0.001f && v < 0.5f) { v = 0.5f; }
        else if (v < -0.001f && v > -0.5f) { v = -0.5f; }
        if (h > 0.001f && h < 0.5f) { h = 0.5f; }
        else if (h < -0.001f && h > -0.5f) { h = -0.5f; }

        // Triggers
        float rt = CrossPlatformInputManager.GetAxis("Dash");
        if ((rt != 0 || Input.GetKey(KeyCode.LeftShift)) && playerCharacter.StaminaBar > 0 
            && !sprintCoolDown && (v != 0 || h != 0) )
        {
            playerCharacter.UseStamina(playerCharacter.SprintCost);
            m_sprinting = true;
            
        }
        else if(!m_hacking && !m_Character.m_combat.IsRolling)
        {
            m_sprinting = false;
        }
        if(playerCharacter.StaminaBar < (playerCharacter.SprintCost * 2) )
        {
            sprintCoolDown = true;
        }
        else if (playerCharacter.StaminaBar > 10)
        {
            sprintCoolDown = false;
        }

        lt = CrossPlatformInputManager.GetAxis("Aim");
        if (lt != 0)    // May need a threshold
        {
            if (!m_aiming)
            {
                if (AimList())
                {
                    m_Character.CurrentState = ThirdPCharacter.CharacterState.draw_Gun;
                    m_aiming = true;
                    m_Character.m_combat.IsAimming = true;

                    // TODO
                    // Find nearest enemy in front of the player and set as aim target

                    gun.SetActive(true);
                    m_usedConAim = true;
                }
            }
        }
        else if(m_aiming && m_usedConAim) // TODO change to work better with controller and some form of error range
        {
            m_aiming = false;
            m_usedConAim = false;
            m_Character.m_combat.IsAimming = false;
            myCarmera.GetComponent<ThirdPCamera>().SetAimState(false);
            enemies.Clear();
            m_Character.CurrentState = ThirdPCharacter.CharacterState.holster_Gun;
            UnHighlightEnemies();
            StartCoroutine(GunHolsterDelay());
        }

        // D-Pad goes here
        /*
        if (CrossPlatformInputManager.GetAxis("dpX") > 0)
        { Debug.Log("D-Pad Right"); }
        if (CrossPlatformInputManager.GetAxis("dpX") < 0)
        { Debug.Log("D-Pad Left"); }
        if (CrossPlatformInputManager.GetAxis("dpY") > 0)
        { Debug.Log("D-Pad Up"); }
        if (CrossPlatformInputManager.GetAxis("dpY") < 0)
        { Debug.Log("D-Pad Down"); }
        */


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

        // pass all parameters to the controling scripts
        if (!m_aiming)
        {
            myCarmera.GetComponent<ThirdPCamera>().moveCamera(rotationX, rotationY, zoom);
        }
        else
        {
            // Change aim target if one is available based on joystick/mouse movement.
            if (enemyArray.Length > 1)
            {
                // Is current target is dead, get new one
                if (!gunTarget.activeSelf)
                {
                    float dist = float.MaxValue;
                    List<GameObject> tempArray = new List<GameObject>(enemyArray.Length);

                    // Adjust the array to remove the dead enemy
                    for (int i = 0; i < enemyArray.Length; i++)
                    {
                        if(enemyArray[i].activeSelf)
                        {
                            enemyArray[i].GetComponentInChildren<SkinnedMeshRenderer>().material = enemyArray[i].GetComponent<Enemy>().defaultMat;
                            tempArray.Add(enemyArray[i]);
                        }
                    }
                    enemyArray = tempArray.ToArray();
                    
                    // Set the closet enemy as the target
                    for (int i = 0; i < enemyArray.Length; i++)
                    {
                        float temp = Vector3.Distance(enemyArray[i].transform.position, transform.position);
                        if (temp < dist)
                        {
                            dist = temp;
                            gunTarget = enemyArray[i];
                            aimTargetIndex = i;
                        }
                    }
                    SetAimTarget(aimTargetIndex);
                }

                // Switch Aim Target
                if (rotationX > 1 && aimCoolDown < 1)
                {
                    aimTargetIndex++;
                    if (aimTargetIndex == enemyArray.Length)
                    {
                        aimTargetIndex--;
                    }
                    else
                    {
                        SetAimTarget(aimTargetIndex);
                        enemyArray[aimTargetIndex - 1].GetComponentInChildren<SkinnedMeshRenderer>().material = enemyArray[aimTargetIndex - 1].GetComponent<Enemy>().defaultMat;
                        aimCoolDown = 16;
                    }
                }
                else if (rotationX < -1 && aimCoolDown < 1)
                {
                    aimTargetIndex--;
                    if (aimTargetIndex < 0)
                    {
                        aimTargetIndex = 0;
                    }
                    else
                    {
 
                        SetAimTarget(aimTargetIndex);
                        enemyArray[aimTargetIndex + 1].GetComponentInChildren<SkinnedMeshRenderer>().material = enemyArray[aimTargetIndex + 1].GetComponent<Enemy>().defaultMat;
                        aimCoolDown = 16;
                    }
                }


            }
            else if(enemyArray.Length == 1 && !gunTarget.activeSelf)
            {
                // If there are no enemies left to aim at kick the player out of aim mode
                m_aiming = false;
                m_usedConAim = false;
                m_Character.m_combat.IsAimming = false;
                myCarmera.GetComponent<ThirdPCamera>().SetAimState(false);
                enemies.Clear();
                m_Character.CurrentState = ThirdPCharacter.CharacterState.holster_Gun;
                UnHighlightEnemies();
                StartCoroutine(GunHolsterDelay());
            }

            
            if(aimCoolDown > 0)
            {
                aimCoolDown--;
            }

            // Tell the animator which direction the character is moving in
            if (h < 0)  // Left
            {
                m_Character.m_combat.AimMove(0);
            }
            else if (h > 0) // Right
            {
                m_Character.m_combat.AimMove(1);
            }
            else if (v < 0)  // Back
            {
                m_Character.m_combat.AimMove(2);
            }
            else if (v > 0)  // Foward
            {
                m_Character.m_combat.AimMove(3);
            }
            else
            {
                m_Character.m_combat.AimMove(-1);
            }
        }

        m_Character.Move(v, h, myCarmera.GetComponent<ThirdPCamera>().transform.rotation,
            m_Jump, m_running, m_sprinting, m_aiming);

        m_Jump = false;
    }

    /// <summary>
    /// Make a list of enemy targets for aiming
    /// </summary>
    /// <returns>True if a target is found</returns>
    private bool AimList()
    {
        enemies.Clear();
        bool done = false;
        enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
        //GameObject target = null;

        UnHighlightEnemies();

        // Get the enemies in front of me
        for (int i = 0; i < enemyArray.Length; i++)
        {
            float fwdDot = Vector3.Dot((transform.position - enemyArray[i].transform.position), transform.forward);
            float distance = Vector3.Distance(transform.position, enemyArray[i].transform.position);
            if (fwdDot < -1 && distance < 20)
            {
                enemies.Add(enemyArray[i]);
            }
        }

        enemyArray = enemies.ToArray();

        if (enemyArray.Length > 0)
        {
            // Sort enemies from left to right
            while (!done)
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
                if(dot < tempDot)
                {
                    tempDot = dot;
                    aimTargetIndex = i;
                    gunTarget = enemyArray[i];
                    aimTarget = gunTarget.transform.GetChild(0).gameObject;
                }
            }

        
            myCarmera.GetComponent<ThirdPCamera>().SetAimState(true, aimTarget);
            m_Character.m_combat.AimTarget = aimTarget.GetComponentInParent<Character>();
            gunTarget.GetComponentInChildren<SkinnedMeshRenderer>().material = gunTarget.GetComponent<Enemy>().highlightMat;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Vision Hack Ability
    /// Create a ghost of the player and let them move it while recording
    /// </summary>
    void StartVisionHack() {
        //record cam pos

        //create a ghost
        GameObject ghost = Instantiate(ghostPrefab, transform.position, transform.rotation) as GameObject;
        //init the ghost
        GhostController gc = ghost.GetComponent<GhostController>();
        gc.Init(this, myCarmera);
        //set cam
        //myCarmera.GetComponent<ThirdPCamera>().ChangeTarget(gc.target, gc.aimTarget);
        //disappear
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Stop vision hack and return to player
    /// </summary>
    public void EndVisionHack() {
        //visionHackCDTimer = 0;
        gameObject.SetActive(true);
        m_Character.camera = myCarmera;
    }


    IEnumerator GunHolsterDelay()
    {
        yield return new WaitForSeconds(1);

        gun.SetActive(false);
    }

    /// <summary>
    /// Reset Enemy material to the defualt
    /// </summary>
    private void UnHighlightEnemies()
    {
        for (int i = 0; i < enemyArray.Length; i++)
        {
            enemyArray[i].GetComponentInChildren<SkinnedMeshRenderer>().material = enemyArray[i].GetComponent<Enemy>().defaultMat;
        }
    }

    /// <summary>
    /// Set the player's aim target
    /// </summary>
    /// <param name="index">Enemy array index</param>
    private void SetAimTarget(int index)
    {
        gunTarget = enemyArray[index];
        aimTarget = gunTarget.transform.GetChild(0).gameObject;

        myCarmera.GetComponent<ThirdPCamera>().SetAimState(true, aimTarget);
        m_Character.m_combat.AimTarget = aimTarget.GetComponentInParent<Character>();
        gunTarget.GetComponentInChildren<SkinnedMeshRenderer>().material = gunTarget.GetComponent<Enemy>().highlightMat;
    }


}//end ThirdPControl

