using UnityEngine;

/// <summary>
/// Pause State
/// Used for in game menu options
/// </summary>
public class Pause : MonoBehaviour
{
    private bool isPaused;

    public bool IsPaused
    {
        get { return isPaused; }
        set { isPaused = value; }
    }

    // Make global
    public static Pause Instance
    {
        get;
        set;
    }

    // Create first and only instance of
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        Instance = this;
    }

    // Set default state to not paused
    void Start()
    {
        isPaused = false;
    }
}
