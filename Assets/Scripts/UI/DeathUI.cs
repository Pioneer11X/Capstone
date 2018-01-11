using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Death Scene
/// Buttons to go back to Menu, Turtorial, or Main Level
/// </summary>
public class DeathUI : MonoBehaviour
{

    /// <summary>
    /// Go back to the Main Menu
    /// </summary>
    public void Credits()
    {
        SceneManager.LoadSceneAsync("Credits");
    }

    /// <summary>
    /// Load the loading scene and then the tutorial level
    /// </summary>
    public void LoadTutorial()
    {
        SceneManager.LoadSceneAsync("LoadingT");
    }

    /// <summary>
    /// Load the loading scene and then the main level
    /// </summary>
    public void LoadMainLevel()
    {
        SceneManager.LoadSceneAsync("LoadingM");
    }
}
