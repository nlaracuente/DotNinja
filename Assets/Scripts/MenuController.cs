using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Functions as middle man between the UI buttons and the GameManager
/// </summary>
public class MenuController : MonoBehaviour
{
    public void StartGame()
    {
        GameManager.instance.StartGame();
    }

    public void MainMenu()
    {
        GameManager.instance.MainMenu();
    }
}
