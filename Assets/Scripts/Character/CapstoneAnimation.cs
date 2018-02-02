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
        //Debug.Log(state);
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
            case Character.CharacterState.draw_Gun:
                animator.Play("Gun_Draw");
                break;
            case Character.CharacterState.holster_Gun:
                animator.Play("Gun_Holster");
                break;
            case Character.CharacterState.shoot:
                animator.Play("Gun_Shoot");
                break;
            case Character.CharacterState.aim_Idle:
                animator.Play("Gun_Idle");
                break;
            case Character.CharacterState.aim_Move:
                if (parameter == 0)
                {
                    animator.Play("Aim_Sidestep_L");
                }
                else if (parameter == 1)
                {
                    animator.Play("Aim_Sidestep_R");
                }
                else if (parameter == 2)
                {
                    animator.Play("Aim_WalkB");
                }
                else if (parameter == 3)
                {
                    animator.Play("Aim_WalkF");
                }
                break;
            case Character.CharacterState.dodge:
                if (parameter == 0)
                {
                    animator.Play("Dodge_L");
                }
                else if (parameter == 1)
                {
                    animator.Play("Dodge_R");
                }
                else if (parameter == 2)
                {
                    animator.Play("Dodge_B");
                }
                else if (parameter == 3)
                {
                    animator.Play("Dodge_F");
                }
                break;
            case Character.CharacterState.dead:
                animator.Play("Death");
                break;
            case Character.CharacterState.roll:
                animator.Play("Roll");
                break;
            case Character.CharacterState.attack:
                CombatManager.Combat combat = (CombatManager.Combat)parameter;
                animator.Play(combat.ToString(), -1, 0);
                break;
            case Character.CharacterState.adjustPosition:
                animator.Play("Adjust");
                break;
            case Character.CharacterState.hit:
                CombatManager.HitPosition pos = (CombatManager.HitPosition)((parameter / 100) % 10);
                CombatManager.HitDirection dir = (CombatManager.HitDirection)((parameter / 10) % 10);
                CombatManager.HitPower power = (CombatManager.HitPower)((parameter / 1) % 10);
                string animationName = "A_Hit_" + pos.ToString() + "_" + dir.ToString() + "_" + power.ToString();
                animator.Play(animationName,-1,0);
                break;
        }
    }


}
