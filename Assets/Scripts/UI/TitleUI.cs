using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;

/// <summary>
/// Handles the Title Screen UI Buttons
/// </summary>
public class TitleUI : MonoBehaviour
{
    [SerializeField] private GameObject MainButtons;
    [SerializeField] private GameObject SelectButtons;
    [SerializeField] private Button FirstButton;
    [SerializeField] private Button SubButtonOne;

    private bool titleMenu; // Is the title screen on the title menu buttons or sub buttons.

	// Use this for initialization
	void Start ()
    {
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
    }

    /// <summary>
    /// Load the loading scene and then the tutorial level
    /// </summary>
    public void LoadTutorial()
    {
        SceneManager.LoadSceneAsync("LoadingT");
    }

    /// <summary>
    /// Load the loading scene and then the main level
    /// </summary>
    public void LoadMainLevel()
    {
        SceneManager.LoadSceneAsync("LoadingM");
    }

    /// <summary>
    /// Loads Options Scene
    /// </summary>
    public void OptionsButton()
    {
        SceneManager.LoadSceneAsync("Options");
    }

    /// <summary>
    /// Load Help Scene
    /// </summary>
    public void HelpButton()
    {
        SceneManager.LoadSceneAsync("Help");
    }

    /// <summary>
    /// Loads Credits Scene
    /// </summary>
    public void CreditsButton()
    {
        SceneManager.LoadSceneAsync("Credits");
    }

    /// <summary>
    /// Cancel from starting a game level
    /// </summary>
    public void BackButton()
    {
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
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
