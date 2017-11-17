using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapstoneAnimation : MonoBehaviour
{

    private Animator animator;
    private Animation a;
    public enum AnimationName
    {
        run,
        idle_OutCombat,
        idle_InCombat,

    }



    // Use this for initialization
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Play(Character.CharacterState state, int parameter)
    {
        switch (state)
        {
            case Character.CharacterState.idle_OutCombat:
                //animator.Play("Idle_OutCombat");
                animator.CrossFade("Idle_OutCombat", 0.2f);
                break;
            case Character.CharacterState.idle_InCombat:
                animator.CrossFade("Idle_InCombat",0.2f);
                break;
            case Character.CharacterState.run:
                if (parameter == 0)
                {
                    animator.CrossFade("Run",0.1f);                    
                }
                else {
                    animator.Play("Dash");
                }
                
                break;
            case Character.CharacterState.jump_up:
                animator.Play("Jump_Up");
                break;
            case Character.CharacterState.jump_air:
                animator.Play("Jump_Air");
                break;
            case Character.CharacterState.jump_down:
                animator.Play("Jump_Down");
                break;
            case Character.CharacterState.dodge:
                if (parameter == 0)
                {
                    animator.Play("Dodge_Left");
                }
                else if (parameter == 1)
                {
                    animator.Play("Dodge_Right");
                }
                break;
            case Character.CharacterState.roll:
                animator.Play("Roll");
                break;
            case Character.CharacterState.attack:
                ThirdPCharacter.Combat combat = (ThirdPCharacter.Combat)parameter;
                animator.Play(combat.ToString(), -1, 0);
                break;
            case Character.CharacterState.adjustPosition:
                animator.Play("Adjust");
                break;
            case Character.CharacterState.hit:
                ThirdPCharacter.HitPosition pos = (ThirdPCharacter.HitPosition)((parameter / 100) % 10);
                ThirdPCharacter.HitDirection dir = (ThirdPCharacter.HitDirection)((parameter / 10) % 10);
                ThirdPCharacter.HitPower power = (ThirdPCharacter.HitPower)((parameter / 1) % 10);
                string animationName = "A_Hit_" + pos.ToString() + "_" + dir.ToString() + "_" + power.ToString();
                animator.Play(animationName,-1,0);
                break;
        }
    }


}
