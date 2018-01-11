using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Load the main level, once.
/// </summary>
public class LoadLevel : MonoBehaviour
{

    bool loading;

    // Use this for initialization
    void Start()
    {
        loading = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!loading)
        {
            SceneManager.LoadSceneAsync("MainLevel");

            loading = true;
        }
    }
}