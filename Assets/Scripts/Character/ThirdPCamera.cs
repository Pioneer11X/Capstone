using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Third Person Camera
/// This camera controller script is for a third person camera 
/// attached to a third person character controller that also has a 
/// character attached to it. 
/// </summary>

/* Camera Bumper code modified from a SmoothFollowWithCameraBumper Script
 * from Unity 3D Wiki by Daniel P. Rossi (DR9885)
 */

[AddComponentMenu("Camera-Control/Mouse")]

public class ThirdPCamera : MonoBehaviour 
{
    [SerializeField] private Transform target;        //target for camera to interact with
    [SerializeField] private Transform aimTarget;    // target for aim mode
    [SerializeField] private Transform aimTargetPos;    // target for aim mode

    private Transform lookAtTarget; // target camera should look at
    private Vector3 moveAlong;      //vector for camera zoom

    [SerializeField] private float bumperDistanceCheck = 2.0f;  // length of bumper ray
    [SerializeField] private float bumperCameraHeight = 0.5f;   // adjust camera height while bumping
    [SerializeField] private Vector3 bumperRayOffset;           // allows offset of the bumper ray from target origin
    [SerializeField] private float damping = 5.0f;              // damping
    [SerializeField] private float lowerTiltAngle = 45f;        // lower limit of camera Y tilt
    [SerializeField] private float upperTiltAngle = 110f;       // upper limit of camera Y tilt
    [SerializeField] private float minDistance = 3f;            // closet camera should get
    [SerializeField] private float maxDistance = 6f;            // furthest camera should get
    [SerializeField] private float aimHorizontal = 1f;          // furthest camera should aim left/right
    [SerializeField] private float aimVertical = 1f;            // furthest camera should aim up/down
    [SerializeField] private float aimSpeed = 0.1f;            // aiming speed


    // test variables
    // Do not change these in the inspector
    public int tooClose;
    public int tooFar;

    private Vector3 targetLastPos;
    private Vector3 aimTargetDefault;
    private float distance;

    //Start, setup the initial camera position with the character
    void Start()
    {
        // Early out if we don't have a target
        if (!target)
        {
            Debug.Log("Camera has no target at Start.");
            return;
        }
        targetLastPos = target.transform.position;
        lookAtTarget = target;
        aimTargetDefault = aimTarget.transform.localPosition;

        tooClose = 0;
        tooFar = 0;
    }//end start

    /// <summary>
    /// Called in sync with physics.
    /// Using to check if the camera will collide with an object.
    /// </summary>
    private void FixedUpdate()
    {
        Vector3 wantedPosition;
        float dt = Time.deltaTime;

        // check to see if there is anything behind the target
        RaycastHit hit;
        Vector3 back = transform.TransformDirection(-1 * Vector3.forward);

        // cast the bumper ray out from rear and check to see if there is anything behind
        if (Physics.Raycast(target.TransformPoint(bumperRayOffset), back, out hit, bumperDistanceCheck)
            && hit.transform != target) // ignore ray-casts that hit the user. DR
        {
            wantedPosition = transform.position;
            // clamp wanted position to hit position
            wantedPosition.x = hit.point.x;
            wantedPosition.z = hit.point.z;
            wantedPosition.y = Mathf.Lerp(hit.point.y + bumperCameraHeight, wantedPosition.y, dt * damping);

            transform.position = Vector3.Lerp(transform.position, wantedPosition, dt * damping);
        }

        // Set the position of the camera 
        Vector3 dir = target.transform.position - targetLastPos;

        // Adjust if the camera is too close or too far away
        distance = (transform.position - target.transform.position).magnitude;
        if (distance > maxDistance)
        {
            tooFar++;
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, 0.1f);
        }
        else if(distance < minDistance)
        {
            tooClose++;
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, -0.1f);
        }


        transform.position = transform.position + dir;

        targetLastPos = target.position;

        //keep the camera looking at the character
        transform.LookAt(lookAtTarget);
    }//end fixed update

    /// <summary>
    /// Called once per frame after update, checking if camera has a target.
    /// </summary>
    private void LateUpdate()
    {
        // Early out if we don't have a target
        if (!target)
        {
            Debug.Log("Camera has lost it's target during run time.");
            return;
        }

        //Debug.DrawLine(transform.position, transform.position + (transform.forward * 1.5f), Color.yellow);
    }//end late update

    /// <summary>
    /// Control camera vertical position and zoom.
    /// Camera does not control it's horizontal positon.
    /// </summary>
    /// <param name="rotationY">Camera Y value to rotate by</param>
    /// <param name="zoom">Camera zoom value to move by</param>
    public void moveCamera(float rotationX, float rotationY, float zoom)
    {
        //get a vector between camera and target
        moveAlong = transform.position - target.transform.position;
        
        //bind the angle between a lower and upper range
        //keep the camera bounded between alomst straight overhead and near to underneath
        float angle = Vector3.Angle(moveAlong, Vector3.up);

        if (angle > lowerTiltAngle && -rotationY > 0)
        {
            transform.RotateAround(target.transform.position, this.transform.right, -rotationY);
        }
        else if (angle < upperTiltAngle && -rotationY < 0)
        {
            transform.RotateAround(target.transform.position, this.transform.right, -rotationY);
        }

        // Rotate around the target
        if(rotationX != 0)
        {
            angle = Vector3.Angle(moveAlong, Vector3.forward);
            transform.RotateAround(target.transform.position, this.transform.up, rotationX);
        }

        //zoom the camera in or out
        //draw a line between the camera and it's target, and move the camera along that line
        moveAlong = transform.position - target.transform.position;

        if (moveAlong.magnitude > 1f && zoom > 0)
        {
            moveAlong.Normalize();
            moveAlong *= -zoom;
            transform.position += moveAlong / 2;
        }
        else if (zoom < 0 && moveAlong.magnitude < 15f)
        {
            moveAlong.Normalize();
            moveAlong *= -zoom;
            transform.position += moveAlong / 2;
        }
    }//end move camera

    /// <summary>
    /// Move camera to the aim state position.
    /// </summary>
    public void SetAimState(bool aiming)
    {
        if(aiming)
        {
            lookAtTarget = aimTarget;
            transform.forward = target.transform.forward;
            transform.position = aimTargetPos.transform.position;
        }
        else
        {
            lookAtTarget = target;
        }
    }

    /// <summary>
    /// Move camera in a gun aim fashion.
    /// </summary>
    /// <param name="v">Vertical Movement</param>
    /// <param name="h">Horizontal Movement</param>
    public void MoveForAiming(float v, float h)
    {
        if (v != 0 || h != 0)
        {
            if (v > 0 && (aimTarget.localPosition.y - aimTargetDefault.y < aimVertical + aimTargetDefault.y) )
            {
                aimTarget.localPosition = new Vector3(aimTarget.localPosition.x, aimTarget.localPosition.y + aimSpeed, aimTarget.localPosition.z);
            }
            else if (v < 0 && (aimTarget.localPosition.y - aimTargetDefault.y > aimTargetDefault.y - aimVertical))
            {
                aimTarget.localPosition = new Vector3(aimTarget.localPosition.x, aimTarget.localPosition.y - aimSpeed, aimTarget.localPosition.z);
            }

            if (h > 0 && (aimTarget.localPosition.x - aimTargetDefault.x < aimHorizontal + aimTargetDefault.x))
            {
                aimTarget.localPosition = new Vector3(aimTarget.localPosition.x + aimSpeed, aimTarget.localPosition.y, aimTarget.localPosition.z);
            }
            else if (h < 0 && (aimTarget.localPosition.x - aimTargetDefault.x > aimTargetDefault.x - aimHorizontal))
            {
                aimTarget.localPosition = new Vector3(aimTarget.localPosition.x - aimSpeed, aimTarget.localPosition.y, aimTarget.localPosition.z);
            }
        }
        else
        {
            aimTarget.localPosition = aimTargetDefault;
        }
    }


    /// <summary>
    /// Testing
    /// </summary>
    void OnApplicationQuit()
    {
        Debug.Log("Too close: " + tooClose);
        Debug.Log("Too far: " + tooFar);
    }

}//end ThirdPCamera Script
