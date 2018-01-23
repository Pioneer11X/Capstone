using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;

/// <summary>
/// Credits UI
/// </summary>
public class CreditsUI : MonoBehaviour
{
    [SerializeField] private AudioSource SFX;
    [SerializeField] private AudioClip Button_SFX1;

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
        StartCoroutine(WaitForAudioBeforeScene(1.5f, "Title"));
    }

    /// <summary>
    /// Delay the scene change to allow audio to play
    /// </summary>
    /// <param name="delay">How long to delay</param>
    /// <returns></returns>
    IEnumerator WaitForAudioBeforeScene(float delay, string scene)
    {
        SFX.PlayOneShot(Button_SFX1);

        yield return new WaitForSeconds(delay);

        SceneManager.LoadSceneAsync(scene);
    }
}