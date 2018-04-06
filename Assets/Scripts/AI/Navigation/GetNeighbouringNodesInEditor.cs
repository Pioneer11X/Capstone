using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GetNeighbouringNodesInEditor : MonoBehaviour {

    public PathingNode node;

	// Use this for initialization
	void Start () {

        node = this.GetComponent<PathingNode>();

	}
	
	// Update is called once per frame
	void Update () {

        if ( null == node)
        {
            node = this.GetComponent<PathingNode>();
        }

        // మనకి index అన్నదిమనంచేత్తోపెట్టేటట్టయితేచాలాకష్టంగావుంటుంది. అందుకనిమనంకోడుద్వారాకనుక్కుందాం.
        // The index is really hard to fill out so, I decided to make use of Grid X and Grid Y to create a unique index based on them.
        node.index = 1000 * node.gridY + node.gridX;

        // మనతోకలసివున్ననోడులనుకూడాఇలానేకనుక్కుంటాము.
        // The same goes for finding the neighbouring nodes.
        // This is a huge quality of life change for the developer at the expense of a huge performance hit at the begining of the game.
        foreach (GameObject currentNode in GameObject.FindGameObjectsWithTag("PathNode"))
        {
            PathingNode currentNodeNode = currentNode.GetComponent<PathingNode>();
            if (2 > (Mathf.Abs(this.node.gridX - currentNodeNode.gridX) + Mathf.Abs(this.node.gridY - currentNodeNode.gridY)))
            {
                if ( (currentNodeNode.index != this.node.index) && !( this.node.connectedNodes.Contains(currentNodeNode) ) )
                this.node.connectedNodes.Add(currentNodeNode);
            }
        }

    }
}
