using UnityEngine;
using System.Collections;

//This script is curtousey of UnityCookie Creating custom pivot points youtube wideo

public class Gizmo : MonoBehaviour 
{
	public float gizmoSize = 1f;
	public Color gizmoColor = Color.yellow;
    public bool isVisible = true;

	void OnDrawGizmos()
	{
        if (isVisible)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, gizmoSize);
        }
	}
}