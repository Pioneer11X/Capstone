using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;

/// <summary>
/// Level Complete, load credits
/// </summary>
public class EndLevelUI : MonoBehaviour
{
    /// <summary>
    /// Update loop, listen for continue input
    /// </summary>
    private void Update()
    {
        if (CrossPlatformInputManager.GetButtonDown("Submit")) //Button 1 or 6
        {
            //Debug.Log("Submit");
            Credits();
        }
    }

    /// <summary>
    /// Go back to the Main Menu
    /// </summary>
    public void Credits()
    {
        SceneManager.LoadSceneAsync("Credits");
    }
}
