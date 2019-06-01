using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// A public static instance of self
    /// </summary>
    public static GameManager instance;

    /// <summary>
    /// Sets up instance
    /// </summary>
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        } else
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// Terminates the application
    /// Todo: Remove when done debugging
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)){
            Application.Quit();
        }
    }

    /// <summary>
    /// Handles the level completed sequence
    /// </summary>
    public void LevelCompleted()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
