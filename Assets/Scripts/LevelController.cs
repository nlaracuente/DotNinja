using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    /// <summary>
    /// Triggers the GameManager to load the level
    /// </summary>
    void Start()
    {
        GameManager.instance.LoadLevel();
    }
}
