using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;

/// <summary>
/// Credits UI
/// </summary>
public class CreditsUI : MonoBehaviour
{
    /// <summary>
    /// Update loop, listen for continue input
    /// </summary>
    private void Update()
    {
        if (CrossPlatformInputManager.GetButtonDown("Submit")) //Button 1 or 6
        {
            //Debug.Log("Submit");
            MenuButton();
        }
    }

    /// <summary>
    /// Go back to the Main Menu
    /// </summary>
    public void MenuButton()
    {
        SceneManager.LoadSceneAsync("Title");
    }
}