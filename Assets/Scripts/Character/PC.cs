using UnityEngine;
using UnityEngine.SceneManagement;

public class PC : Humanoid
{

    /// <summary>
    /// Kill player, stop game.
    /// </summary>
    protected override void Die()
    {
        SceneManager.LoadSceneAsync("Death");

#if UNITY_EDITOR
        //UnityEditor.EditorApplication.isPlaying = false;
#else
        //Call game over screen here
#endif
    }
}
