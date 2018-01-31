using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Load the tutorial level, once.
/// </summary>
public class LoadTutorial : MonoBehaviour
{
    [SerializeField] private GameObject cam;
    [SerializeField] private AudioSource BGSound;

    private bool loading;
    private bool loaded;
    private bool once;

    // Use this for initialization
    void Start()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        loading = false;
        loaded = false;
        once = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!loading)
        {
            loading = true;

            StartCoroutine(LoadYourAsyncScene());

            Debug.Log("called loading");
        }

        if (loaded && !once)
        {
            once = true;
            BGSound.Stop();
            cam.SetActive(false);
            SceneManager.UnloadSceneAsync("LoadingT");
        }
    }

    /// <summary>
    /// Load function from Unity Documentation
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadYourAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Tutorial", LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Tutorial"));
        loaded = true;
    }
}
