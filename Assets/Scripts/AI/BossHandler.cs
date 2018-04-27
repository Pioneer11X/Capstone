using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHandler : MonoBehaviour
{
    public GameObject Boss;
    public GameObject BossBar;


    // Use this for initialization
    void Start ()
    {
        Boss.SetActive(false);
        BossBar = GameObject.FindGameObjectWithTag("BossBar");
        BossBar.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "PlayerChest")
        {
            Boss.SetActive(true);
            BossBar.SetActive(true);
        }
    }
}
