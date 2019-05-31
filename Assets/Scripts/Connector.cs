using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A connector is an anchor point that the player can connect to in order to towards it
/// </summary>
public class Connector : MonoBehaviour
{
    public delegate void ConnectorClickedEvent(Connector connector);
    public event ConnectorClickedEvent OnClicked;

    /// <summary>
    /// Dispatches connector clicked event
    /// </summary>
    private void OnMouseDown()
    {
        OnClicked?.Invoke(this);
    }
}
