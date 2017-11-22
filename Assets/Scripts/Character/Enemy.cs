using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Humanoid
{
    /// <summary>
    /// Kill Enemey, remove them from the scene.
    /// </summary>
    protected override void Die()
    {
        Destroy(gameObject);
    }
}
