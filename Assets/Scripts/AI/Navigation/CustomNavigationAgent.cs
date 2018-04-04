﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ఇది మనం Unity NavMeshAgent లాగా వాడాలి.
// This should be aligned as close to the Unitys NavMesh Agent as possible.
public class CustomNavigationAgent : MonoBehaviour
{

    [SerializeField] private float AISpeedMod;

    public Vector3 destination;
    public LayerMask targetLayer;

    // This location is used to go back once you lose the target.
    // Player బాగాదూరంగావెళ్ళిపోతేంమనంవెనక్కిరావటానికివుంటుంది.
    public Vector3 initialLocation;

    [SerializeField]
    private List<PathingNode> path;

    // This is to check if we have to reCalculate the Path or not.
    // మనకి ఒకో సారి మళ్ళీ మళ్ళీ కనుక్కోవాల్సిన అవసరం లేదు.
    public bool reCalculatePath = false;

    // Our Current Node
    // మనం ఇప్పుడు వున్న Node.
    [SerializeField]
    private PathingNode currentNode;

    // మనం వెళ్ళాల్సిన Node
    [SerializeField]
    private PathingNode targetNode;

    private Vector3 rayCastSourcePoint;
    private Vector3 rayCastTargetPoint;

    // ఆగున్నామా?
    // Similar to the Unity NavMeshAgent.
    public bool isStopped;

    [SerializeField] private float rayCastHeight;

    public bool GetIsStopped()
    {
        return isStopped;
    }

    public void SetIsStopped(bool _isStopped)
    {
        isStopped = _isStopped;
        if ( true == isStopped)
        {
            canTraverseDirectly = false;
        }
    }

    // Dummy Variable for debugging.
    // ఇది వుత్తినే పెట్టాం.
    [SerializeField]
    bool canTraverseDirectly;
    [SerializeField]
    RaycastHit hitInfo;

    // Reference
    private AICharacter aICharacter;
    private float maxSensoryRadius = 0;

    private void Awake()
    {
        
    }

    // Use this for initialization
    void Start () {

        initialLocation = this.transform.position;

        aICharacter = this.GetComponent<AICharacter>();
        Debug.Assert(null != aICharacter);

        maxSensoryRadius = aICharacter.GetMaxSensoryRadius();
        Debug.Assert(0.5 < maxSensoryRadius);
	}

    void calculatePath()
    {

        targetNode = NavigationSingleton.Instance.GetCurrentNode(destination);

        Debug.Assert(null != currentNode);
        Debug.Assert(null != targetNode);

        // This is an infinite loop waiting for the canCalculate Flag to be set.
        // దారి కనుక్కోగలిగే వరకూ ఆగు. ఇది ఎంత మంచిదన్నది తెలియదు.
        while (!NavigationSingleton.Instance.canCalculte)
        {
            continue;
        }
        path = NavigationSingleton.Instance.GetPath(currentNode, targetNode);

        Debug.Assert(null != path);

    }
	
	// Update is called once per frame
	void Update () {
        // This would get the Current node and set it.
        // మనమిప్పుడున్న Node.
        currentNode = NavigationSingleton.Instance.GetCurrentNode(this.transform.position);

        if (reCalculatePath)
        {
            calculatePath();
            reCalculatePath = false;
        }
        
        if (canTraverseDirectly)
        {
            // Check for Rotation.
            var direc = destination - transform.position;
            var rot = Quaternion.LookRotation(direc, transform.TransformDirection(Vector3.up));
            float angle = Quaternion.Angle(transform.rotation, new Quaternion(0, rot.y, 0, rot.w));
            if (angle > 10) { transform.rotation = Quaternion.RotateTowards(transform.rotation, new Quaternion(0, rot.y, 0, rot.w), aICharacter.turnSpeed * Time.deltaTime); return; }

            // Rotate First.
            this.aICharacter.ForceMove(AISpeedMod, 1);
        }
        else
        {
            Debug.DrawRay(rayCastSourcePoint, hitInfo.point, Color.red);
            // Use the nodes. Use the Nodes to traverse.
        }

	}

    internal void SetDestination(Vector3 targetPos, LayerMask _targetLayer)
    {
        this.destination = targetPos;
        this.targetLayer = _targetLayer;

        this.rayCastSourcePoint = this.transform.position;
        rayCastSourcePoint.y = rayCastHeight;
        this.rayCastTargetPoint = targetPos;
        rayCastTargetPoint.y = rayCastHeight;

        Vector3 rayDir = (rayCastTargetPoint - rayCastSourcePoint).normalized;

        //Debug.DrawRay(new Vector3( rayCastSourcePoint.x, rayCastSourcePoint.y + 0.1f, rayCastSourcePoint.z), rayDir * maxSensoryRadius, Color.green);

        // Raycast for the target, and if you can find it, we do not need the pathing nodes anymore...
        if ( Physics.Raycast(rayCastSourcePoint, rayDir, out hitInfo, maxSensoryRadius))
        {
            //Debug.DrawRay(rayCastSourcePoint, (rayCastTargetPoint - rayCastSourcePoint).normalized * maxSensoryRadius, Color.blue, 0.5f);
            if ( targetLayer == hitInfo.collider.transform.gameObject.layer)
            {
                Debug.DrawRay(rayCastSourcePoint, (rayCastTargetPoint - rayCastSourcePoint).normalized * maxSensoryRadius, Color.blue, 0.5f);
                canTraverseDirectly = true;
                return;
            }
        }

        canTraverseDirectly = false;
        reCalculatePath = true;

    }
}