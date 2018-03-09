using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationSingleton : MonoBehaviour {

    public static NavigationSingleton Instance { get; private set; }

    // ఇవి మనం scene import చేసినప్పుడు c++ ఇంజెను లో నింపుతాము.
    // This is the pointer to the existing nodes. This would be filled upfront on the engine from the exporter.
    public List<PathingNode> nodes;

    void Awake()
    {
        Instance = this;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // మనం ప్రస్తుతం ఎక్కడ వున్నామో చూసుకోవాలి.
    // Returns the current node that you are standing over.
    public PathingNode GetCurrentNode(Vector3 currentPosition)
    {

        PathingNode returnNode = null;
        float temporaryDistance = 0;

        // దీనిని తరువాత మార్చు.
        // This is not ideal as we call this quite frequently and we do not want to perform this same function over and over again unless you actually optimise this.
        for ( int i = 0; i < nodes.Count; i++)
        {
            if ( temporaryDistance > Vector3.Distance(this.transform.position, nodes[i].nodePosition))
            {
                returnNode = nodes[i];
            }
        }

        Debug.Assert(null != returnNode, "The Nodes aren't set properly.");

        return returnNode;

    }

    // మనం ఒక నోడు నుండి వేరొకదానికి వెళ్ళటానికి దారి కనుక్కోవాలి.
    // Find the path between two one to another node.
    public List<PathingNode> GetPath(PathingNode nodeA, PathingNode nodeB)
    {

        // మనం A* వాడతాము. రెండు నోడ్ల మధ్య దూరం 1 అనుకుందాం.
        // Assume that the distance between two nodes is 1. We will be using A* method for Pathfinding.

        List<PathingNode> returnPath = new List<PathingNode> { };



        return returnPath;

    }

}