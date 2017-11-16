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
                animator.Play("TestAttack");
                break;
            case ThirdPCharacter.CharacterState.adjustPosition:
                animator.Play("Adjust");
                break;
            case ThirdPCharacter.CharacterState.hit:
                if (parameter == 0) {
                    animator.Play("HitForward");

                }
                if (parameter == 1)
                {
                    animator.Play("HitBackward");

                }
                if (parameter == 2)
                {
                    animator.Play("HitLeft");

                }
                if (parameter == 3)
                {
                    animator.Play("HitRight");

                }
                break;
        }
    }


}
