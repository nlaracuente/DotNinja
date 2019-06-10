using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The icon that appears on the level selection screens
/// Controls the states of button as well as the image for the button and level number
/// </summary>
public class LevelSelectionButton : MonoBehaviour
{
    /// <summary>
    /// The button component to disable/enable
    /// </summary>
    [SerializeField]
    Button m_button;

    /// <summary>
    /// The image component to update the sprite based on level progress
    /// </summary>
    [SerializeField]
    Image m_image;

    /// <summary>
    /// The text component to indicate which level this is
    /// </summary>
    [SerializeField]
    Text m_levelNumberText;

    /// <summary>
    /// The sprite that represents the level is locked
    /// </summary>
    [SerializeField]
    Sprite m_lockedSprite;

    /// <summary>
    /// The sprite that represents the level is available to play
    /// </summary>
    [SerializeField]
    Sprite m_unlockedSprite;    

    /// <summary>
    /// The sprite that represents the level was completed with a perfect score
    /// </summary>
    [SerializeField]
    Sprite m_perfectSprite;

    /// <summary>
    /// Text color for when the mouse is over this button
    /// </summary>
    [SerializeField]
    Color m_highlightedTextColor = Color.red;

    /// <summary>
    /// Text color for when the mouse exits the button
    /// </summary>
    [SerializeField]
    Color m_normalTextColor = Color.black;

    /// <summary>
    /// The level this represents
    /// </summary>
    int m_level;

    /// <summary>
    /// Initializes the button as locked, not interactible, and with no level number
    /// </summary>
    void Start()
    {
        if (!m_button) {
            m_button = GetComponentInChildren<Button>();
        }

        if (!m_image) {
            m_image = GetComponentInChildren<Image>();
        }

        if (!m_levelNumberText) {
            m_levelNumberText = GetComponentInChildren<Text>();
        }

        m_button.interactable = false;
        m_image.sprite = m_lockedSprite;
        m_levelNumberText.text = "";
    }

    /// <summary>
    /// Highlight
    /// </summary>
    void OnMouseEnter()
    {
        m_levelNumberText.color = m_highlightedTextColor;
    }

    /// <summary>
    /// Reset color
    /// </summary>
    void OnMouseExit()
    {
        m_levelNumberText.color = m_normalTextColor;
    }

    /// <summary>
    /// Reset color
    /// </summary>
    void OnMouseDown()
    {
        m_levelNumberText.color = m_normalTextColor;
    }

    /// <summary>
    /// Changes the appereance and the level this buttton transitions to when clicked
    /// </summary>
    /// <param name="level"></param>
    /// <param name="isUnlocked"></param>
    /// <param name="isPerfect"></param>
    public void Initialize(int level, bool isUnlocked, bool isPerfect)
    {
        m_level = level;

        if (isUnlocked) {
            m_button.interactable = true;
            m_image.sprite = m_unlockedSprite;
        }

        if (isPerfect) {
            m_image.sprite = m_perfectSprite;
        }
    }

    /// <summary>
    /// Disables the button so that it can only be clicked once
    /// Triggers a transitions to the level this button represents
    /// </summary>
    public void OnClick()
    {
        m_button.interactable = false;
        GameManager.instance.TransitionToLevel(m_level);
    }
}
