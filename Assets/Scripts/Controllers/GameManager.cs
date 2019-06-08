using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// A public static instance of self
    /// </summary>
    public static GameManager instance;

    /// <summary>
    /// When true only allows one connection per connector at a time
    /// </summary>
    [SerializeField]
    bool m_singleConnections = true;
    public bool SingleConnections { get { return m_singleConnections; } }

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
    public int CurrentLevel { get; set; } = 1;

    /// <summary>
    /// Total levels available
    /// This is exposed to see it in the editor but is auto populated in the <see cref="ApplicationStart"/>
    /// </summary>
    [SerializeField]
    int m_totalLevels = 0;

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
    /// True when the pause menu is opened
    /// </summary>
    public bool IsGamePaused { get; set; } = false;

    /// <summary>
    /// Total moves the player has peformed for the current level
    /// </summary>
    public int TotalMoves { get; set; } = 0;

    /// <summary>
    /// A reference to the current scene fader
    /// </summary>
    SceneFader m_fader;

    /// <summary>
    /// The container for loading and storing the data to save
    /// </summary>
    SavedData m_savedData = new SavedData();

    /// <summary>
    /// Lazy loads the SceneFader as it changes per scene
    /// </summary>
    SceneFader Fader
    {
        get {
            if (m_fader == null)
            {
                m_fader = FindObjectOfType<SceneFader>();
            }

            return m_fader;
        }
    }

    /// <summary>
    /// A reference to the current active player
    /// </summary>
    Player m_player;

    /// <summary>
    /// Lazy loads current player as it changes per scene
    /// </summary>
    public Player ActivePlayer
    {
        get {
            if (m_player == null)
            {
                m_player = FindObjectOfType<Player>();
            }

            return m_player;
        }
    }

    /// <summary>
    /// Holds the routine for loading the level to avoid re-loading it
    /// </summary>
    IEnumerator m_loadLevelRoutine;

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
    /// Trigger initial load logic
    /// </summary>
    private void Start()
    {
        ApplicationStart();
    }

    /// <summary>
    /// Handles the initial application load
    /// </summary>
    void ApplicationStart()
    {
        // Level numbers start at 1
        for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++) {
            string sceneName = string.Format(m_levelSceneNameFormat, i);

            if (Application.CanStreamedLevelBeLoaded(sceneName)) {
                m_totalLevels++;
            }
        }

        m_savedData.SetDefaults(AudioManager.instance.MusicVolume, AudioManager.instance.FxVolume, m_totalLevels);
    }

    /// <summary>
    /// Stores the given level's progress in the <see cref="m_savedData"/> to save later
    /// </summary>
    /// <param name="level"></param>
    /// <param name="isCompleted"></param>
    /// <param name="isPrefect"></param>
    void SetLevelProgress(int level, bool isCompleted, bool isPrefect)
    {
        if (level > 0 && level < m_savedData.Progress.Length) {
            m_savedData.Progress[level] = new LevelProgress() {
                IsCompleted = isCompleted,
                IsPerfect = isPrefect
            };
        }
    }

    /// <summary>
    /// Terminates the application
    /// Todo: Remove when done debugging
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Resets the level counter to 1 and loads the level
    /// </summary>
    public void StartGame()
    {
        CurrentLevel = 1;
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
        // Already running
        if (m_loadLevelRoutine != null) {
            return;
        }

        // Ensures game manager forgets about a previous level
        ResetLevel();

        // Ensure the current level matches the scene name
        UpdateCurrentLevelNumber();

        m_loadLevelRoutine = LoadLevelRoutine();
        StartCoroutine(m_loadLevelRoutine);
    }

    /// <summary>
    /// Resets level references, flags and counters
    /// </summary>
    private void ResetLevel()
    {
        m_fader = null;
        IsLevelLoaded = false;
        IsLevelCompleted = false;
        IsGamePaused = false;
        TotalMoves = 0;
    }

    /// <summary>
    /// Uses the name of the active scene to determine the current level number
    /// </summary>
    private void UpdateCurrentLevelNumber()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string levelNumber = Regex.Match(sceneName, @"\d+").Value;
        CurrentLevel = int.Parse(levelNumber);
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
    /// Triggers the player's death routine
    /// </summary>
    public void TriggerPlayerDeath()
    {
        ActivePlayer.TriggerDeath();
    }

    /// <summary>
    /// Handles the level completed sequence
    /// </summary>
    public void LevelCompleted(Door door)
    {
        IsLevelCompleted = true;
        IsGamePaused = false;
        StartCoroutine(LevelCompletedRoutine(door));
    }

    /// <summary>
    /// Plays the door animations, fades the screen, and transitions to the next level
    /// </summary>
    /// <param name="door"></param>
    /// <returns></returns>
    IEnumerator LevelCompletedRoutine(Door door)
    {
        yield return StartCoroutine(door.OpenRoutine());

        // Make the player exit
        AudioManager.instance.PlayLevelCompletedSound(ActivePlayer.transform);
        yield return StartCoroutine(ActivePlayer.LevelCompletedAnimationRoutine());


        // Display the results
        LevelController controller = FindObjectOfType<LevelController>();
        MenuController menu = FindObjectOfType<MenuController>();
        menu.ShowLevelCompletedMenu(CurrentLevel, TotalMoves, controller.MaxMoves);

        // Store the results
        bool isCompleted = true;
        bool isPerfect = TotalMoves <= controller.MaxMoves;
        SetLevelProgress(CurrentLevel, isCompleted, isPerfect);
    }

    /// <summary>
    /// Triggers the transition into the next level
    /// Loads the credit if at the last level
    /// </summary>
    public void LoadNextLevel()
    {
        // Defaults action to credits screen
        Action transitionTo = TransitionToCredits;

        // Switches to loading the level if it can be loaded
        int nextLevel = CurrentLevel + 1;
        if (LevelSceneCanBeLoaded(nextLevel)) {
            CurrentLevel = nextLevel;
            transitionTo = LoadCurrentLevel;
        }

        StartCoroutine(FadeScreenAndTransitionTo(transitionTo));

        // Not always reset
        // This is a temporary hack
        IsLevelCompleted = false;
        IsGamePaused = false;
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
        string levelName = string.Format(m_levelSceneNameFormat, CurrentLevel);
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
