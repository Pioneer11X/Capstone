using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Load the tutorial level, once.
/// </summary>
public class LoadTutorial : MonoBehaviour
{
    bool loading;

	// Use this for initialization
	void Start ()
    {
        loading = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (!loading)
        {
            SceneManager.LoadSceneAsync("Tutorial");

            loading = true;
        }
	}
}
