using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ActionSelector : MonoBehaviour
{

    ActionList l_action;
    CombatManager m_combat;

    // ప్రస్తుత పరిస్థితి
    // Current State.
    State currentState;

    // Use this for initialization
    void Start()
    {
        l_action = GetComponent<ActionList>();
        m_combat = GetComponent<CombatManager>();
    }

    // Update is called once per frame
    void Update()
    {}

    public void selectNextOption()
    {

        // Check to see if I am facing the target or not.
        //Rotate
        var direc = this.m_combat.CurrentTarget.transform.position - transform.position;
        var rot = Quaternion.LookRotation(direc, transform.TransformDirection(Vector3.up));
        float angle = Quaternion.Angle(transform.rotation, new Quaternion(0, rot.y, 0, rot.w));
        // transform.rotation = new Quaternion(0, rot.y, 0, rot.w);
        currentState.amIFacingTheTarget = (angle < 20) ? customBool.True : customBool.False;

        // మన ప్రస్తుత పరిస్థితి చూసుకోవాలి.
        // Perform action based on the current State.
        currentState.hasTarget = (null != m_combat.CurrentTarget) ? customBool.True : customBool.False;
        currentState.canIAttack = (m_combat.canAttack) ? customBool.True : customBool.False;
        currentState.canTargetAttack = (m_combat.CurrentTarget.GetComponentInChildren<CombatManager>().canAttack) ? (customBool.True) : (customBool.False);
        currentState.amIStunned = (this.m_combat.IsHit) ? customBool.True : customBool.False;

        // వీటిని చూసి మార్చాలి.
        // TODO: Change these based on the current State.
        currentState.amIStunned = customBool.False;
        currentState.targetStunned = customBool.False;
        currentState.isTargetFacingMe = customBool.True;

        // దాని ద్వారా మనం చేయగలిగినవి ఏమేమి వున్నాయో చూసుకోవాలి.
        // Get the valid actions for the current state.
        List<Action> validActions = l_action.GetValidActions(currentState);

        // తరువాత అందులో ఒకటి చూసుకొని దానిని తీసుకోవాలి.
        // Select one of the valid actions.
        if (validActions.Count > 0)
        {
            int curAction = validActions.Count - 1;
            // ఇందిలో మొదటి Action తీసుకోవాలి.
            // Get the Action.
            if (m_combat.canAttack)
            {
                // Check the distance and see if the Enemy can use the attack or not??

                // New Code, is the problem
                {

                    float distanceToTarget = Vector3.Distance(this.transform.position, this.m_combat.CurrentTarget.transform.position);
                    float strikingDistance = this.m_combat.allMoves[(int)((validActions[curAction]).combat) - 1].AD;

                    float buffer = 0.1f;

                    if (Mathf.Abs(distanceToTarget - strikingDistance) > buffer)
                    {
                        this.m_combat.AdjustMinDistance = strikingDistance - buffer;
                        this.m_combat.AdjustMinDistance = strikingDistance + buffer;
                        this.m_combat.IsAdjusting = true;
                    }
                    else
                    {
                        m_combat.PerformAction(validActions[curAction]);
                        l_action.UpdateActionPreference(validActions[curAction].name, validActions[curAction].preference - 1);
                    }
                }


                //// Old Code
                //{
                //    m_combat.PerformAction(validActions[curAction]);
                //    l_action.UpdateActionPreference(validActions[curAction].name, validActions[curAction].preference - 1);
                //}
            }
        }
        else
        {
            // This Triggers when you are dead..
            Debug.LogError("No Valid Actions");
        }

        // ఏదీ లేకపోతే ఎం చెయ్యాలి? ఒక exception వెయ్యాలి. చూడాలి.
        // TODO: Throw an exception when no action is present..

    }

}
