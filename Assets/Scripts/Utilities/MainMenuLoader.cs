using UnityEngine;

/// <summary>
/// Triggers the main menu to load when the scene is loaded
/// This replaces the need to click on the "play" button for mobile
/// </summary>
public class MainMenuLoader : MonoBehaviour
{
    /// <summary>
    /// Main Menu controller
    /// </summary>
    [SerializeField]
    LevelSelectController m_controller;

    /// <summary>
    /// True when the call to load the level select is made
    /// </summary>
    bool m_loaded = false;

    /// <summary>
    /// Triggers the level select to load
    /// </summary>
    void Update()
    {
        if (!m_controller) {
            m_controller = FindObjectOfType<LevelSelectController>();
        }

        if (!m_loaded && m_controller) {
            m_loaded = true;
            m_controller.LoadLevelSelection(GameManager.instance.AllLevelProgress);
        }
    }
}
