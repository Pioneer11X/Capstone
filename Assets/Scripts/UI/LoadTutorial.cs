using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadTutorial : MonoBehaviour
{
    bool loaded;

	// Use this for initialization
	void Start ()
    {
        loaded = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (!loaded)
        {
            SceneManager.LoadSceneAsync("Tutorial");
            SceneManager.UnloadSceneAsync("LoadingT");

            loaded = true;
        }
	}
}
