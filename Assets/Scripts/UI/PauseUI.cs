using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pause Screen, show/hide menu items
/// </summary>
public class PauseUI : MonoBehaviour
{
    [SerializeField] private GameObject HelpSprite;
    [SerializeField] private GameObject OptionsSprite;

    private Pause pause;

    /// <summary>
    /// Hide the temporary sprites
    /// </summary>
    void Start()
    {
        HelpSprite.SetActive(false);
        OptionsSprite.SetActive(false);

        pause = Pause.Instance;
    }

    /// <summary>
    /// Shows Options
    /// </summary>
    public void OptionsButton()
    {
        HelpSprite.SetActive(false);
        OptionsSprite.SetActive(true);
    }

    /// <summary>
    /// Shows Help
    /// </summary>
    public void HelpButton()
    {
        HelpSprite.SetActive(true);
        OptionsSprite.SetActive(false);
    }

    /// <summary>
    /// Unpause
    /// </summary>
    public void Unpause()
    {
        HelpSprite.SetActive(false);
        OptionsSprite.SetActive(false);

        pause.isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        SceneManager.UnloadSceneAsync("Pause");
    }

    /// <summary>
    /// Go back to the Main Menu
    /// </summary>
    public void MenuButton()
    {
        SceneManager.LoadSceneAsync("Title");
    }
}
