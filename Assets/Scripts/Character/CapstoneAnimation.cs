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

    public void Play(ThirdPCharacter.CharacterState state, int parameter)
    {
        switch (state)
        {
            case ThirdPCharacter.CharacterState.idle_OutCombat:
                //animator.Play("Idle_OutCombat");
                animator.CrossFade("Idle_OutCombat", 0.2f);
                break;
            case ThirdPCharacter.CharacterState.idle_InCombat:
                animator.CrossFade("Idle_InCombat",0.2f);
                break;
            case ThirdPCharacter.CharacterState.run:
                if (parameter == 0)
                {
                    animator.CrossFade("Run",0.1f);                    
                }
                else {
                    animator.Play("Dash");
                }
                
                break;
            case ThirdPCharacter.CharacterState.jump_up:
                animator.Play("Jump_Up");
                break;
            case ThirdPCharacter.CharacterState.jump_air:
                animator.Play("Jump_Air");
                break;
            case ThirdPCharacter.CharacterState.jump_down:
                animator.Play("Jump_Down");
                break;
            case ThirdPCharacter.CharacterState.dodge:
                if (parameter == 0)
                {
                    animator.Play("Dodge_Left");
                }
                else if (parameter == 1)
                {
                    animator.Play("Dodge_Right");
                }
                break;
            case ThirdPCharacter.CharacterState.roll:
                animator.Play("Roll");
                break;
            case ThirdPCharacter.CharacterState.attack:
                ThirdPCharacter.Combat combat = (ThirdPCharacter.Combat)parameter;
                animator.Play(combat.ToString(), -1, 0);
                break;
            case ThirdPCharacter.CharacterState.adjustPosition:
                animator.Play("Adjust");
                break;
            case ThirdPCharacter.CharacterState.hit:
                ThirdPCharacter.HitPosition pos = (ThirdPCharacter.HitPosition)((parameter / 100) % 10);
                ThirdPCharacter.HitDirection dir = (ThirdPCharacter.HitDirection)((parameter / 10) % 10);
                ThirdPCharacter.HitPower power = (ThirdPCharacter.HitPower)((parameter / 1) % 10);
                string animationName = "A_Hit_" + pos.ToString() + "_" + dir.ToString() + "_" + power.ToString();
                animator.Play(animationName,-1,0);
                break;
        }
    }


}
