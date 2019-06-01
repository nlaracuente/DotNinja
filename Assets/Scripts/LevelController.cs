using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{


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
