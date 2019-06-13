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
    /// Loads the level selection
    /// </summary>
    void Start()
    {
        LoadLevelSelection(GameManager.instance.AllLevelProgress);
    }

    /// <summary>
    /// Creates all the level selection buttons 
    /// Initializes them to match the level progress passed
    /// </summary>
    /// <param name="allLevelProgress"></param>
    public void LoadLevelSelection(LevelProgress[] allLevelProgress)
    {
        if(allLevelProgress == null) {
            Debug.LogError("Level Profress is null");
            return;
        }

        for (int i = 1; i < allLevelProgress.Length; i++) {
            LevelProgress level = allLevelProgress[i];
            LevelSelectionButton button = Instantiate(m_buttonPrefab, m_contentXform);
            button.Setup(i, level.IsUnlocked, level.IsPerfect);
        }
    }
}
