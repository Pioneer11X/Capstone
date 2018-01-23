using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Death Scene
/// Buttons to go back to Menu, Turtorial, or Main Level
/// </summary>
public class DeathUI : MonoBehaviour
{
    [SerializeField] private AudioSource SFX;
    [SerializeField] private AudioClip Button_SFX1;

    /// <summary>
    /// Initial stuff
    /// </summary>
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Go back to the Main Menu
    /// </summary>
    public void Credits()
    {
        StartCoroutine(WaitForAudioBeforeScene(1.5f, "Credits"));
    }

    /// <summary>
    /// Load the loading scene and then the tutorial level
    /// </summary>
    public void LoadTutorial()
    {
        StartCoroutine(WaitForAudioBeforeScene(1.5f, "LoadingT"));
    }

    /// <summary>
    /// Load the loading scene and then the main level
    /// </summary>
    public void LoadMainLevel()
    {
        StartCoroutine(WaitForAudioBeforeScene(1.5f, "LoadingM"));
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
