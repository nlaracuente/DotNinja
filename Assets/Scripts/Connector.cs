using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A connector is an anchor point that the player can connect to in order to towards it
/// </summary>
public class Connector : MonoBehaviour
{
    public delegate void ConnectorClickedEvent(Connector connector);
    public event ConnectorClickedEvent OnSelected;
    public event ConnectorClickedEvent OnDeselected;

    [SerializeField]
    Transform m_anchor;

    /// <summary>
    /// The transform for the player to anchor to
    /// </summary>
    public Transform Anchor
    {
        get {
            // Default to self
            if (m_anchor == null)
            {
                m_anchor = transform;
            }

            return m_anchor;
        }
    }

    /// <summary>
    /// Dispatches connector clicked event
    /// </summary>
    private void OnMouseOver()
    {
        if (Input.GetButtonDown("Select"))
        {
            OnSelected?.Invoke(this);
        }
        else if (Input.GetButtonDown("Deselect"))
        {
            OnDeselected?.Invoke(this);
        }
        
    }
}
