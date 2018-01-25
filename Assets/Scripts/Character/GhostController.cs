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
    private bool m_playing = false;


    private float v = 0;
    private float h = 0;
    private float rotationY = 0;
    private float rotationX = 0;
    private float zoom = 0;

    private Pause pause;

    private float maxVisionHackTime;
    private float visionHackTimer;

    private ThirdPControl player;

    public Transform target;
    public Transform aimTarget;

    private Vector3 camPos;
    private Quaternion camRot;


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

        m_Character.camera = myCarmera;

        oriPos = transform.position;
        oriRot = transform.rotation;
        //set the target of camera to this

        //
        isReplay = false;
        frameCount = 1;
        visionHackTimer = 0;
    }//end start

    public void Init(float time,ThirdPControl p,GameObject camera) {
        maxVisionHackTime = time;
        player = p;
        myCarmera = camera;
        camPos = myCarmera.transform.position;
        camRot = myCarmera.transform.rotation;
    }

    private void End()
    {
        //release the player gameobject
        player.EndVisionHack();
        //set back the camera
        myCarmera.transform.position = camPos;
        myCarmera.transform.rotation = camRot;
        myCarmera.GetComponent<ThirdPCamera>().ChangeTarget(player.target, player.aimTarget);
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
            visionHackTimer += Time.deltaTime;

            if (visionHackTimer >= maxVisionHackTime || !CrossPlatformInputManager.GetButton("Hack")) {
                End();
            }

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

          

            //check to see if the character should jump
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
            MoveRecord mr = new MoveRecord() { frameCount = frameCount, h = h, v = v, jump = m_Jump, camRotation = myCarmera.GetComponent<ThirdPCamera>().transform.rotation };
            records.Add(mr);
            frameCount++;
       
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


            myCarmera.GetComponent<ThirdPCamera>().moveCamera(rotationX, rotationY, zoom);

            //charcter animation based on movement direction
            charAnimation();

            // pass all parameters to the controling scripts
           
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
            m_Character.Move(v, h, myCarmera.GetComponent<ThirdPCamera>().transform.rotation,
                false, m_Jump, m_running, false, false);
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

            m_Character.Move(v, h, r, false, m_Jump, m_running, false, false);
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

    }

}//end

