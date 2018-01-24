using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections.Generic;

public class GhostController : MonoBehaviour
{
    // Use this for initialization

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
    private PC playerCharacter;

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
    //private int jumpCount = 0;
    private int waitTime = 45;

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

    public struct MoveRecord
    {
        public int frameCount;
        public float v;
        public float h;
        public bool jump;
        public Quaternion camRotation;
    };
    private Vector3 oriPos;
    private Quaternion oriRot;
    private List<MoveRecord> records = new List<MoveRecord>();
    private bool isReplay;
    private int frameCount;
    private int deadCount;

    private void Start()
    {
        // get the third person character ( this should never be null due to require component )
        m_Character = GetComponent<ThirdPCharacter>();
        playerCharacter = GetComponent<PC>();
        //anim = GetComponent<ThirdPCharacter>().charBody.GetComponent<Animator>();

        pause = Pause.Instance;

        enemies = new List<GameObject>();

        aimCoolDown = 60;

        oriPos = transform.position;
        oriRot = transform.rotation;
        //set the target of camera to this

        //
        isReplay = false;
        frameCount = 1;
    }//end start

    private void End()
    {
        //release the player gameobject

        //set back the camera

        //return to the init pos
        transform.position = oriPos;
        transform.rotation = oriRot;
        //start to replay
        isReplay = true;
        deadCount = frameCount;
        frameCount = 1;
    }

    private void Update()
    {
        if (!isReplay)
        {
            if (!pause.IsPaused)
            {
                Time.timeScale = 1;
                myCarmera.SetActive(true);
            }
            else // Is Paused
            {
                Time.timeScale = 0;
                myCarmera.SetActive(false);
            }

            //if (!m_aiming)
            //{
            //    // Primary Attack
            //    if (attackButtonDown)
            //    {
            //        attackButtonTimer += Time.deltaTime;
            //    }

            //    // Special (Sword) Attack
            //    if (specialButtonDown)
            //    {
            //        specialButtonTimer += Time.deltaTime;
            //    }

            //    // Button Press Attack

            //    // Button Hold Attack

            //}
            //else if (m_aiming)
            //{
            //    // Gun Attack
            //    if (attackButtonDown)
            //    {
            //        attackButtonTimer += Time.deltaTime;
            //    }

            //}

            //check to see if the character should jump
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
            MoveRecord mr = new MoveRecord() { frameCount = frameCount, h = h, v = v, jump = m_Jump, camRotation = myCarmera.GetComponent<ThirdPCamera>().transform.rotation };
            records.Add(mr);
            frameCount++;
            if (frameCount == 500)
            {
                End();
            }
        }
        else
        {
            frameCount++;
            charAnimation();
            if (frameCount == deadCount)
            {
                Destroy(gameObject);
            }
        }


    }//end update

    // Fixed update is called in sync with physics
    // This controls the character's and camera's movement
    private void FixedUpdate()
    {
        if (!isReplay)
        {
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

            // Triggers

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
                if (enemyArray.Length > 1)
                {
                    if (rotationX > 1 && aimCoolDown > 29)
                    {
                        aimTargetIndex++;
                        if (aimTargetIndex == enemyArray.Length)
                        {
                            aimTargetIndex--;
                        }
                        myCarmera.GetComponent<ThirdPCamera>().SetAimState(true, enemyArray[aimTargetIndex]);
                    }
                    else if (rotationX < -1 && aimCoolDown > 29)
                    {
                        aimTargetIndex--;
                        if (aimTargetIndex < 0)
                        {
                            aimTargetIndex = 0;
                        }
                        myCarmera.GetComponent<ThirdPCamera>().SetAimState(true, enemyArray[aimTargetIndex]);
                    }
                }
                aimCoolDown--;
                if (aimCoolDown < 1)
                {
                    aimCoolDown = 30;
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
                m_crouch, m_Jump, m_running, m_dashing, m_aiming);
            //records[frameCount].jump = 
            m_Jump = false;
        }
        else {
            h = records[frameCount].h;
            v = records[frameCount].v;
            m_Jump = records[frameCount].jump;
            Quaternion r = records[frameCount].camRotation;
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

            m_Character.Move(v, h, r, m_crouch, m_Jump, m_running, m_dashing, m_aiming);
            m_Jump = false;
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

        if (combatCounter > waitTime && m_attacking)
        {
            m_attacking = false;
            combatCounter = 0;
        }
        if (rollCounter > 45 && m_rolling)
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
            for (int i = 0; i < enemyArray.Length; i++)
            {

                float dot = Vector3.Dot((transform.position - enemyArray[i].transform.position), transform.forward);
                if (dot < tempDot)
                {
                    tempDot = dot;
                    aimTargetIndex = i;
                    aimTarget = enemyArray[i].transform.GetChild(0).gameObject;
                }
            }


            myCarmera.GetComponent<ThirdPCamera>().SetAimState(true, aimTarget);
            m_Character.m_combat.AimTarget = aimTarget.GetComponentInParent<Character>();
        }
    }
}//end

