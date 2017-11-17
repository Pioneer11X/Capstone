using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ThirdPCharacter))]

public class PursuitAndAttack : MonoBehaviour {

    public Transform target;

    NavMeshAgent _navMeshAgent;

    ThirdPCharacter _characterController;

	// Use this for initialization
	void Start () {

        target = GameObject.FindGameObjectWithTag("Player").transform;
        Debug.Assert(null != target);

        Vector3 targetPos = target.position;

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _characterController = GetComponent<ThirdPCharacter>();

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

        if ( Vector3.Distance(transform.position, target.position) <= _characterController.GetAdjustMaxDistance())
        {
            _characterController.BasicCombo();
        }
        else
        {
            Vector3 targetPos = target.position;

            // If the player moves, and the distance b/w your target and their position is >= .. , Recalculate the Path.
            if ( Vector3.Distance(_navMeshAgent.destination, targetPos) >= _characterController.GetAdjustMaxDistance())
            {
                _navMeshAgent.SetDestination(targetPos);
            }

            float h = _navMeshAgent.desiredVelocity.normalized.x;
            float v = _navMeshAgent.desiredVelocity.normalized.z;

            Vector3 rotatDirection = Vector3.RotateTowards(transform.forward, _navMeshAgent.desiredVelocity, _navMeshAgent.speed * Time.deltaTime, 0.0f);
            _characterController.Move(h, v, Quaternion.LookRotation(rotatDirection), false, false, true, false);
        }

        
        
    }
}
