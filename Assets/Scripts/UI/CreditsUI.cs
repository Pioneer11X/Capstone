using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsUI : MonoBehaviour
{

    /// <summary>
    /// Go back to the Main Menu
    /// </summary>
    public void MenuButton()
    {
        SceneManager.LoadSceneAsync("Title");
        SceneManager.UnloadSceneAsync("Credits");
    }
}