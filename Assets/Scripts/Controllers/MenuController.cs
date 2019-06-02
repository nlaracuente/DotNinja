using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Functions as middle man between the UI buttons and the GameManager
/// </summary>
public class MenuController : MonoBehaviour
{
    /// <summary>
    /// A reference to the menu game object
    /// </summary>
    [SerializeField]
    GameObject m_menuGO;

    /// <summary>
    /// Default menu to closed
    /// </summary>
    private void Start()
    {
        ToggleMenu(false);
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
    /// Toggles the menu object to open/close
    /// </summary>
    /// <param name="isOpened"></param>
    public void ToggleMenu(bool isOpened)
    {
        if(m_menuGO != null) {
            m_menuGO.SetActive(isOpened);
        }
    }
}
