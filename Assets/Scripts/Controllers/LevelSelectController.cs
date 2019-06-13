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

        /**
         * Encountered a weird bug for mobile only that causes the level selection build
         * to no longer happen. Not sure what causes this bug but trying this method instead.
         * Essentially, I added the levels manually and here we loop to set them up
         * 
         * NOTE: this does not check that all the levels actually exist and it is merely a hack
         *       to make mobile work 
         */
        //if (m_contentXform.childCount > 0) {
        //    for (int i = 0; i < m_contentXform.childCount; i++) {
        //        // +1 because there is no level zero
        //        int levelNum = i + 1;
        //        LevelProgress level = allLevelProgress[levelNum];
        //        LevelSelectionButton button = m_contentXform.GetChild(i).GetComponent<LevelSelectionButton>();
        //        button.Setup(levelNum, level.IsUnlocked, level.IsPerfect);
        //    }

        //// Otherwise, keep the old way of auto-building
        //} else {
            for (int i = 1; i < allLevelProgress.Length; i++) {
                LevelProgress level = allLevelProgress[i];
                LevelSelectionButton button = Instantiate(m_buttonPrefab, m_contentXform);
                button.Setup(i, level.IsUnlocked, level.IsPerfect);
            }
        //}        
    }
}
