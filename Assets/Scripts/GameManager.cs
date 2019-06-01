using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// A public static instance of self
    /// </summary>
    public static GameManager instance;

    /// <summary>
    /// How long the fade in effect lasts
    /// </summary>
    [SerializeField]
    float m_fadeInDelay = 1f;

    /// <summary>
    /// How long the fade out effect lasts
    /// </summary>
    [SerializeField]
    float m_fadeOutDelay = 2f;

    /// <summary>
    /// Keeps track of the current level
    /// </summary>
    [SerializeField]
    int m_currentLevel = 1;

    /// <summary>
    /// The name of the scene for the main menu
    /// </summary>
    [SerializeField]
    string m_mainMenuSceneName = "MainMenu";

    /// <summary>
    /// The format for scene level names
    /// </summary>
    [SerializeField]
    string m_levelSceneNameFormat = "Level_{0}";

    /// <summary>
    /// The name of the scene for the end credits
    /// </summary>
    [SerializeField]
    string m_creditsSceneName = "Credits";

    /// <summary>
    /// True once the level is started
    /// </summary>
    public bool IsLevelLoaded { get; private set; } = false;

    /// <summary>
    /// True when the player reaches the door and has the key
    /// </summary>
    public bool IsLevelCompleted { get; private set; } = false;

    /// <summary>
    /// A reference to the current scene fader
    /// </summary>
    SceneFader m_fader;

    /// <summary>
    /// Lazy loads the SceneFader as it changes per scene
    /// </summary>
    SceneFader Fader {
        get {
            if (m_fader == null)
            {
                m_fader = FindObjectOfType<SceneFader>();
            }

            return m_fader;
        }
    }

    /// <summary>
    /// Sets up instance
    /// </summary>
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Terminates the application
    /// Todo: Remove when done debugging
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)){
            Application.Quit();
        }
    }

    /// <summary>
    /// Resets the level counter to 1 and loads the level
    /// </summary>
    public void StartGame()
    {
        m_currentLevel = 1;
        LoadCurrentLevel();
    }

    /// <summary>
    /// Transitions to the main menu scene
    /// </summary>
    public void MainMenu()
    {
        TransitionToScene(m_mainMenuSceneName);
    }

    /// <summary>
    /// Triggers the level load routine
    /// </summary>
    public void LoadLevel()
    {
        IsLevelLoaded = false;
        IsLevelCompleted = false;
        StartCoroutine(LoadLevelRoutine());
    }

    /// <summary>
    /// Fades the screen in and enables gameplay
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadLevelRoutine()
    {
        yield return StartCoroutine(Fader.FadeRoutine(1f, 0f, m_fadeInDelay));
        IsLevelLoaded = true;
    }

    /// <summary>
    /// Handles the level completed sequence
    /// </summary>
    public void LevelCompleted()
    {
        IsLevelCompleted = true;

        // Defaults action to credits screen
        Action transitionTo = TransitionToCredits;

        // Switches to loading the level if it can be loaded
        int nextLevel = m_currentLevel + 1;
        if (LevelSceneCanBeLoaded(nextLevel))
        {
            m_currentLevel = nextLevel;
            transitionTo = LoadCurrentLevel;
        }

        StartCoroutine(FadeScreenAndTransitionTo(transitionTo));
    }

    /// <summary>
    /// True when the given level number is a scene that can be loaded
    /// </summary>
    /// <param name="levelNumber"></param>
    /// <returns></returns>
    bool LevelSceneCanBeLoaded(int levelNumber)
    {
        string levelName = string.Format(m_levelSceneNameFormat, levelNumber);
        return Application.CanStreamedLevelBeLoaded(levelName);
    }

    /// <summary>
    /// Triggers the screen to fade in and then invokes the action given
    /// </summary>
    /// <param name="transitionTo"></param>
    /// <returns></returns>
    IEnumerator FadeScreenAndTransitionTo(Action transitionTo)
    {
        yield return StartCoroutine(Fader.FadeRoutine(0f, 1f, m_fadeOutDelay));
        transitionTo?.Invoke();
    }

    /// <summary>
    /// Loads the credits scene
    /// </summary>
    public void TransitionToCredits()
    {
        TransitionToScene(m_creditsSceneName);
    }


    /// <summary>
    /// Loads the current level
    /// </summary>
    public void LoadCurrentLevel()
    { 
        string levelName = string.Format(m_levelSceneNameFormat, m_currentLevel);
        TransitionToScene(levelName);
    }


    /// <summary>
    /// Loads the given scene if it can be loaded
    /// </summary>
    /// <param name="sceneName"></param>
    void TransitionToScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogErrorFormat("Scene '{0}' cannot be loaded", sceneName);

            // Failsafe - reload the current one
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
