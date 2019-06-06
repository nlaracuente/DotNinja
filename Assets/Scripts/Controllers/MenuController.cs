using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Functions as middle man between the UI buttons and the GameManager
/// </summary>
public class MenuController : MonoBehaviour
{
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
    /// Default menu to closed
    /// Ensures volume sliders match current volume levels
    /// </summary>
    private void Start()
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
    }

    /// <summary>
    /// Triggers the game to start
    /// </summary>
    public void StartGame()
    {
        GameManager.instance.StartGame();
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
