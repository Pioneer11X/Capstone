using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;

/// <summary>
/// Handles the Title Screen UI Buttons
/// </summary>
public class TitleUI : MonoBehaviour
{
    [SerializeField] private GameObject MainButtons;
    [SerializeField] private GameObject SelectButtons;
    [SerializeField] private Button FirstButton;
    [SerializeField] private Button SubButtonOne;
    [SerializeField] private AudioSource SFX;
    [SerializeField] private AudioClip Button_SFX1;
    [SerializeField] private AudioClip Button_SFX2;

    private bool titleMenu; // Is the title screen on the title menu buttons or sub buttons.

	// Use this for initialization
	void Start ()
    {
        Time.timeScale = 1;
        MainButtons.SetActive(true);
        SelectButtons.SetActive(false);
        titleMenu = true;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!titleMenu)
        {
            if (CrossPlatformInputManager.GetButtonDown("Cancel")) //Button 1 or 6
            {
                //Debug.Log("Cancel");
                SFX.PlayOneShot(Button_SFX1);
                BackButton();
            }
        }
        else
        {
            if (CrossPlatformInputManager.GetButtonDown("Cancel")) //Button 1 or 6
            {
                //Debug.Log("Exit Game");
                ExitButton();
            }
        }
    }

    /// <summary>
    /// Start Game Button
    /// Brings up the launch game menu
    /// </summary>
    public void StartButton()
    {
        // Hide regular menu buttons and show option to load tutorial, game, or go back.
        MainButtons.SetActive(false);
        SelectButtons.SetActive(true);
        SubButtonOne.Select();
        titleMenu = false;
        SFX.PlayOneShot(Button_SFX1);
    }

    /// <summary>
    /// Load the loading scene and then the tutorial level
    /// </summary>
    public void LoadTutorial()
    {
        SFX.PlayOneShot(Button_SFX2);
        StartCoroutine(WaitForAudioBeforeScene(1, "LoadingT"));
    }

    /// <summary>
    /// Load the loading scene and then the main level
    /// </summary>
    public void LoadMainLevel()
    {
        SFX.PlayOneShot(Button_SFX2);
        StartCoroutine(WaitForAudioBeforeScene(1, "LoadingM"));
    }

    /// <summary>
    /// Loads Options Scene
    /// </summary>
    public void OptionsButton()
    {
        SFX.PlayOneShot(Button_SFX1);
        StartCoroutine(WaitForAudioBeforeScene(1, "Options"));
    }

    /// <summary>
    /// Load Help Scene
    /// </summary>
    public void HelpButton()
    {
        SFX.PlayOneShot(Button_SFX1);
        StartCoroutine(WaitForAudioBeforeScene(1, "Help"));
    }

    /// <summary>
    /// Loads Credits Scene
    /// </summary>
    public void CreditsButton()
    {
        SFX.PlayOneShot(Button_SFX1);
        StartCoroutine(WaitForAudioBeforeScene(1, "Credits"));
    }

    /// <summary>
    /// Cancel from starting a game level
    /// </summary>
    public void BackButton()
    {
        SFX.PlayOneShot(Button_SFX1);
        MainButtons.SetActive(true);
        SelectButtons.SetActive(false);
        FirstButton.Select();
        titleMenu = true;
    }

    /// <summary>
    /// Exits Game
    /// </summary>
    public void ExitButton()
    {
        SFX.PlayOneShot(Button_SFX2);
        StartCoroutine(WaitForAudioBeforeEnd(2));
    }

    /// <summary>
    /// Delay the application close while audio plays
    /// </summary>
    /// <param name="delay">How long to wait before exiting program</param>
    /// <returns></returns>
    IEnumerator WaitForAudioBeforeEnd(float delay)
    {
        yield return new WaitForSeconds(delay);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Delay the scene change to allow audio to play
    /// </summary>
    /// <param name="delay">How long to delay</param>
    /// <returns></returns>
    IEnumerator WaitForAudioBeforeScene(float delay, string scene)
    {
        yield return new WaitForSeconds(delay);

        SceneManager.LoadSceneAsync(scene);
    }
}
