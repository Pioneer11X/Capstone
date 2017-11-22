using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AICharacter))]

public class PursuitAndAttack : MonoBehaviour {

    public Transform target;

    [SerializeField]
    private float maxSensoryRadius;

    NavMeshAgent _navMeshAgent;

    AICharacter _characterController;

    float timer = 0.0f;
    float timerLimit = 2.0f;

	// Use this for initialization
	void Start () {

        target = GameObject.FindGameObjectWithTag("Player").transform;
        Debug.Assert(null != target);

        Vector3 targetPos = target.position;

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _characterController = GetComponent<AICharacter>();

        if (null != target)
            _navMeshAgent.SetDestination(targetPos);



    }
	
	// Update is called once per frame
	void Update () {
        // Debug.Log(_navMeshAgent.nextPosition);

        // Vector3 directionToMove = (_navMeshAgent.nextPosition - transform.position);
        /*
         * 
         */

        timer += Time.deltaTime;

        if ( Vector3.Distance(transform.position, target.position) <= maxSensoryRadius)
        {
            if (Vector3.Distance(transform.position, target.position) <= _characterController.m_combat.GetAdjustMaxDistance())
            {

                _navMeshAgent.isStopped = true;
                _characterController.m_combat.IsMoving = false;

                //if (Vector3.Distance(transform.position, target.position) > _characterController.GetAdjustMinDistance())
                //{
                //    _characterController.Adjust();
                //}
                //else
                //{
                //    _characterController.BasicCombo();
                //}

                if ( timer > timerLimit)
                {
                    _characterController.m_combat.BasicCombo();
                    timer = 0;
                }
                

            }
            else
            {
                Vector3 targetPos = target.position;
                _navMeshAgent.isStopped = false;

                // If the player moves, and the distance b/w your target and their position is >= .. , Recalculate the Path.
                if (Vector3.Distance(_navMeshAgent.destination, targetPos) >= _characterController.m_combat.GetAdjustMaxDistance())
                {
                    _navMeshAgent.SetDestination(targetPos);
                }

                // TODO: Play the Animation here            
                _characterController.m_combat.IsMoving = true;

                // Vector3 rotatDirection = Vector3.RotateTowards(transform.forward, _navMeshAgent.desiredVelocity, _navMeshAgent.speed * Time.deltaTime, 0.0f);
                // _characterController.Move(h, v, Quaternion.identity, false, false, true, false, true);
            }
        }
        else
        {
            // TODO: Play IDLE Animaiton Here.
            _navMeshAgent.isStopped = true;
            _characterController.m_combat.IsMoving = false;
        }

        

        
        
    }
}
