using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomHeight : MonoBehaviour {
    public float minHeight;
    public float maxHeight;

	// Use this for initialization
	void Start () {
        print("count");
        transform.localScale = new Vector3(transform.localScale.x, Random.Range(minHeight, maxHeight), transform.localScale.z);
        transform.position = new Vector3(transform.position.x, 0.5f * transform.localScale.y, transform.position.z);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

}
