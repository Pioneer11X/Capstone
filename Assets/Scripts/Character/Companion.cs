using UnityEngine;

/// <summary>
/// Companion follow script
/// </summary>
public class Companion : MonoBehaviour
{
    public Transform target;    //target for companion to interact with

    [SerializeField] private float initDistance;
    [SerializeField] private float initHeight = 1.5f;
    [SerializeField] private float combatDistance;
    [SerializeField] private float combatHeight = 1.0f;
    [SerializeField] private float heightDamping = 4.0f;
    [SerializeField] private float positionDamping = 4.0f;

    private float distance;
    private float height;
    private Vector3 targetLastPos;
    public bool inUse;
    public bool inCombat;

    //Start, setup the initial companion position with the character
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
        inCombat = false;
    }//end start


    private void Update()
    {
        // Early out if we don't have a target
        if (!target)
        {
            Debug.Log("Companion has lost it's target during run time.");
            return;
        }

        if(!inCombat)
        {
            distance = initDistance;
            height = initHeight;
        }
        else
        {
            distance = combatDistance;
            height = combatHeight;
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

            transform.position = transform.position + dir;

            targetLastPos = target.position;
        }
        //Debug.DrawLine(transform.position, transform.position + (transform.forward * 1.5f), Color.yellow);
    }


}//end of Companion
