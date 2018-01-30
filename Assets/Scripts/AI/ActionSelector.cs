using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ActionSelector : MonoBehaviour {

    ActionList l_action;
    CombatManager m_combat;

    // ప్రస్తుత పరిస్థితి
    State currentState;

    // మనకి మన ఆక్షన్ పేరుతో పాటు ఒక ప్రాదాన్యతా అంక్య కూడా వుండాలి.

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
        currentState.hasTarget = (null != m_combat.CurrentTarget) ? customBool.True : customBool.False;
        currentState.canIAttack = (m_combat.canAttack) ? customBool.True : customBool.False;
        currentState.canTargetAttack = (m_combat.CurrentTarget.GetComponentInChildren<CombatManager>().canAttack) ? (customBool.True) : (customBool.False);

        // వీటిని చూసి మార్చాలి.
        currentState.amIStunned = customBool.False;
        currentState.targetStunned = customBool.False;
        currentState.isTargetFacingMe = customBool.True;
        currentState.amIFacingTheTarget = customBool.True;

        // దాని ద్వారా మనం చేయగలిగినవి ఏమేమి వున్నాయో చూసుకోవాలి.
        List<Action> validActions = l_action.GetValidActions(currentState);

        // తరువాత అందులో ఒకటి చూసుకొని దానిని తీసుకోవాలి.
        if ( validActions.Count > 0)
        {
            // ఇందిలో మొదటి Action తీసుకోవాలి.
            // ఎలా అన్నది కొంచెం తేడాగా వుంది.
            Debug.Log(validActions[0].name);
        }
        
        // ఏదీ లేకపోతే ఎం చెయ్యాలి? ఒక exception వెయ్యాలి.

    }


}
