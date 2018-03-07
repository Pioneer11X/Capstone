using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathingNode : MonoBehaviour {

    // ఇది మనం అన్ని నోడ్లు చూసుకొని పెట్టుకొంటాము.
    public int index;

    public Vector3 nodePosition;

    [SerializeField]
    public PathingNode[] connectedNodes;

    // Use this for initialization
    void Start () {
		if ( null == nodePosition)
        {
            // ఇది అస్తమానూ మార్చము. అసలు మార్చకుండా చుడాలి.
            // We wouldn't change this often. Ideally, not at all.
            nodePosition = this.transform.position;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
