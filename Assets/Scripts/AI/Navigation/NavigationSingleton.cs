using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PathingNode
{
    int index;
    Vector3 nodePosition;
    PathingNode[] connectedNodes;
}

public class NavigationSingleton : MonoBehaviour {

    public static NavigationSingleton Instance { get; private set; }

    // 
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

    

}
