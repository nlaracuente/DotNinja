using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConnectorType
{
    Normal,
    Breakable,
}

/// <summary>
/// A connector is an anchor point that the player can connect to in order to towards it
/// </summary>
public class Connector : MonoBehaviour
{
    public delegate void ConnectorClickedEvent(Connector connector);
    public event ConnectorClickedEvent OnSelectedEvent;
    public event ConnectorClickedEvent OnMouseOverEvent;
    public event ConnectorClickedEvent OnMouseExitEvent;

    /// <summary>
    /// The type of connector this is
    /// </summary>
    [SerializeField]
    ConnectorType m_connectorType;

    /// <summary>
    /// Seconds before the tile breaks
    /// </summary>
    [SerializeField]
    float m_timeToBreak = 2f;

    /// <summary>
    /// How long after it is broken before it respawns
    /// </summary>
    [SerializeField]
    float m_timeToRespawn = 3f;

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
    /// Uses by breakable connectors to indicate if they are broken or not
    /// </summary>
    bool m_broken = false;

    /// <summary>
    /// True while the player is connected to this connector
    /// </summary>
    bool m_playerConnected = false;

    /// <summary>
    /// A reference to the routine for breaking
    /// </summary>
    IEnumerator m_breakRoutine;

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

        // Trigger break?
        if (!m_broken && m_connectorType == ConnectorType.Breakable)
        {
            if(m_breakRoutine == null)
            {
                m_breakRoutine = BreakRoutine();
                StartCoroutine(m_breakRoutine);
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
    /// Handles the connector breaking routine
    /// </summary>
    /// <returns></returns>
    IEnumerator BreakRoutine()
    {
        // Wait to break
        yield return new WaitForSeconds(m_timeToBreak);

        // Break
        m_broken = true;
        Sprite currentSprite = m_renderer.sprite;
        m_renderer.sprite = null;

        if (m_playerConnected)
        {
            GameManager.instance.TriggerPlayerDeath();
        }

        // Respawn
        yield return new WaitForSeconds(m_timeToRespawn);
        m_broken = false;
        m_renderer.sprite = currentSprite;
    }


    /// <summary>
    /// Dispatches on mouse over event
    /// If the select or deselect buttons are pressed 
    /// then it sends their respective events
    /// </summary>
    private void OnMouseOver()
    {
        // Connector is broken, ignore it
        if (m_broken)
        {
            return;
        }

        OnMouseOverEvent?.Invoke(this);

        if (Input.GetButtonDown("Select"))
        {
            OnSelectedEvent?.Invoke(this);
        }
    }

    /// <summary>
    /// Dispatches on mouse exit event
    /// </summary>
    void OnMouseExit()
    {
        // Connector is broken, ignore it
        if (m_broken)
        {
            return;
        }

        OnMouseExitEvent?.Invoke(this);
    }
}
