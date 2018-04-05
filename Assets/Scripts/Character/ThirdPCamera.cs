using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Third Person Camera
/// This camera controller script is for a third person camera 
/// attached to a third person character controller that also has a 
/// character attached to it. 
/// Originally from Unity Third Person Controller, heavily modified for personal use.
/// </summary>

[AddComponentMenu("Camera-Control/Mouse")]

public class ThirdPCamera : MonoBehaviour 
{
    [SerializeField] private Transform target;                  // target for camera to interact with
    [SerializeField] private Transform aimTargetPos;            // target for aim mode

    private Transform lookAtTarget;                             // target camera should look at
    private Vector3 moveAlong;                                  // vector for camera zoom

    [SerializeField] private float bumperDistanceCheck = 2.0f;  // length of bumper ray
    [SerializeField] private float bumperMaxDistance = 0.5f;
    [SerializeField] private Vector3 bumperRayOffset;           // allows offset of the bumper ray from target origin
    [SerializeField] private float damping = 5.0f;              // damping
    [SerializeField] private float lowerTiltAngle = 45f;        // lower limit of camera Y tilt
    [SerializeField] private float upperTiltAngle = 110f;       // upper limit of camera Y tilt
    [SerializeField] private float minDistance = 3f;            // closet camera should get
    [SerializeField] private float maxDistance = 6f;            // furthest camera should get

    private bool isAiming;                                      // is the character aiming?
    private Vector3 targetLastPos;                              // target's last known position
    private RaycastHit hitRay;                                  // Raycast hit
    private Vector3 toTarget;                                   // Vector from camera to player

    //**************************************************************
    // test variables
    // Do not change these in the inspector
    public int tooClose;
    public int tooFar;
    //**************************************************************



    /// <summary>
    /// Start, setup the initial camera position with the character
    /// </summary>
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

        isAiming = false;

        tooClose = 0;
        tooFar = 0;
    }// end start

    /// <summary>
    /// Update
    /// </summary>
    private void Update()
    {
        float dT = Time.deltaTime;

        if(BumperCheck(transform.position, dT))
        {
            AdjustCameraPosition(hitRay, toTarget, dT);
        }
        else if (!isAiming)
        {         
            // Adjust if the camera is too close or too far away
            float dist = (transform.position - target.transform.position).magnitude;
            if (dist > maxDistance)
            {
                tooFar++;
                transform.position = Vector3.MoveTowards(transform.position, target.transform.position, 0.1f);
            }
            else if (dist < minDistance + 2.0f)
            {
                tooClose++;
                Vector3 testVec = Vector3.MoveTowards(transform.position, target.transform.position, -damping * dT);
                if (!BumperCheck(testVec, dT))
                {
                    transform.position = testVec;
                }
            }
        }

    }// End Update


    /// <summary>
    /// Called in sync with physics.
    /// Using to check if the camera will collide with an object.
    /// </summary>
    private void FixedUpdate()
    {
        // Maintain Aim Position if in Aim Mode
        if (isAiming )
        {
            transform.position = aimTargetPos.transform.position;
        }


        // Set the position of the camera 
        Vector3 dir = target.transform.position - targetLastPos;

        transform.position = transform.position + dir;

        targetLastPos = target.position;

        // If the camera is too far away, move it to the player
        if(Vector3.Distance(transform.position, target.transform.position) > 20)
        {
            transform.position = target.position + new Vector3(1,0,1);
        }

        //keep the camera looking at the character
        transform.LookAt(lookAtTarget);
    }// end fixed update

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
    }// end late update

    /// <summary>
    /// Control camera vertical position and zoom.
    /// Camera does not control it's horizontal positon.
    /// </summary>
    /// <param name="rotationY">Camera Y value to rotate by</param>
    /// <param name="zoom">Camera zoom value to move by</param>
    public void moveCamera(float rotationX, float rotationY, float zoom)
    {
		// t = t*t*t * (t * (6f*t - 15f) + 10f)
        // get a vector between camera and target
        moveAlong = transform.position - target.transform.position;
        
        // bind the angle between a lower and upper range
        // keep the camera bounded between alomst straight overhead and near to underneath
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

        // zoom the camera in or out
        // draw a line between the camera and it's target, and move the camera along that line
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
    }// end move camera

    /// <summary>
    /// Move camera to the aim state position.
    /// </summary>
    public void SetAimState(bool aiming, GameObject obj = null)
    {
        isAiming = aiming;
        if (aiming && obj != null)
        {
            lookAtTarget = obj.transform;
            transform.forward = target.transform.forward;
            transform.position = aimTargetPos.transform.position;
        }
        else
        {
            lookAtTarget = target;
        }
    }

    /// <summary>
    /// Testing
    /// </summary>
    void OnApplicationQuit()
    {
        //Debug.Log("Too close: " + tooClose);
        //Debug.Log("Too far: " + tooFar);
    }

    /// <summary>
    /// Change the Camera's target
    /// </summary>
    /// <param name="t"></param>
    /// <param name="at"></param>
    public void ChangeTarget(Transform t, Transform at) {
        target = t;

        if(null != at)
            aimTargetPos = at;

        if (!target)
        {
            Debug.Log("Camera has no target at Start.");
            return;
        }

        targetLastPos = target.transform.position;
        lookAtTarget = target;

        isAiming = false;

        tooClose = 0;
        tooFar = 0;
    }

    /// <summary>
    /// Bumper check against surroundings
    /// </summary>
    /// <param name="dT"></param>
    /// <returns></returns>
    private bool BumperCheck(Vector3 pos, float dT)
    {
        RaycastHit hit;
        Vector3 back = transform.TransformDirection(-1 * Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        Vector3 left = transform.TransformDirection(-1 * Vector3.right);
        toTarget = (pos - target.transform.position);

        // Perform three separate checks, right, back, left.
        // Ignore hits on the player and companion, maybe enemies too
        if (Physics.SphereCast(pos, bumperDistanceCheck / 2, right, out hit, bumperMaxDistance)
            && !hit.collider.CompareTag("Player"))
        {
            hitRay = hit;
            return true;
        }
        else if (Physics.SphereCast(pos, bumperDistanceCheck, back, out hit, bumperMaxDistance)
            && !hit.collider.CompareTag("Player"))
        {
            hitRay = hit;
            return true;
        }
        else if (Physics.SphereCast(pos, bumperDistanceCheck / 2, left, out hit, bumperMaxDistance)
            && !hit.collider.CompareTag("Player"))
        {
            hitRay = hit;
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// Adjust the camera's position to account for hitting an object
    /// </summary>
    private void AdjustCameraPosition(RaycastHit hit, Vector3 toTarget, float dT)
    {
        float dist = (transform.position - target.transform.position).magnitude;
        float distance = ((hit.distance * 100) / bumperMaxDistance) * maxDistance;
        toTarget = Vector3.ClampMagnitude(toTarget, distance);
        if (!(dist < minDistance))
        {
            transform.position = Vector3.Lerp(transform.position, (transform.position - toTarget), dT * damping);
        }
    }


}// end ThirdPCamera Script
