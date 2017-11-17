using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class Character : MonoBehaviour
{
    public enum CharacterState
    {
        none,
        idle_OutCombat,
        idle_InCombat,
        run,
        jump_up,
        jump_air,
        jump_down,
        aim,
        shoot,
        attack,
        adjustPosition,
        hit,
        dodge,
        roll,

    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

}
