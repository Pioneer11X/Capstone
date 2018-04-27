using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;

/// <summary>
/// Pause Screen, show/hide menu items
/// </summary>
public class PauseUI : MonoBehaviour
{
    [SerializeField] private GameObject HelpSprite;
    [SerializeField] private GameObject OptionsSprite;
    [SerializeField] private GameObject EventSystemObject;
    [SerializeField] private Text       OptionsText;

    private Pause pause;

    /// <summary>
    /// Hide the temporary sprites
    /// </summary>
    private void Start()
    {
        HelpSprite.SetActive(false);
        OptionsSprite.SetActive(false);
        OptionsText.enabled = false;
        EventSystemObject.SetActive(true);

        pause = Pause.Instance;
    }

    /// <summary>
    /// Update loop, listen for cancel input
    /// </summary>
    private void Update()
    {
        if (CrossPlatformInputManager.GetButtonDown("Cancel")) //Button 1 or 6
        {
            //Debug.Log("Unpause");
            Unpause();
        }
    }

    /// <summary>
    /// Shows Options
    /// </summary>
    public void OptionsButton()
    {
        HelpSprite.SetActive(false);
        OptionsSprite.SetActive(true);
        OptionsText.enabled = true;
    }

    /// <summary>
    /// Shows Help
    /// </summary>
    public void HelpButton()
    {
        HelpSprite.SetActive(true);
        OptionsSprite.SetActive(false);
        OptionsText.enabled = false;
    }

    /// <summary>
    /// Unpause
    /// </summary>
    public void Unpause()
    {
        HelpSprite.SetActive(false);
        OptionsSprite.SetActive(false);
        OptionsText.enabled = false;

        pause.IsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
        SceneManager.UnloadSceneAsync("Pause");
    }

    /// <summary>
    /// Go back to the Main Menu
    /// </summary>
    public void MenuButton()
    {
        EventSystemObject.SetActive(false);
        SceneManager.LoadSceneAsync("Title");
        Destroy(GameObject.FindGameObjectWithTag("LevelManager"));
    }
}
