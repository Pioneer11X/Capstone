using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ActionSelector : MonoBehaviour {

    ActionList l_action;
    CombatManager m_combat;

    // ప్రస్తుత పరిస్థితి
    // Current State.
    State currentState;

    // Use this for initialization
    void Start () {
        l_action = GetComponent<ActionList>();
        m_combat = GetComponent<CombatManager>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void selectNextOption()
    {
        // మన ప్రస్తుత పరిస్థితి చూసుకోవాలి.
        // Perform action based on the current State.
        currentState.hasTarget = (null != m_combat.CurrentTarget) ? customBool.True : customBool.False;
        currentState.canIAttack = (m_combat.canAttack) ? customBool.True : customBool.False;
        currentState.canTargetAttack = (m_combat.CurrentTarget.GetComponentInChildren<CombatManager>().canAttack) ? (customBool.True) : (customBool.False);

        // వీటిని చూసి మార్చాలి.
        // TODO: Change these based on the current State.
        currentState.amIStunned = customBool.False;
        currentState.targetStunned = customBool.False;
        currentState.isTargetFacingMe = customBool.True;
        currentState.amIFacingTheTarget = customBool.True;

        // దాని ద్వారా మనం చేయగలిగినవి ఏమేమి వున్నాయో చూసుకోవాలి.
        // Get the valid actions for the current state.
        List<Action> validActions = l_action.GetValidActions(currentState);

        // తరువాత అందులో ఒకటి చూసుకొని దానిని తీసుకోవాలి.
        // Select one of the valid actions.
        if ( validActions.Count > 0)
        {
            int curAction = validActions.Count - 1;
            // ఇందిలో మొదటి Action తీసుకోవాలి.
            // Get the Action.
            if (m_combat.canAttack)
            {
                m_combat.PerformAction(validActions[curAction].combat);
                l_action.UpdateActionPreference(validActions[curAction].name, validActions[curAction].preference - 1);
            }
        }
        
        // ఏదీ లేకపోతే ఎం చెయ్యాలి? ఒక exception వెయ్యాలి. చూడాలి.
        // TODO: Throw an exception when no action is present..

    }

}
