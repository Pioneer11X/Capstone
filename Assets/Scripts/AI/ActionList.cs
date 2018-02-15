using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum customBool
{
    True,
    False,
    DoesntMatter
}

[System.Serializable]
public struct State
{

    public customBool hasTarget;
    // int targetState;
    public customBool canTargetAttack;
    public customBool canIAttack;
    public customBool targetStunned;
    public customBool amIStunned;
    public customBool isTargetFacingMe;
    public customBool amIFacingTheTarget;

    public string myPrevMove;
    public string targetPrevMove;

    public State(customBool _hasTarget = customBool.True, customBool _canTargetAttack = customBool.DoesntMatter, customBool _canIAttack = customBool.True, customBool _targetStunned = customBool.DoesntMatter, customBool _amIStunned = customBool.False, customBool _isTargetFacingMe = customBool.DoesntMatter, customBool _amIFacingTheTarget = customBool.True, string _myPrevMove = "", string _targetPrevMove = "")
    {
        this.hasTarget = _hasTarget;
        this.canTargetAttack = _canTargetAttack;
        this.canIAttack = _canIAttack;
        this.targetStunned = _targetStunned;
        this.amIStunned = _amIStunned;
        this.isTargetFacingMe = _isTargetFacingMe;
        this.amIFacingTheTarget = _amIFacingTheTarget;
        this.myPrevMove = _myPrevMove;
        this.targetPrevMove = _targetPrevMove;
    }

};

[System.Serializable]
public struct Action
{
    // Combat attributes are taken care of by the Combat. The final State contains the estimated final state.
    public string name;
    [Space(10)]

    public State intialState;
    public State finalState;

    public CombatManager.Combat combat;

    public float actionTime;
    public float damage;

    // దీనితో పాటు ఒక ప్రాదాన్యత అంకె కూడా కావలి. చేసినవి మళ్ళి మళ్ళి చెయ్యకూడదు.
    // We need a preference to make sure that the actions would not be repeated.
    public int preference;
 

    public Action(string _name, State _initialState, State _finalState, CombatManager.Combat _combat, float actionTime, float _damage, int _preference = 10)
    {
        this.name = _name;
        this.intialState = _initialState;
        this.finalState = _finalState;
        this.combat = _combat;
        this.actionTime = actionTime;
        this.damage = _damage;
        this.preference = _preference;
    }

};


public class ActionList : MonoBehaviour {

    public List<Action> initalActionsList;

	// Use this for initialization
	void Start () {


	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateActionPreference(String actionName, int preference)
    {
        for (int i = 0; i < initalActionsList.Count; i++)
        {
            if ( actionName == initalActionsList[i].name)
            {
                Action newAction;
                newAction = initalActionsList[i];
                newAction.preference = preference;
                initalActionsList[i] = newAction;
            }
        }
    }

    // ఒక పరిస్థితి ఇస్తే దానికి తగట్టు మనం ఒక సమాధానంగా ఏ పని చెయ్యాలి?
    // Get the valid actions for the given state.
    public List<Action> GetValidActions(State currentState)
    {

        List<Action> validActions = new List<Action>();

        // వున్నా అన్నిటినీ ముందు తెచ్చుకోవాలి.
        // అందులో ఏవి మనకి కుదురుతాయో చూసుకోవాలి.
        for (int i = 0; i < initalActionsList.Count; i++)
        {
            // ఒక్కొక్క దానికి చూస్తుంటావు.
            // ఒక్కొక్క సంబందిత ప్రమాన్నన్నీ చూసుకోవాలి.
            // initialState సరిగ్గా వుందో లేదో చూసుకో.
            try {

                if (compatible(initalActionsList[i].intialState, currentState))
                {
                    validActions.Add(initalActionsList[i]);
                }

            }
            catch(Exception e)
            {
                Debug.LogError(e);
                Debug.Log("Welp");
            }
            

        }

        
        // ఇప్పుడు చెయ్యగలిగిన అన్నిటిలో మనం దేనికి ఎక్కువ ప్రాధాన్యత ఇస్తామో చూసుకొని దానిని చెయ్యి.
        // Sort all the actions by the preference..
        validActions.Sort((a, b) => (a.preference - b.preference));

        // Return the Valid ACtions. This could be an empty string.
        // ఇప్పుడు మొదల్లో వున్నదాన్ని చెయ్యాలి.
        return validActions;

    }

    public bool compatible(State needState, State givenState)
    {
        // ఇందిలో అన్ని పరినామాల్నీ చూసుకొని సరిగ్గా వుందో లేదో చెప్పాలి.

        if (!compatible(needState.hasTarget, givenState.hasTarget)) return false;
        if (!compatible(needState.canTargetAttack, givenState.canTargetAttack)) return false;
        if (!compatible(needState.canIAttack, givenState.canIAttack)) return false;
        if (!compatible(needState.targetStunned, givenState.targetStunned)) return false;
        if (!compatible(needState.amIStunned, givenState.amIStunned)) return false;
        if (!compatible(needState.isTargetFacingMe, givenState.isTargetFacingMe)) return false;
        if (!compatible(needState.amIFacingTheTarget, givenState.amIFacingTheTarget)) return false;

        return true;
    }

    // మన పరిణామాన్ని(parameter) కొలిసే function ఒకటి కావాలి.
    public bool compatible(customBool needed, customBool given)
    {
        // అవసరము లేదంటే మంచిది.
        if ( needed == customBool.DoesntMatter)
        {
            return true;
        }else if ( needed == given)
        {
            // ఒకవేళ అవసరమైతే, సరిగ్గా ఉందేమో చూడు.
            return true;
        }
        return false;
    }

}
