using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Level Complete, load credits
/// </summary>
public class EndLevelUI : MonoBehaviour
{

    /// <summary>
    /// Go back to the Main Menu
    /// </summary>
    public void Credits()
    {
        SceneManager.LoadSceneAsync("Credits");
    }
}
