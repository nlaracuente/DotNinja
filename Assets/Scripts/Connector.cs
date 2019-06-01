using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A connector is an anchor point that the player can connect to in order to towards it
/// </summary>
public class Connector : MonoBehaviour
{
    public delegate void ConnectorClickedEvent(Connector connector);
    public event ConnectorClickedEvent OnSelectedEvent;
    public event ConnectorClickedEvent OnDeselectedEvent;

    public event ConnectorClickedEvent OnMouseOverEvent;
    public event ConnectorClickedEvent OnMouseExitEvent;

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
    /// Dispatches on mouse over event
    /// If the select or deselect buttons are pressed 
    /// then it sends their respective events
    /// </summary>
    private void OnMouseOver()
    {
        OnMouseOverEvent?.Invoke(this);

        if (Input.GetButtonDown("Select"))
        {
            OnSelectedEvent?.Invoke(this);
        }
        else if (Input.GetButtonDown("Deselect"))
        {
            OnDeselectedEvent?.Invoke(this);
        }        
    }

    /// <summary>
    /// Dispatches on mouse exit event
    /// </summary>
    void OnMouseExit()
    {
        OnMouseExitEvent?.Invoke(this);
    }
}
