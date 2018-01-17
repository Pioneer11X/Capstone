using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Player class, extends from PC base class
/// </summary>
public class PC : Humanoid
{
    /// <summary>
    /// Kill player, stop game.
    /// </summary>
    protected override void Die()
    {
        SceneManager.LoadSceneAsync("Death");
    }
}
