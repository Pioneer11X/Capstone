using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NavMeshAgent))]

/*This is an AI Boss version of the third person character script. 
 * This script is required by the third person control script.
 * Darren Farr 1/25/2018 */

public class Boss : Character
{
    public GameObject subordinates;
    public Transform playerTarg;
    public Transform seekTarg;    // A variable to store the transform of the target to seek.

    public GameObject[] nodes;

    public bool flee;
    private bool wait;
    private int nodePos;
    [SerializeField]
    private float maxBossSensoryRadius;

    protected NavMeshAgent navMeshAgent;      // A Reference to the NavMeshAgent Component attached to the GameObject.

    // Temporary timer variables.
    protected float timer = 0.0f;
    protected float timerLimit = 3.0f;

    // Use this for initialization
    override protected void Start()
    {
        subordinates.SetActive(false);
        base.Start();

        playerTarg = GameObject.FindGameObjectWithTag("Player").transform;

        seekTarg = nodes[0].transform;

        // Assert that the scene has a player tagged with Player.
        Debug.Assert(null != seekTarg);

        navMeshAgent = GetComponent<NavMeshAgent>();
        // Assert that the scene has a player has a Nav Mesh Agent.
        Debug.Assert(null != seekTarg);

        m_combat.SetChar(this);

        // Set the destination for the NavMesh.
        if (null != seekTarg)
        {
            navMeshAgent.SetDestination(target: seekTarg.position);
        }

    }

    // Special update loop for boss
    void Update()
    {
        if (flee)
        {
            if (seekTarg.tag == "LastNode" && Vector3.Distance(transform.position, playerTarg.position) <= maxBossSensoryRadius)
            {
                seekTarg = playerTarg;
            }
        }

        if (null == seekTarg)
        {
            // Play the Idle Animation.
            return;
        }

        if (!wait)
        {
            if (seekTarg.tag == "Player" && Vector3.Distance(transform.position, seekTarg.position) <= maxBossSensoryRadius)
            {
                Debug.Log(Vector3.Distance(transform.position, seekTarg.position));

                if (Vector3.Distance(transform.position, seekTarg.position) <= this.m_combat.GetAdjustMaxDistance())
                {
                    navMeshAgent.isStopped = true;
                    this.m_combat.IsMoving = false;

                    if (null == this.m_combat.CurrentTarget)
                    {
                        this.m_combat.CurrentTarget = seekTarg.gameObject.GetComponent<Character>();
                    }

                    if (timer > timerLimit)
                    {
                        this.m_combat.BasicCombo();
                        timer = 0;
                    }
                }
                else
                {
                    Vector3 targetPos = seekTarg.position;
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
            else if (Vector3.Distance(transform.position, seekTarg.position) < 1 && seekTarg.tag != "Player")
            {
                if (!wait && nodePos != nodes.Length - 1)
                {
                    wait = true;
                    StartCoroutine(WaitAtNode(.1f));
                }
                else
                {
                    // TODO: Play IDLE Animaiton Here.
                    navMeshAgent.isStopped = true;
                    this.m_combat.IsMoving = false;
                }
            }
            else
            {
                Vector3 targetPos = seekTarg.position;
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

        if(!subordinates.activeSelf)
        {
            subordinates.SetActive(true);
        }
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
    { }//end move

    /// <summary>
    /// Make the enemy wait at a node before seeking the next one
    /// </summary>
    /// <param name="delay">how long to wait</param>
    /// <returns></returns>
    IEnumerator WaitAtNode(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Set next node in the list
        if (wait)
        {
            wait = false;
            if (nodePos < nodes.Length - 1)
            {
                nodePos++;
                seekTarg = nodes[nodePos].transform;
            }
        }
    }
}
