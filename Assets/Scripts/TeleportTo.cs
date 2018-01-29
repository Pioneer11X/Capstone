using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Teleport GameObject to a specific location
/// </summary>
public class TeleportTo : MonoBehaviour
{
    [SerializeField] GameObject ObjectToTele;
    [SerializeField] GameObject PlayerToTele;
    [SerializeField] GameObject CompanionToTele;
    [SerializeField] GameObject CameraToTele;
    [SerializeField] GameObject ObjectToTeleTo;

	// Use this for initialization
	void Start ()
    {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            PlayerToTele.transform.position = ObjectToTeleTo.transform.localPosition;
            CompanionToTele.transform.position = ObjectToTeleTo.transform.localPosition;
            CameraToTele.transform.position = ObjectToTeleTo.transform.localPosition;
        }
    }
}
