using UnityEngine;

/// <summary>
/// Handles the creation of the level selection menu
/// </summary>
public class LevelSelectController : MonoBehaviour
{
    /// <summary>
    /// The level selection prefab
    /// </summary>
    [SerializeField]
    LevelSelectionButton m_buttonPrefab;

    /// <summary>
    /// A reference to where the buttons will be parented
    /// </summary>
    [SerializeField]
    Transform m_contentXform;

    /// <summary>
    /// Creates all the level selection buttons 
    /// Initializes them to match the level progress passed
    /// </summary>
    /// <param name="allLevelProgress"></param>
    public void LoadLevelSelection(LevelProgress[] allLevelProgress)
    {
        for (int i = 1; i < allLevelProgress.Length; i++) {
            LevelProgress progress = allLevelProgress[i];
            LevelSelectionButton button = Instantiate(m_buttonPrefab, m_contentXform);
            button.Initialize(i, true, progress.IsPerfect);
        }
    }
}
