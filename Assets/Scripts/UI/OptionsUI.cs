using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsUI : MonoBehaviour
{

    /// <summary>
    /// Go back to the Main Menu
    /// </summary>
    public void BackButton()
    {
        SceneManager.LoadSceneAsync("Title");
        SceneManager.UnloadSceneAsync("Options");
    }
}