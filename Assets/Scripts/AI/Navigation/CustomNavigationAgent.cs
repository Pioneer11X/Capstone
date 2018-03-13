using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ఇది మనం Unity NavMeshAgent లాగా వాడాలి.
// This should be aligned as close to the Unitys NavMesh Agent as possible.
public class CustomNavigationAgent : MonoBehaviour {

    public Vector3 destination;
    public LayerMask targetLayer;

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

    // ఆగున్నామా?
    // Similar to the Unity NavMeshAgent.
    public bool isStopped;

    // Dummy Variable for debugging.
    // ఇది వుత్తినే పెట్టాం.
    [SerializeField]
    bool canTraverseDirectly;
    [SerializeField]
    RaycastHit hitInfo;

    // Use this for initialization
    void Start () {
		
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
            Debug.DrawLine(this.transform.position, hitInfo.transform.position);
        }

	}

    internal void SetDestination(Vector3 targetPos, LayerMask _targetLayer)
    {
        this.destination = targetPos;
        this.targetLayer = _targetLayer;

        // Raycast for the target, and if you can find it, we do not need the pathing nodes anymore...
        if ( Physics.Raycast(this.transform.position, (targetPos - this.transform.position), out hitInfo))
        {
            if ( targetLayer == hitInfo.transform.gameObject.layer)
            {
                canTraverseDirectly = true;
                return;
            }
        }

        canTraverseDirectly = false;
        reCalculatePath = true;

    }
}