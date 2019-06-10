using UnityEngine;

/// <summary>
/// Functions as middle man between the UI buttons and the GameManager
/// </summary>
public class MenuController : MonoBehaviour
{
    /// <summary>
    /// A reference to the main menu
    /// </summary>
    [SerializeField]
    GameObject m_mainMenuGO;

    /// <summary>
    /// A reference to the level selection menu
    /// </summary>
    [SerializeField]
    GameObject m_levelSelectionMenuGO;

    /// <summary>
    /// A reference to the pause menu
    /// </summary>
    [SerializeField]
    GameObject m_pasueMenuGO;

    /// <summary>
    /// A reference to the level completed menu
    /// </summary>
    [SerializeField]
    LevelCompletionSummary m_completionSummary;

    /// <summary>
    /// The volume slider for controlling the music
    /// </summary>
    [SerializeField]
    VolumeSlider m_musicVolumeSlider;

    /// <summary>
    /// The sound slider for controlling the sound effects
    /// </summary>
    [SerializeField]
    VolumeSlider m_fxVolumeSlider;

    /// <summary>
    /// A reference to the level select controller
    /// </summary>
    LevelSelectController m_levelSelectController;

    /// <summary>
    /// Default menu to closed
    /// Ensures volume sliders match current volume levels
    /// </summary>
    void Start()
    {
        ToggleMenu(false);

        if (m_completionSummary) {
            m_completionSummary.gameObject.SetActive(false);
        }

        if (m_musicVolumeSlider != null) {
            m_musicVolumeSlider.SetValue(AudioManager.instance.MusicVolume);
        }

        if (m_fxVolumeSlider != null) {
            m_fxVolumeSlider.SetValue(AudioManager.instance.FxVolume);
        }

        if (m_mainMenuGO != null) {
            m_mainMenuGO.SetActive(true);
        }

        if (m_levelSelectionMenuGO != null) {
            m_levelSelectController = FindObjectOfType<LevelSelectController>();
            m_levelSelectionMenuGO.SetActive(false);
        }
    }

    /// <summary>
    /// Loads the level selection menu 
    /// </summary>
    public void LoadLevelSelect()
    {
        if (m_mainMenuGO != null && m_levelSelectionMenuGO != null && m_levelSelectController != null) {
            m_mainMenuGO.SetActive(false);
            m_levelSelectionMenuGO.SetActive(true);
            m_levelSelectController.LoadLevelSelection(GameManager.instance.AllLevelProgress);
        } else {
            Debug.LogError("Menu Controller is missing a reference to one or more of the following: " +
                           "main menu go, level selection go, level select controller.\n" + 
                           "Defaulting to level 1 ");
            GameManager.instance.StartGame();
        }
    }

    /// <summary>
    /// Triggers the transition to the main menu
    /// </summary>
    public void MainMenu()
    {
        // This may be a request from the PAUSE menu which means we need to reset the scale
        Time.timeScale = 1f;
        GameManager.instance.MainMenu();
    }

    /// <summary>
    /// Reloads the current level to "re-start" it
    /// </summary>
    public void RestartLevel()
    {
        GameManager.instance.Restartlevel();
    }

    /// <summary>
    /// Triggers the transition into the next level
    /// </summary>
    public void NextLevel()
    {
        GameManager.instance.LoadNextLevel();
    }

    /// <summary>
    /// Terminates the game
    /// </summary>
    public void QuitGame()
    {
        GameManager.instance.QuitGame();
    }

    /// <summary>
    /// Toggles the menu object to open/close
    /// </summary>
    /// <param name="isOpened"></param>
    public void ToggleMenu(bool isOpened)
    {
        if(m_pasueMenuGO != null) {
            m_pasueMenuGO.SetActive(isOpened);
        }

        GameManager.instance.IsGamePaused = isOpened;
    }

    /// <summary>
    /// Sets the music volume
    /// </summary>
    /// <param name="volume"></param>
    public void ChangeMusicVolume(float volume)
    {
        AudioManager.instance.MusicVolume = volume;
    }

    /// <summary>
    /// Sets the fx volume
    /// </summary>
    /// <param name="volume"></param>
    public void ChangeFxVolume(float volume)
    {
        AudioManager.instance.FxVolume = volume;
    }

    /// <summary>
    /// Sets the level completed menu to active
    /// </summary>
    public void ShowLevelCompletedMenu(int level, int totalMoves, int maxMoves)
    {
        if (m_completionSummary) {
            m_completionSummary.gameObject.SetActive(true);
            m_completionSummary.DisplayResults(level, totalMoves, maxMoves);
        } else {
            Debug.LogError("MenuController does not have a reference to the Level Completed Menu.\n" +
                           "Auto moving to next level...");
            NextLevel();
        }
    }
}
