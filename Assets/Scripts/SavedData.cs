
/// <summary>
/// The model for the data saved for a single level
/// </summary>
[System.Serializable]
public struct LevelProgress 
{
    /// <summary>
    /// Level has been compeleted
    /// </summary>
    public bool IsCompleted;

    /// <summary>
    /// Level ranking is perfect
    /// </summary>
    public bool IsPerfect;
}

/// <summary>
/// Contains all the values to save
/// </summary>
[System.Serializable]
public class SavedData
{
    /// <summary>
    /// The current music volume 
    /// </summary>
    public float MusicVolume { get; set; }

    /// <summary>
    /// The current effects volume
    /// </summary>
    public float FxVolume { get; set; }

    /// <summary>
    /// A collection of state of each level
    /// </summary>
    public LevelProgress[] Progress { get; set; }

    /// <summary>
    /// Defaults the save data to the given values
    /// </summary>
    /// <param name="musicVol"></param>
    /// <param name="fxVol"></param>
    /// <param name="contentSize"></param>
    public void SetDefaults(float musicVol, float fxVol, int contentSize)
    {
        MusicVolume = musicVol;
        FxVolume = fxVol;
        Progress = new LevelProgress[contentSize];
    }
}