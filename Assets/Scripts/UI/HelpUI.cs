using UnityEngine;
using UnityEngine.SceneManagement;

public class HelpUI : MonoBehaviour
{

    /// <summary>
    /// Go back to the Main Menu
    /// </summary>
    public void BackButton()
    {
        SceneManager.LoadSceneAsync("Title");
        SceneManager.UnloadSceneAsync("Help");
    }

}
