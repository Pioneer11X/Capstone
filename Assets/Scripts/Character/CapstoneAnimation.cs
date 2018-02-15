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
        string animationName_0 = "";
        string animationName_1 = "";
        float duration = 0.0f;
        int layerOverride = 0;
        
        //Debug.Log(state);
        switch (state)
        {
            case Character.CharacterState.idle_OutCombat:
                //animator.Play("Idle_OutCombat");
                animationName_0 = animationName_1 = "Idle_OutCombat";
                duration = 0.2f;
                break;
            case Character.CharacterState.idle_InCombat:
                animationName_0 = animationName_1 = "Idle_InCombat";
                duration = 0.2f;
                break;
            case Character.CharacterState.walk:
                animationName_0 = animationName_1 = "Walk";
                duration = 0.15f;
                break;
            case Character.CharacterState.run:
                animationName_0 = animationName_1 = "Run";
                duration = 0.1f;
                break;
            case Character.CharacterState.jump_up:
                animationName_0 = animationName_1 = "Jump_Up";
                break;
            case Character.CharacterState.jump_air:
                animationName_0 = animationName_1 = "Jump_Air";
                break;
            case Character.CharacterState.jump_down:
                animationName_0 = animationName_1 = "Jump_Down";
                break;
            case Character.CharacterState.draw_Gun:
                animationName_0 = animationName_1 = "Gun_Draw";
                break;
            case Character.CharacterState.holster_Gun:
                animationName_0 = animationName_1 = "Gun_Holster";
                break;
            case Character.CharacterState.shoot:
                animationName_0 = animationName_1 = "Gun_Shoot";
                break;
            case Character.CharacterState.aim_Idle:
                animationName_0 = animationName_1 = "Gun_Idle";
                break;
            case Character.CharacterState.aim_Move:
                if (parameter == 0)
                {
                    animationName_0 = "Aim_Sidestep_L";
                    animationName_1 = "Gun_Idle"; 
                }
                else if (parameter == 1)
                {
                    animationName_0 = "Aim_Sidestep_R";
                    animationName_1 = "Gun_Idle";
                }
                else if (parameter == 2)
                {
                    animationName_0 = "Aim_WalkB";
                    animationName_1 = "Gun_Idle";
                }
                else if (parameter == 3)
                {
                    animationName_0 = "Aim_WalkF";
                    animationName_1 = "Gun_Idle";
                }
                break;
            case Character.CharacterState.dodge:
                if (parameter == 0)
                {
                    animationName_0 = "Dodge_L";
                }
                else if (parameter == 1)
                {
                    animationName_0 = "Dodge_R";
                }
                else if (parameter == 2)
                {
                    animationName_0 = "Dodge_B";
                }
                else if (parameter == 3)
                {
                    animationName_0 = "Dodge_F";
                }
                break;
            case Character.CharacterState.dead:
                animationName_0 = animationName_1 = "Death";
                break;
            case Character.CharacterState.roll:
                animationName_0 = animationName_1 = "Roll";
                break;
            case Character.CharacterState.attack:
                CombatManager.Combat combat = (CombatManager.Combat)parameter;
                //animator.Play(combat.ToString(), -1, 0);
                animationName_0 = animationName_1 = combat.ToString();
                duration = 0;
                layerOverride = -1;
                break;
            case Character.CharacterState.adjustPosition:
                animationName_0 = animationName_1 = "Adjust";
                break;
            case Character.CharacterState.hit:
                CombatManager.HitPosition pos = (CombatManager.HitPosition)((parameter / 100) % 10);
                CombatManager.HitDirection dir = (CombatManager.HitDirection)((parameter / 10) % 10);
                CombatManager.HitPower power = (CombatManager.HitPower)((parameter / 1) % 10);
                string animationName = "A_Hit_" + pos.ToString() + "_" + dir.ToString() + "_" + power.ToString();
                //animator.Play(animationName,-1,0);
                animationName_0 = animationName_1 = animationName;
                duration = 0;
                layerOverride = -1;
                break;
        }// End Switch

        if (layerOverride == 0)
        {
            animator.CrossFade(animationName_0, duration, 0);
            animator.CrossFade(animationName_1, duration, 1);
        }
        else if(layerOverride == -1)
        {
            animator.CrossFade(animationName_0, 0.0f, 0, 0);
            animator.CrossFade(animationName_1, 0.0f, 1, 0);
        }

    }


}
