using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turns off it's assigned object and turns it back on when player enters the trigger
/// Can be manually turned off
/// </summary>
public class ActivateTutorialItem : MonoBehaviour
{
    [SerializeField] GameObject TutorialItem;

	// Use this for initialization
	void Start ()
    {
        TutorialItem.SetActive(false);

    }

    public void TurnOff()
    {
        TutorialItem.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            TutorialItem.SetActive(true);
        }
    }
}
