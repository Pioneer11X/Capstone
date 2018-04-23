using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private AudioSource BGM_Source;
    [SerializeField] private AudioSource BGM_Mix_Source;

    [SerializeField] private AudioClip BGM_Clip;
    [SerializeField] private AudioClip BGA_Clip;
    [SerializeField] private AudioClip BGM_Low_Health_Clip;
    [SerializeField] private AudioClip BGM_Combat_Clip;


    private int audioCounterDown;
    private int audioCounterUp;
    private int counter;

    private bool isMainTrack = true;
    private bool combatTrack = false;
    private bool isHealthTrack = true;

    // Use this for initialization
    void Start ()
    {
        audioCounterDown = 0;
        audioCounterUp = 0;
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
    /// Switch to the low health BGM
    /// </summary>
    public void PlayLowHealthTrack()
    {
        if (isHealthTrack)
        {
            isHealthTrack = false;
            audioCounterDown = 0;
            audioCounterUp = 0;
            BGM_Mix_Source.clip = BGM_Low_Health_Clip;
            BGM_Mix_Source.Play();

            StartCoroutine(SpinUpAudio(BGM_Mix_Source));
            StartCoroutine(SpinDownAudio(BGM_Source));
        }
    }

    public void PlayGoodHealthTrack()
    {
        if (!isHealthTrack)
        {
            isHealthTrack = true;
            audioCounterDown = 0;
            audioCounterUp = 0;

            StartCoroutine(SpinUpAudio(BGM_Source));
            StartCoroutine(SpinDownAudio(BGM_Mix_Source));
        }

    }

    public void PlayMainTrack()
    {
        if (!isMainTrack && combatTrack)
        {
            isMainTrack = true;
            combatTrack = false;
            BGM_Source.clip = BGM_Clip;
            BGM_Source.volume = 1.0f;
            BGM_Source.Play();
        }
        
    }
    

    public void PlayCombatTrack()
    {
        if (isMainTrack && !combatTrack)
        {
            isMainTrack = false;
            combatTrack = true;
            BGM_Source.clip = BGM_Combat_Clip;
            BGM_Source.volume = 1.0f;
            BGM_Source.Play();
        }

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

        StopAllCoroutines();

        SceneManager.LoadSceneAsync("EndLevel");
    }


    public IEnumerator SpinDownAudio(AudioSource source)
    {
        while (audioCounterDown < 180)
        {
            audioCounterDown++;
            source.volume -= 0.017f;
            yield return null;
        }

        source.volume = 0.0f;

        source.Stop();
    }

    public IEnumerator SpinUpAudio(AudioSource source)
    {
        source.Play();

        while (audioCounterUp < 180)
        {
            audioCounterUp++;
            source.volume += 0.017f;
            yield return null;
        }

        source.volume = 1.0f;
    }

}
