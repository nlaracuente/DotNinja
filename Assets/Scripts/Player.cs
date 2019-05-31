using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    /// <summary>
    /// How fast the player moves towards the next connector
    /// </summary>
    [SerializeField]
    float m_moveSpeed = 5f;

    /// <summary>
    /// A reference to the line render that shows the connections to the connectors
    /// </summary>
    [SerializeField]
    LineRenderer m_lineRenderer;

    /// <summary>
    /// A of connectors to move towards
    /// </summary>
    List<Connector> m_connectors;    


    /// <summary>
    /// Subscribes to all connectors
    /// </summary>
    void Start()
    {
        m_connectors = new List<Connector>();

        if(m_lineRenderer == null)
        {
            m_lineRenderer = GetComponentInChildren<LineRenderer>();
        }
        
        // To ensure the line renderer is only showing active connections
        // which at this point should be none
        DrawConnections();

        foreach (Connector connector in FindObjectsOfType<Connector>())
        {
            connector.OnClicked += OnConnectorClicked;
        }
    }

    /// <summary>
    /// Registers the connector as a new connection when it does not exist
    /// When the connector exist it removes everything after it
    /// </summary>
    /// <param name="connector"></param>
    public void OnConnectorClicked(Connector connector)
    {
        if (!m_connectors.Contains(connector))
        {
            m_connectors.Add(connector);

        // Remove everything after this connector but keep this one
        }  else
        {
            int index = m_connectors.IndexOf(connector) + 1;
            int count = m_connectors.Count - index;

            // Avoid attempting to remove beyond the last item
            if (index < m_connectors.Count - 1)
            {
                m_connectors.RemoveRange(index, count);
            }
        }

        DrawConnections();
    }

    /// <summary>
    /// Resets line renderer's position count to match current connection count
    /// Adds first the player's current position and the connectors' position
    /// Updates the line renderer to draw all new positions
    /// </summary>
    void DrawConnections()
    {
        if(m_lineRenderer == null)
        {
            Debug.LogWarning("WARNING! Player is missing a reference to the line renderer");
            return;
        }

        // Reset positions to current count
        m_lineRenderer.positionCount = m_connectors.Count + 1;

        // + 1 to account for the player's position
        Vector3[] positions = new Vector3[m_lineRenderer.positionCount];

        // First position is always the player's
        positions[0] = transform.position;
        for (int i = 1; i <= m_connectors.Count; i++)
        {
            Connector connector = m_connectors[i - 1];
            positions[i] = connector.transform.position;
        }

        m_lineRenderer.SetPositions(positions);
    }

    /// <summary>
    /// Removes all active connections
    /// </summary>
    void OnMouseDown()
    {
        m_connectors.Clear();
        DrawConnections();
    }
}
