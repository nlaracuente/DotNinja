﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConnectorType
{
    Normal,
    Retractable,
}

/// <summary>
/// A connector is an anchor point that the player can connect to in order to towards it
/// </summary>
public class Connector : MonoBehaviour
{
    public delegate void ConnectorClickedEvent(Connector connector);
    public event ConnectorClickedEvent OnSelectedEvent;
    public event ConnectorClickedEvent OnMouseEnterEvent;
    public event ConnectorClickedEvent OnMouseOverEvent;
    public event ConnectorClickedEvent OnMouseExitEvent;

    /// <summary>
    /// The type of connector this is
    /// </summary>
    [SerializeField]
    ConnectorType m_connectorType;

    /// <summary>
    /// Seconds before the connector retacts
    /// </summary>
    [SerializeField]
    float m_timeToRetract = 1f;

    /// <summary>
    /// How long after it retracts before it resets
    /// </summary>
    [SerializeField]
    float m_timeToReset = 2f;

    /// <summary>
    /// A reference to the sprite renderer
    /// </summary>
    [SerializeField]
    SpriteRenderer m_renderer;

    /// <summary>
    /// Where the player anchors to
    /// </summary>
    [SerializeField]
    Transform m_anchor;

    /// <summary>
    /// Default sprite when not hooked or connected
    /// </summary>
    [SerializeField]
    Sprite m_defaultSprite;

    /// <summary>
    /// Default sprite when not hooked or connected
    /// </summary>
    [SerializeField]
    Sprite m_defaultRetractableSprite;

    /// <summary>
    /// Sprite when connecting with another target
    /// </summary>
    [SerializeField]
    Sprite m_tetheredSprite;

    /// <summary>
    /// Sprite when connecting with another target
    /// </summary>
    [SerializeField]
    Sprite m_tetheredRetractableSprite;

    /// <summary>
    /// Sprite when this is the last connection
    /// </summary>
    [SerializeField]
    Sprite m_targetedSprite;

    /// <summary>
    /// Sprite when this is the last connection
    /// </summary>
    [SerializeField]
    Sprite m_targetedRetractableSprite;

    /// <summary>
    /// Sprite to use when retracted
    /// </summary>
    [SerializeField]
    Sprite m_retractedSprite;

    /// <summary>
    /// Uses by retractable connectors to indicate if they are broken or not
    /// </summary>
    bool m_retracted = false;

    /// <summary>
    /// True while the player is connected to this connector
    /// </summary>
    bool m_playerConnected = false;

    /// <summary>
    /// A reference to the routine for retracting
    /// </summary>
    IEnumerator m_retractRoutine;

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
    /// Set references
    /// </summary>
    private void Start()
    {
        if(m_renderer == null)
        {
            m_renderer = GetComponentInChildren<SpriteRenderer>();
        }

        SetDefaultSprite();
    }

    /// <summary>
    /// Detects collision with player to trigger break routine
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        m_playerConnected = true;

        // Trigger retract?
        if (!m_retracted && m_connectorType == ConnectorType.Retractable)
        {
            if(m_retractRoutine == null)
            {
                m_retractRoutine = RetractRoutine();
                StartCoroutine(m_retractRoutine);
            }
        }
    }

    /// <summary>
    /// Player is no longer connected
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_playerConnected = false;
        }
    }


    /// <summary>
    /// Handles the connector retracting routine
    /// </summary>
    /// <returns></returns>
    IEnumerator RetractRoutine()
    {
        // Wait to retract
        yield return new WaitForSeconds(m_timeToRetract);

        // retract
        AudioManager.instance.PlayConnectorRetracted();
        m_retracted = true;
        SetRetractedSprite();

        if (m_playerConnected)
        {
            GameManager.instance.TriggerPlayerDeath();
        }

        // Respawn
        yield return new WaitForSeconds(m_timeToReset);
        AudioManager.instance.PlayConnectorReset();
        m_retracted = false;
        SetDefaultSprite();
        m_retractRoutine = null;
    }

    /// <summary>
    /// /// Dispatches on mouse over event
    /// If the select or deselect buttons are pressed 
    /// then it sends their respective events
    /// </summary>
    private void OnMouseEnter()
    {
        if (m_retracted) {
            return;
        }

        OnMouseEnterEvent?.Invoke(this);
    }

    /// <summary>
    /// Dispatch event when clickd on
    /// </summary>
    private void OnMouseOver()
    {
        if (m_retracted) {
            return;
        }

        OnMouseOverEvent?.Invoke(this);

        if (Input.GetButtonDown("Select")) {
            OnSelectedEvent?.Invoke(this);
        }
    }

    /// <summary>
    /// Dispatches on mouse exit event
    /// </summary>
    void OnMouseExit()
    {
        // Connector is broken, ignore it
        if (m_retracted)
        {
            return;
        }

        OnMouseExitEvent?.Invoke(this);
    }

    /// <summary>
    /// For debugging purposes to see the change in the editor only
    /// rather than creating an editor script (call it a short cut)
    /// </summary>
    private void OnDrawGizmos()
    {
        SpriteRenderer renderer = m_renderer ?? GetComponentInChildren<SpriteRenderer>();

        // Probaly the door
        if(renderer == null) {
            return;
        }

        if (m_connectorType == ConnectorType.Retractable) {
            renderer.sprite = m_defaultRetractableSprite;
        } else {
            renderer.sprite = m_defaultSprite;
        }
    }

    /// <summary>
    /// Sets the sprite to indicate connector is no longer part of a connection
    /// </summary>
    public void Disconnected()
    {
        SetSprite(m_defaultSprite);
    }

    /// <summary>
    /// Updates the sprite to indicate connector is tethered to another connector
    /// </summary>
    public void ConnectorTethered()
    {
        SetTetheredSprite();
    }

    /// <summary>
    /// Updates the sprite to indicate connector is the last one on the connections
    /// </summary>
    public void ConnectorTargeted()
    {
        SetTargetedSprite();
    }

    /// <summary>
    /// Changes to default sprite
    /// </summary>
    void SetDefaultSprite()
    {
        Sprite sprite = m_connectorType == ConnectorType.Retractable ? m_defaultRetractableSprite : m_defaultSprite;
        SetSprite(sprite);
    }

    /// <summary>
    /// Changes to tethered sprite
    /// </summary>
    void SetTetheredSprite()
    {
        Sprite sprite = m_connectorType == ConnectorType.Retractable ? m_tetheredRetractableSprite : m_tetheredSprite;
        SetSprite(sprite);
    }

    /// <summary>
    /// Changes to tethered sprite
    /// </summary>
    void SetTargetedSprite()
    {
        Sprite sprite = m_connectorType == ConnectorType.Retractable ? m_targetedRetractableSprite : m_targetedSprite;
        SetSprite(sprite);
    }

    /// <summary>
    /// Changes to tethered sprite
    /// </summary>
    void SetRetractedSprite()
    {
        Sprite sprite = m_retractedSprite;
        SetSprite(sprite);
    }

    /// <summary>
    /// Sets the given sprite so long as we can
    /// </summary>
    /// <param name="sprite"></param>
    void SetSprite(Sprite sprite)
    {
        // Ingore during a retraction routine
        if (!m_retracted && m_renderer && sprite) {
            m_renderer.sprite = sprite;
        }   
    }
}
