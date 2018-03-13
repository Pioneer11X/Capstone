using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationSingleton : MonoBehaviour {

    public static NavigationSingleton Instance { get; private set; }

    // This is a flag to determine if we can calculate the path or not. If one enemy is calculating the path.. Another can't because the fCost and hCost is dependant on the start position. Also, the parent or the node where it came from.
    // మనం రెండు enemyలకు ఒకే సారి దారి కనుక్కోవాలంటే కష్టం. fCost ఇంకా hCost రెండిటికీ తేడా వస్తుంది.
    public bool canCalculte = true;

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
        float temporaryDistance = 1000;

        // దీనిని తరువాత మార్చు.
        // This is not ideal as we call this quite frequently and we do not want to perform this same function over and over again unless you actually optimise this.
        for ( int i = 0; i < nodes.Count; i++)
        {
            if ( temporaryDistance > Vector3.Distance(currentPosition, nodes[i].nodePosition))
            {
                temporaryDistance = Vector3.Distance(currentPosition, nodes[i].nodePosition);
                returnNode = nodes[i];
            }
        }

        Debug.Assert(null != returnNode, "The Nodes aren't set properly. There is not node for position of " + currentPosition);

        return returnNode;

    }

    // మనం ఒక నోడు నుండి వేరొకదానికి వెళ్ళటానికి దారి కనుక్కోవాలి.
    // Find the path between two one to another node.
    public List<PathingNode> GetPath(PathingNode nodeA, PathingNode nodeB)
    {
        // nodeA is the Start Node. nodeB is the endNode.

        // మనం A* వాడతాము. రెండు ప్రక్కప్రక్క నోడ్ల మధ్య దూరం 1 అనుకుందాం.
        // Assume that the distance between two adjacent nodes is 1. We will be using A* method for Pathfinding.

        List<PathingNode> openSet = new List<PathingNode>();
        HashSet<PathingNode> closedSet = new HashSet<PathingNode>();

        openSet.Add(nodeA);
        nodeA.gCost = 0;

        while (openSet.Count > 0)
        {

            PathingNode currentNode = openSet[0];

            for ( int i = 1; i < openSet.Count; i++)
            {
                if ( openSet[i].fCost < currentNode.fCost || ( (openSet[i].fCost == currentNode.fCost) && openSet[i].hCost < currentNode.hCost ) )
                {
                    currentNode = openSet[i];
                }
            }

            if (nodeB == currentNode)
            {
                // TODO: What is pushed onto return Path.
                List<PathingNode> returnPath = RetracePath(nodeA, nodeB);
                return returnPath;
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
                        
            foreach ( PathingNode neighbour in currentNode.connectedNodes)
            {
                if (closedSet.Contains(neighbour))
                {
                    continue;
                }

                Debug.Assert(null != currentNode);
                Debug.Assert(null != nodeB);
                Debug.Assert(null != neighbour);

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if ( newMovementCostToNeighbour < neighbour.gCost || !(openSet.Contains(neighbour))) {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, nodeB);
                    neighbour.cameToThisNodeFrom = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }

                }
            }

        }

        // Well, if you reach this. There is no path..
        Debug.LogError("There is no path found from Nodes with index, " + nodeA.index + " to node with index, " + nodeB.index + ". They are named " + nodeA.transform.name + " and " + nodeB.transform.name);
        return null;

    }

    List<PathingNode> RetracePath(PathingNode nodeA, PathingNode nodeB)
    {
        List<PathingNode> returnPath = new List<PathingNode>();
        PathingNode currentNode = nodeB;

        while ( nodeA != currentNode)
        {
            returnPath.Add(currentNode);
            Debug.Assert(null != currentNode.cameToThisNodeFrom, "This path is broken / incomplete.");
            currentNode = currentNode.cameToThisNodeFrom;
        }

        returnPath.Reverse();
        return returnPath;
    }

    int GetDistance(PathingNode nodeA, PathingNode nodeB)
    {

        Debug.Assert(null != nodeA);
        Debug.Assert(null != nodeB);

        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return (distX > distY) ? (14 * distY + 10 * (distX - distY)) : (14 * distX + 10 * (distY - distX));
    }

}