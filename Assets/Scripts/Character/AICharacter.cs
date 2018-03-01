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
    private Transform playerTarget;
    public Transform ghostTarget;

    [SerializeField]
    private float maxSensoryRadius; // A variable to store the maximum sensory radius of the AI.

    protected NavMeshAgent navMeshAgent;      // A Reference to the NavMeshAgent Component attached to the GameObject.


    // Temporary timer variables.
    protected float timer = 0.0f;

    public ActionSelector s_action;

    // Use this for initialization
    override protected void Start()
    {
        base.Start();

        s_action = GetComponent<ActionSelector>();

        seekTarget = playerTarget = GameObject.FindGameObjectWithTag("Player").transform;

        // Assert that the scene has a player tagged with Player.
        Debug.Assert(null != seekTarget);

        navMeshAgent = GetComponent<NavMeshAgent>();
        // Assert that the scene has a player has a Nav Mesh Agent.
        Debug.Assert(null != navMeshAgent);

        m_combat.SetChar(this);

        // Set the destination for the NavMesh.
        if ( null != seekTarget)
        {
            navMeshAgent.SetDestination(target: seekTarget.position);
        }

    }

    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Ghost") != null)
        {
            ghostTarget = GameObject.FindGameObjectWithTag("Ghost").transform;
            seekTarget = ghostTarget;
        }
        else
        {
            seekTarget = playerTarget;
        }
        if ( null == seekTarget)
        {
            // Play the Idle Animation.
            return;
        }

        if (this.m_combat.IsTurning && (null != m_combat.CurrentTarget))
        {
            var direc = this.m_combat.CurrentTarget.transform.position - transform.position;
            var rot = Quaternion.LookRotation(direc, transform.TransformDirection(Vector3.up));
            float angle = Quaternion.Angle(transform.rotation, new Quaternion(0, rot.y, 0, rot.w));
            if (angle < 20) { this.m_combat.IsTurning = false; }
            else
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, new Quaternion(0, rot.y, 0, rot.w), turnSpeed * Time.deltaTime);
            }
        }

        if (this.m_combat.IsAdjusting)
        {
            // సరి చేసుకొని, ముందుకో వెనక్కో వెళ్ళు. అంతే.
            // Well... Adjust.. and adjust only.. Do not move do not look to perform the next action..
            float distanceToTarget = Vector3.Distance(this.transform.position, this.m_combat.CurrentTarget.transform.position);
            if ( distanceToTarget > this.m_combat.GetAdjustMaxDistance())
            {
                ForceMove(1.0f, 1);
            }

            if ( distanceToTarget < this.m_combat.GetAdjustMinDistance())
            {
                ForceMove(1.0f, -1);
            }

            if (!(distanceToTarget > this.m_combat.GetAdjustMaxDistance() || distanceToTarget < this.m_combat.GetAdjustMinDistance()))
            {
                this.m_combat.IsAdjusting = false;
            }

            // Also Check if the adjusting is done..
        }



        if (ghostTarget != null && Vector3.Distance(transform.position, ghostTarget.position) <= maxSensoryRadius)
        {
            if (Vector3.Distance(transform.position, seekTarget.position) <= this.m_combat.GetAdjustMaxDistance())
            {
                navMeshAgent.isStopped = true;
                this.m_combat.IsMoving = false;

                if (null == this.m_combat.CurrentTarget)
                {
                    this.m_combat.CurrentTarget = seekTarget.gameObject.GetComponent<Character>();
                }

                if (timer > m_combat.TimeBetweenAttacks)
                {
                    // Only call this when we aren't stunned..
                    // మనం కొట్టగలమో లేదో చూడాలి.
                    if (!this.m_combat.IsHit)
                    {
                        this.s_action.selectNextOption();
                        timer = 0;
                    } 
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
        else if (Vector3.Distance(transform.position, seekTarget.position) <= maxSensoryRadius)
        {
            if (Vector3.Distance(transform.position, seekTarget.position) <= this.m_combat.GetAdjustMaxDistance())
            {

                navMeshAgent.isStopped = true;
                this.m_combat.IsMoving = false;

                if ( null == this.m_combat.CurrentTarget) 
                {
                    this.m_combat.CurrentTarget = seekTarget.gameObject.GetComponent<Character>();
                }

                if (timer > m_combat.TimeBetweenAttacks)
                {
                    // TODO: Use the Action Selector here. Select an Item and then, reduce the preference.
                    // this.m_combat.BasicCombo();
                    // this.m_combat.BasicCombo();
                    this.s_action.selectNextOption();
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
    /// <param name="jump">should player jump</param>
    /// <param name="running">is the player running</param>
    /// <param name="dash">is the player dashing</param>
    override public void Move(float vert, float hori, Quaternion camRot, bool jump, bool running, bool dash, bool aiming)
    {}//end move

    // Pursuit the Player Function.

}//end of class
