using UnityEngine;

public class Companion : MonoBehaviour
{
    public Transform target;    //target for companion to interact with

    [SerializeField] private float bumperDistanceCheck = 2.0f;  // length of bumper ray
    [SerializeField] private float bumperHeight = 0.5f;         // adjust height while bumping
    [SerializeField] private Vector3 bumperRayOffset;           // allows offset of the bumper ray from target origin
    [SerializeField] private float damping = 5.0f;              // damping
    [SerializeField] private float distance;
    [SerializeField] private float height = 1.5f;
    [SerializeField] private float heightDamping = 4.0f;
    [SerializeField] private float positionDamping = 4.0f;
    [SerializeField] private float rotationDamping = 4.0f;


    private Vector3 targetLastPos;
    
    public bool inUse;

    //Start, setup the initial camera position with the character
    void Start()
    {
        // Early out if we don't have a target
        if (!target)
        {
            Debug.Log("Companion has no target at Start.");
            return;
        }
        targetLastPos = target.transform.position;
        inUse = false;
    }//end start


    private void Update()
    {
        // Early out if we don't have a target
        if (!target)
        {
            Debug.Log("Companion has lost it's target during run time.");
            return;
        }

        if (!inUse)
        {
            Vector3 dir = target.transform.position - targetLastPos;

            float dt = Time.deltaTime;
            float wantedHeight = target.transform.position.y + height;
            float currentHeight = transform.position.y;

            // Damp the height
            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * dt);

            // Set the position of the companion 
            Vector3 wantedPosition = target.position - target.forward * distance;
            transform.position = Vector3.Lerp(transform.position, wantedPosition, positionDamping * dt);

            // adjust the height of the companion
            transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

            // look at the target

            transform.forward = Vector3.Lerp(transform.forward, target.position - transform.position, rotationDamping * dt);

            transform.position = transform.position + dir;

            targetLastPos = target.position;
        }
        //Debug.DrawLine(transform.position, transform.position + (transform.forward * 1.5f), Color.yellow);
    }

    /// <summary>
    /// Called in sync with physics.
    /// Using to check if the camera will collide with an object.
    /// </summary>
    private void FixedUpdate()
    {
        Vector3 wantedPosition;
        float dt = Time.deltaTime;

        //check to see if there is anything behind the target
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
            wantedPosition.y = Mathf.Lerp(hit.point.y + bumperHeight, wantedPosition.y, dt * damping);

            transform.position = Vector3.Lerp(transform.position, wantedPosition, dt * damping);
        }

    }//end fixed update

    /// <summary>
    /// Called once per frame after update, checking if camera has a target.
    /// </summary>
    private void LateUpdate()
    {
        
    }//end late update

}//end of Companion
