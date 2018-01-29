using UnityEngine;
using System.Collections;

/// <summary>
/// Courtesy of http://wiki.unity3d.com/index.php?title=CameraFacingBillboard
/// Modified to stay upright.
/// </summary>
public class CameraFacingBillboard : MonoBehaviour
{
    public Camera m_Camera;

    void Update()
    {
        transform.LookAt(transform.position + new Vector3(m_Camera.transform.forward.x, 0, m_Camera.transform.forward.z),
             Vector3.up);
    }
}