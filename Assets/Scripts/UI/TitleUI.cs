using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the Title Screen UI Buttons
/// </summary>
public class TitleUI : MonoBehaviour
{
    [SerializeField] private GameObject MainButtons;
    [SerializeField] private GameObject SelectButtons;

	// Use this for initialization
	void Start ()
    {
        MainButtons.SetActive(true);
        SelectButtons.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
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
    }

    /// <summary>
    /// Exits Game
    /// </summary>
    public void ExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Call exit here
#endif
    }
}
