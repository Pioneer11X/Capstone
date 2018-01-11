using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{

    bool loaded;

    // Use this for initialization
    void Start()
    {
        loaded = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!loaded)
        {
            SceneManager.LoadSceneAsync("MainLevel");
            SceneManager.UnloadSceneAsync("LoadingM");

            loaded = true;
        }
    }
}