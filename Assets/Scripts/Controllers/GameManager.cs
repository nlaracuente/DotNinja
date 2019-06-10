using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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
    /// The name of the save file
    /// </summary>
    [SerializeField]
    string m_saveFileName = "DotNinja.test";
    string SaveFilePath { get { return string.Format("{0}/{1}", Application.persistentDataPath, m_saveFileName); } }

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
    /// The container for loading and storing the data to save
    /// </summary>
    SavedData m_savedData = new SavedData();
    public LevelProgress[] AllLevelProgress { get { return m_savedData.Levels; } }

    /// <summary>
    /// A reference to the current active menu controller
    /// </summary>
    MenuController m_menuController;
    /// <summary>
    /// Returns a reference to the current menu controller
    /// Menu controllers change per scene
    /// </summary>
    MenuController CurrentMenuController
    {
        get {
            if (m_menuController == null) {
                m_menuController = FindObjectOfType<MenuController>();
            }
            return m_menuController;
        }
    }

    /// <summary>
    /// Holds the routine for loading the level to avoid re-loading it
    /// </summary>
    IEnumerator m_loadLevelRoutine;

    /// <summary>
    /// Holds the routine for loading the next level to avoid re-triggering
    /// </summary>
    IEnumerator m_levelTransitionRoutine;

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
    void Start()
    {
        ApplicationStart();
    }

    /// <summary>
    /// Let's make sure the game is always saved before it closes
    /// </summary>
    void OnApplicationQuit()
    {
        SaveGame();
    }

    /// <summary>
    /// Handles the initial application load
    /// Initializes the AudioManager
    /// Initializes the main MenuController
    /// </summary>
    void ApplicationStart()
    {
        // Default volumes settings to AudioManager's defaults
        float musicVolume = AudioManager.instance.MusicVolume;
        float fxVolume = AudioManager.instance.FxVolume;

        // Saved game loaded
        if (LoadSavedGame()) {
            musicVolume = m_savedData.MusicVolume;
            fxVolume = m_savedData.FxVolume;

        // Create new save data 
        } else { 

            // Level numbers start at 1 so we skip level 0
            // Note: We have more built scenes than levels (i.e MainMenu/Credits)
            for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++) {
                string sceneName = string.Format(m_levelSceneNameFormat, i);

                if (Application.CanStreamedLevelBeLoaded(sceneName)) {
                    m_totalLevels++;
                }
            }

            m_savedData.SetDefaults(musicVolume, fxVolume, m_totalLevels);
        }

        AudioManager.instance.Initialize(musicVolume, fxVolume);
    }

    /// <summary>
    /// Stores the given level's progress in the <see cref="m_savedData"/> to save later
    /// </summary>
    /// <param name="level"></param>
    /// <param name="isUnlocked"></param>
    /// <param name="isPrefect"></param>
    void SetLevelProgress(int level, bool isUnlocked, bool isPrefect = false)
    {
        if (level > 0 && level < m_savedData.Levels.Length) {

            // Do not override if the existing progress is better
            // by checking the given values are the best value to store
            LevelProgress progress = m_savedData.Levels[level];

            // Since these default to FALSE we only update them if they are TRUE
            if (isUnlocked) {
                progress.IsUnlocked = true;
            }

            if (isPrefect) {
                progress.IsPerfect = true;
            }

            m_savedData.Levels[level] = progress;
        }
    }

    /// <summary>
    /// Loads any saved file and 
    /// Returns true when the file is loaded
    /// </summary>
    bool LoadSavedGame()
    {
        if (!File.Exists(SaveFilePath)) {
            return false;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(SaveFilePath, FileMode.Open);
        m_savedData = formatter.Deserialize(stream) as SavedData;
        stream.Close();

        return true;
    }

    /// <summary>
    /// Saves the current progress
    /// </summary>
    public void SaveGame()
    {
        m_savedData.MusicVolume = AudioManager.instance.MusicVolume;
        m_savedData.FxVolume = AudioManager.instance.FxVolume;

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(SaveFilePath, FileMode.Create);

        formatter.Serialize(stream, m_savedData);
        stream.Close();
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
        int level = 1;
        TransitionToLevel(level);
    }

    /// <summary>
    /// Sets the current level to the given level
    /// Triggers a transition to that level
    /// </summary>
    /// <param name="level"></param>
    public void TransitionToLevel(int level)
    {
        CurrentLevel = level;
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
        IsLevelLoaded = false;
        IsLevelCompleted = false;
        IsGamePaused = false;
        TotalMoves = 0;
        m_levelTransitionRoutine = null;
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
        m_loadLevelRoutine = null;
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
        StartCoroutine(ActivePlayer.LevelCompletedAnimationRoutine());

        // Store the results
        LevelController controller = FindObjectOfType<LevelController>();
        MenuController menu = FindObjectOfType<MenuController>();

        bool isUnlocked = true;
        bool isPerfect = TotalMoves <= controller.MaxMoves;

        SetLevelProgress(CurrentLevel, isUnlocked, isPerfect);
        // We also want to update the next level as "unlocked"
        SetLevelProgress(CurrentLevel + 1, isUnlocked);

        SaveGame();

        // Display the results
        menu.ShowLevelCompletedMenu(CurrentLevel, TotalMoves, controller.MaxMoves);
    }

    /// <summary>
    /// Triggers the transition into the next level
    /// Loads the credit if at the last level
    /// </summary>
    public void LoadNextLevel()
    {
        // Already running
        if (m_levelTransitionRoutine != null) {
            return;
        }

        m_levelTransitionRoutine = LoadNextLevelRoutine();
        StartCoroutine(m_levelTransitionRoutine);
    }

    /// <summary>
    /// Handles moving to the next level
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadNextLevelRoutine()
    {
        // Defaults action to credits screen
        Action transitionTo = TransitionToCredits;

        // Switches to loading the level if it can be loaded
        int nextLevel = CurrentLevel + 1;
        if (LevelSceneCanBeLoaded(nextLevel)) {
            CurrentLevel = nextLevel;
            transitionTo = LoadCurrentLevel;
        }

        m_levelTransitionRoutine = FadeScreenAndTransitionTo(transitionTo);
        yield return StartCoroutine(m_levelTransitionRoutine);

        // Ensure nothing else is running
        // This is curcial or else the level loading sequence might overlap when level ending sequence
        StopAllCoroutines();

        m_levelTransitionRoutine = null;
        IsLevelLoaded = false;
    }

    /// <summary>
    /// Triggers the reloading of the current level
    /// </summary>
    public void Restartlevel()
    {
        // Already running
        if (m_levelTransitionRoutine != null) {
            return;
        }

        m_levelTransitionRoutine = RestartLevelRoutine();
        StartCoroutine(m_levelTransitionRoutine);
    }

    /// <summary>
    /// Reloads the current level
    /// </summary>
    /// <returns></returns>
    IEnumerator RestartLevelRoutine()
    {
        // Defaults action to credits screen
        Action transitionTo = LoadCurrentLevel;

        m_levelTransitionRoutine = FadeScreenAndTransitionTo(transitionTo);
        yield return StartCoroutine(m_levelTransitionRoutine);

        // Ensure nothing else is running
        // This is curcial or else the level loading sequence might overlap when level ending sequence
        StopAllCoroutines();

        m_levelTransitionRoutine = null;
        IsLevelLoaded = false;
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
