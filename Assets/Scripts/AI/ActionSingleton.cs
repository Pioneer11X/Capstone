using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct State
{

    public bool hasTarget;
    public float targetHealth;
    // int targetState;
    public bool canTargetAttack;
    public bool canIAttack;
    public bool targetStunned;
    public bool amIStunned;
    public bool isTargetFacingMe;
    public bool amIFacingTheTarget;

    public string myPrevMove;
    public string targetPrevMove;

};

public struct Action
{
    State intialState;
    State finalState;
};

public class ActionSingleton : MonoBehaviour {

    public List<Action> initalActionsList = new List<Action>();

	// Use this for initialization
	void Start () {


	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
