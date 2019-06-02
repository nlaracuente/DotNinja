using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Toggles the pause menu on/off
/// </summary>
[RequireComponent(typeof(Image), typeof(Button))]
public class MenuButtonIcon : MonoBehaviour
{
    /// <summary>
    /// The sprite that indicates the menu will open when clicked
    /// </summary>
    [SerializeField]
    Sprite m_openMenuSprite;

    /// <summary>
    /// The sprite that indicates the menu will close when clicked
    /// </summary>
    [SerializeField]
    Sprite m_closeMenuSprite;

    /// <summary>
    /// True when the menu is opened
    /// </summary>
    bool m_isMenuOpened = false;

    /// <summary>
    /// A reference to the menu controller
    /// </summary>
    MenuController m_menuController;

    /// <summary>
    /// A reference to the UI image
    /// </summary>
    Image m_image;

    /// <summary>
    /// A reference to the UI image
    /// </summary>
    Button m_button;

    /// <summary>
    /// Set references
    /// </summary>
    private void Start()
    {
        m_image = GetComponent<Image>();
        m_button = GetComponent<Button>();
        m_menuController = FindObjectOfType<MenuController>();
    }

    /// <summary>
    /// Disables this option while the game is in a state where this cannot be accessed
    /// </summary>
    private void Update()
    {
        m_button.interactable = GameManager.instance.IsLevelLoaded && 
                               !GameManager.instance.IsLevelCompleted;

        // Force a menu close as we are in a state where it should not be opened 
        if (!m_button.interactable && m_isMenuOpened) {
            OnClicked();
        }
    }

    /// <summary>
    /// Toggles the menu to open/close
    /// </summary>
    public void OnClicked()
    {
        m_isMenuOpened = !m_isMenuOpened;
        m_menuController.ToggleMenu(m_isMenuOpened);

        if (m_isMenuOpened) {
            Time.timeScale = 0f;
            m_image.sprite = m_closeMenuSprite;
        } else {
            Time.timeScale = 1f;
            m_image.sprite = m_openMenuSprite;
        }
    }
}
