using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    private int counter;

    // Use this for initialization
    void Start ()
    {
        counter = 0;
    }
	
	// Update is called once per frame
	void Update ()
    {	}

    public void End()
    {
        StartCoroutine(WaitToEnd());
    }

    /// <summary>
    /// Call Load on EndLevel
    /// </summary>
    /// <returns></returns>
    public IEnumerator WaitToEnd()
    {
        while (counter < 60)
        {
            Debug.Log("running");
            counter++;
            yield return null;
        }

        SceneManager.LoadSceneAsync("EndLevel");
    }
}
