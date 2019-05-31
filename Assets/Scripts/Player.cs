﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{
    /// <summary>
    /// How fast the player moves towards the next connector
    /// </summary>
    [SerializeField]
    float m_moveSpeed = 5f;

    /// <summary>
    /// How long to linger at each connection before moving to the next
    /// </summary>
    [SerializeField]
    float m_connectionDealy = 0.25f;

    /// <summary>
    /// Where to place the Z of the line renderer's position to avoid being infront of the player
    /// </summary>
    [SerializeField]
    float m_lineZPosition = 2f;

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
    /// True while the player is moving along the connection
    /// </summary>
    public bool IsMoving { get; private set; }

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
            connector.OnSelected += OnConnectorSelected;
            connector.OnDeselected += OnConnectorDeselected;
        }
    }

    private void Update()
    {
        // Ignore when moving
        if (IsMoving)
        {
            return;
        }

        if (Input.GetButtonDown("Jump"))
        {
            Move();
        }
    }

    /// <summary>
    /// Registers the connector as a new connection when it does not exist
    /// When the connector exist it removes everything after it
    /// </summary>
    /// <param name="connector"></param>
    public void OnConnectorSelected(Connector connector)
    {
        // Ignore when moving
        if (IsMoving)
        {
            return;
        }

        // Last Connector
        Connector lastConnetor = m_connectors.LastOrDefault();

        // Not already on the last or not the last one on the list
        // Then we can add or re-add it
        if (lastConnetor == null || lastConnetor != connector)
        {
            m_connectors.Add(connector);
        }

        DrawConnections();
    }

    /// <summary>
    /// Removes the given connector from the list of connections and anything after it
    /// </summary>
    /// <param name="connector"></param>
    public void OnConnectorDeselected(Connector connector)
    {
        // Ignore when moving
        if (IsMoving)
        {
            return;
        }

        int index = m_connectors.LastIndexOf(connector) + 1;
        int count = m_connectors.Count - index;

        // Avoid attempting to remove beyond the last item
        m_connectors.RemoveRange(index, count);

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
            Vector3 position = new Vector3(connector.transform.position.x, connector.transform.position.y, m_lineZPosition);

            // Place the Z away from the player
            positions[i] = position;
        }

        m_lineRenderer.SetPositions(positions);
    }

    /// <summary>
    /// Updates the position in the line renderer that represent the player's current position
    /// </summary>
    void UpdatePlayerPositionInLineRenderer()
    {
        if (m_lineRenderer == null)
        {
            Debug.LogWarning("WARNING! Player is missing a reference to the line renderer");
            return;
        }

        m_lineRenderer.SetPosition(0, transform.position);
    }

    /// <summary>
    /// Removes all active connections
    /// </summary>
    void OnMouseDown()
    {
        // Ignore when moving
        if (IsMoving)
        {
            return;
        }

        m_connectors.Clear();
        DrawConnections();
    }

    /// <summary>
    /// Triggers the movement routine
    /// </summary>
    void Move()
    {
        if (!IsMoving)
        {
            StartCoroutine(MoveRoutine());
        }
    }

    /// <summary>
    /// Moves the player along all connections
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveRoutine()
    {
        IsMoving = true;
        
        while (m_connectors.Count > 0)
        {
            Connector connector = m_connectors[0];
            Vector2 destination = connector.Anchor.position;

            while (Vector2.Distance(transform.position, destination) > .001f)
            {
                Vector3 position = Vector2.MoveTowards(transform.position, destination, m_moveSpeed * Time.deltaTime);

                // Keep the player's current Z position
                position.z = transform.position.z;
                transform.position = position;

                UpdatePlayerPositionInLineRenderer();
                yield return new WaitForEndOfFrame();
            }

            transform.position = destination;
            m_connectors.RemoveAt(0);

            // Re-draw connections to reflect the removed one
            DrawConnections();

            yield return new WaitForSeconds(m_connectionDealy);
        }

        IsMoving = false;
    }
}
