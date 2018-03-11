using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ఇది మనం Unity NavMeshAgent లాగా వాడాలి.
// This should be aligned as close to the Unitys NavMesh Agent as possible.
public class CustomNavigationAgent : MonoBehaviour {

    public Vector3 destination;

    [SerializeField]
    private List<PathingNode> path;

    // Our Current Node
    // మనం ఇప్పుడు వున్న Node.
    private PathingNode currentNode;

    // మనం వెళ్ళాల్సిన Node
    private PathingNode targetNode;

    // ఆగున్నామా?
    // Similar to the Unity NavMeshAgent.
    public bool isStopped;

	// Use this for initialization
	void Start () {
		
	}

    void calculatePath()
    {
        Debug.Assert(null != currentNode);
        Debug.Assert(null != targetNode);

        // This is an infinite loop waiting for the canCalculate Flag to be set.
        // దారి కనుక్కోగలిగే వరకూ ఆగు. ఇది ఎంత మంచిదన్నది తెలియదు.
        while (!NavigationSingleton.Instance.canCalculte)
        {
            continue;
        }
        path = NavigationSingleton.Instance.GetPath(currentNode, targetNode);
    }
	
	// Update is called once per frame
	void Update () {
        // This would get the Current node and set it.
        // మనమిప్పుడున్న Node.
        currentNode = NavigationSingleton.Instance.GetCurrentNode(this.transform.position);
	}

    internal void SetDestination(Vector3 targetPos)
    {
        this.destination = targetPos;
    }
}