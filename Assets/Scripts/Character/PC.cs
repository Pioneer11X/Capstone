using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PC : Humanoid
{

    /// <summary>
    /// Kill player, stop game.
    /// </summary>
    protected override void Die()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        //Call game over screen here
#endif
    }
}
