using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Load the main level, once.
/// </summary>
public class LoadLevel : MonoBehaviour
{
    [SerializeField] private GameObject cam;
    [SerializeField] private AudioSource audio;

    private bool loading;
    private bool loaded;
    private bool once;

    // Use this for initialization
    void Start()
    {
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

        if(loaded && !once)
        {
            once = true;
            audio.Stop();
            cam.SetActive(false);
            SceneManager.UnloadSceneAsync("LoadingM");
        }
    }

    /// <summary>
    /// Load function from Unity Documentation
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadYourAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainLevel", LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainLevel"));
        loaded = true;
    }
}