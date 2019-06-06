using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlls Unique level information and triggers the level to load
/// </summary>
public class LevelController : MonoBehaviour
{
    [SerializeField]
    int m_maxMoves;
    /// <summary>
    /// Maximum moves to score a perfect on this level
    /// </summary>
    public int MaxMoves { get { return m_maxMoves; } }

    /// <summary>
    /// Triggers the GameManager to load the level
    /// </summary>
    void Update()
    {
        if (!GameManager.instance.IsLevelLoaded)
        {
            GameManager.instance.LoadLevel();
        }
    }
}
