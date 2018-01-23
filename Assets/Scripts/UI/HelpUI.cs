using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;

/// <summary>
/// Help UI
/// </summary>
public class HelpUI : MonoBehaviour
{
    [SerializeField] private AudioSource SFX;
    [SerializeField] private AudioClip Button_SFX1;

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
