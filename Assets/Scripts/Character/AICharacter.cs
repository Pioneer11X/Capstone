using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NavMeshAgent))]

/*This is an AI version of the third person character script. 
 * This script is required by the third person control script.
 * Darren Farr 11/08/2017 */

public class AICharacter : Character
{

    public Transform seekTarget;    // A variable to store the transform of the target to seek.

    [SerializeField]
    private float maxSensoryRadius; // A variable to store the maximum sensory radius of the AI.

    NavMeshAgent navMeshAgent;      // A Reference to the NavMeshAgent Component attached to the GameObject.


    // Temporary timer variables.
    float timer = 0.0f;
    float timerLimit = 3.0f;

    // Use this for initialization
    override protected void Start()
    {
        base.Start();

        seekTarget = GameObject.FindGameObjectWithTag("Player").transform;

        // Assert that the scene has a player tagged with Player.
        Debug.Assert(null != seekTarget);

        navMeshAgent = GetComponent<NavMeshAgent>();
        // Assert that the scene has a player has a Nav Mesh Agent.
        Debug.Assert(null != seekTarget);

        m_combat.SetChar(this);

        // Set the destination for the NavMesh.
        if ( null != seekTarget)
        {
            navMeshAgent.SetDestination(target: seekTarget.position);
        }

    }

    void Update()
    {

        if ( null == seekTarget)
        {
            // Play the Idle Animation.
            return;
        }

        if (Vector3.Distance(transform.position, seekTarget.position) <= maxSensoryRadius)
        {
            if (Vector3.Distance(transform.position, seekTarget.position) <= this.m_combat.GetAdjustMaxDistance())
            {
                navMeshAgent.isStopped = true;
                this.m_combat.IsMoving = false;

                if ( null == this.m_combat.CurrentTarget)
                {
                    this.m_combat.CurrentTarget = seekTarget.gameObject.GetComponent<Character>();
                }

                if (timer > timerLimit)
                {
                    this.m_combat.BasicCombo();
                    timer = 0;
                }
            }
            else
            {
                Vector3 targetPos = seekTarget.position;
                this.navMeshAgent.isStopped = false;

                // If the player moves, and the distance b/w your target and their position is >= .. , Recalculate the Path.
                if (Vector3.Distance(navMeshAgent.destination, targetPos) >= this.m_combat.GetAdjustMaxDistance())
                {
                    navMeshAgent.SetDestination(targetPos);
                }

                // TODO: Play the Animation here            
                this.m_combat.IsMoving = true;
            }
        }
        else
        {
            // TODO: Play IDLE Animaiton Here.
            navMeshAgent.isStopped = true;
            this.m_combat.IsMoving = false;
        }

        // Update the Moving State for animating..
        this.m_moving = !(navMeshAgent.isStopped);
        timer += Time.deltaTime;
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

    // Pursuit the Player Function.

}//end of class
