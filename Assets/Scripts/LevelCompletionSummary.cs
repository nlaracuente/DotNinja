using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the display of level completion summary
/// This lets the player current level, total moves, and rewards them with a [perfect]
/// if the level is completed within a certain set of moves
/// </summary>
public class LevelCompletionSummary : MonoBehaviour
{
    /// <summary>
    /// The summary panel image that shows the star
    /// </summary>
    [SerializeField]
    Image m_summaryImage;

    /// <summary>
    /// The sprite that represents a "perfect" score
    /// </summary>
    [SerializeField]
    Sprite m_perfectSprite;

    /// <summary>
    /// The current level number text container
    /// </summary>
    [SerializeField]
    Text m_levelNumber;

    /// <summary>
    /// The total moves text container
    /// </summary>
    [SerializeField]
    Text m_totalMoves;

    /// <summary>
    /// Updates the level completion summary to display the player's results
    /// </summary>
    /// <param name="level"></param>
    /// <param name="totalMoves"></param>
    /// <param name="maxMoves"></param>
    public void DisplayResults(int level, int totalMoves, int maxMoves)
    {
        // Update text
        if(m_levelNumber != null) {
            m_levelNumber.text = level.ToString();
        }

        if (m_totalMoves != null) {
            m_totalMoves.text = totalMoves.ToString();
        }

        // Update visuals
        if(totalMoves <= maxMoves && m_perfectSprite != null && m_summaryImage != null) {
            m_summaryImage.sprite = m_perfectSprite;
        }
    }
}
