using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;

/// <summary>
/// Help UI
/// </summary>
public class HelpUI : MonoBehaviour
{
    /// <summary>
    /// Update loop, listen for cancel input
    /// </summary>
    private void Update()
    {
        if (CrossPlatformInputManager.GetButtonDown("Cancel")) //Button 1 or 6
        {
            //Debug.Log("Cancel");
            BackButton();
        }
    }

    /// <summary>
    /// Go back to the Main Menu
    /// </summary>
    public void BackButton()
    {
        SceneManager.LoadSceneAsync("Title");
    }

}
