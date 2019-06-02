using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Handles the displaying of the active path and preview path
/// </summary>
public class PathRenderer : MonoBehaviour
{
    /// <summary>
    /// Line renderer that shows potential connections
    /// </summary>
    [SerializeField]
    LineRenderer m_previewPathRenderer;

    /// <summary>
    /// Line renderer that shows the active connections
    /// </summary>
    [SerializeField]
    LineRenderer m_activePathRenderer;

    /// <summary>
    /// Material to use on the line renderer when a connection has not been made
    [SerializeField]
    Material m_pathPendingMaterial;

    /// <summary>
    /// Material to use on the line renderer when the path is available
    /// </summary>
    [SerializeField]
    Material m_invalidPathMaterial;

    /// <summary>
    /// Material to use on the line renderer when the path is not available
    /// </summary>
    [SerializeField]
    Material m_validPathMaterial;

    /// <summary>
    /// Where to place the Z of the line renderer's position to avoid being infront of the player
    /// </summary>
    [SerializeField]
    float m_lineZPosition = 2f;

    /// <summary>
    /// The layer mask for obstacles that prevent connections from being made
    /// </summary>
    [SerializeField]
    LayerMask m_obstacleMask;

    /// <summary>
    /// A container for all active connectors
    /// </summary>
    public List<Connector> Connectors { get; private set; } = new List<Connector>();

    /// <summary>
    /// True while the mouse is on a connector
    /// </summary>
    bool m_mouseOnConnector = false;

    /// <summary>
    /// A reference to the player component
    /// </summary>
    Player m_player;
    Vector3 PlayerPosition
    {
        get {
            Vector3 position = transform.position;

            if (m_player == null)
            {
                Debug.LogErrorFormat("ERROR! PathRender is missing player reference");
            } else
            {
                position = m_player.transform.position;
            }

            return position;
        }
    }

    /// <summary>
    /// Sets refrences, resets the line renderers, and subscribes to connectors
    /// </summary>
    private void Start()
    {
        m_player = FindObjectOfType<Player>();
        Connectors = new List<Connector>();

        ResetRenderers();
        SubscribeToConnectors();
    }

    /// <summary>
    /// Shows paths preview
    /// </summary>
    private void Update()
    {
        PreviewPath();
    }

    /// <summary>
    /// Ensures the line renderers start with zero positions
    /// </summary>
    void ResetRenderers()
    {
        ResetConnections();

        // This should only need to be called once for now
        if (m_previewPathRenderer)
        {
            m_previewPathRenderer.positionCount = 0;
        }
    }

    /// <summary>
    /// Clears active connections
    /// Removes all positions from the active path renderer
    /// </summary>
    public void ResetConnections()
    {
        Connectors.Clear();

        if (m_activePathRenderer)
        {
            m_activePathRenderer.positionCount = 0;
        }
    }

    /// <summary>
    /// Subscribes to all available connector's on select and deselect eventss
    /// </summary>
    void SubscribeToConnectors()
    {
        foreach (Connector connector in FindObjectsOfType<Connector>())
        {
            connector.OnSelectedEvent += OnConnectorSelected;
            connector.OnDeselectedEvent += OnConnectorDeselected;
            connector.OnMouseOverEvent += OnMouseEnterConnector;
            connector.OnMouseExitEvent += OnMouseExitConnector;
        }
    }

    /// <summary>
    /// Display a potential connection
    /// </summary>
    void PreviewPath()
    {
        if (PreventAction())
        {
            m_previewPathRenderer.positionCount = 0;
            return;
        }
        
        Connector lastConnetor = Connectors.LastOrDefault();

        // To validate the connection is good we need to line cast
        // from either the player's current position or the last connector on the list
        // to the connector passed in
        Vector2 start = lastConnetor ? lastConnetor.transform.position : PlayerPosition;
        Vector2 end = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Set the positions
        m_previewPathRenderer.positionCount = 2;
        m_previewPathRenderer.SetPositions( new Vector3[] {start, end} );

        // Update colors depending of what is available
        // Default to pending connection
        m_previewPathRenderer.material = m_pathPendingMaterial;
        if (m_mouseOnConnector)
        {
            if (IsConnectionPossible(start, end))
            {
                m_previewPathRenderer.material = m_validPathMaterial;
            } else
            {
                m_previewPathRenderer.material = m_invalidPathMaterial;
            }
        }
    }

    /// <summary>
    /// Registers the connector as a new connection when it does not exist
    /// When the connector exist it removes everything after it
    /// </summary>
    /// <param name="connector"></param>
    public void OnConnectorSelected(Connector connector)
    {
        if (PreventAction())
        {
            return;
        }

        // Last Connector
        Connector lastConnetor = Connectors.LastOrDefault();

        // We are only allowing a single connection per connector
        // and this connector is already on the list
        if (GameManager.instance.SingleConnections && Connectors.Contains(connector))
        {
            // Piggy back on the conditions that prevents adding new connectors
            lastConnetor = connector;
        }

        // Not already on the last or not the last one on the list
        // Then we can add or re-add it
        if (lastConnetor == null || lastConnetor != connector)
        {
            // To validate the connection is good we need to line cast
            // from either the player's current position or the last connector on the list
            // to the connector passed in
            Vector2 start = lastConnetor ? lastConnetor.transform.position : PlayerPosition;
            Vector2 end = connector.transform.position;

            if (IsConnectionPossible(start, end))
            {
                Connectors.Add(connector);
            }
        }

        DrawConnections();
    }

    /// <summary>
    /// Removes the given connector from the list of connections and anything after it
    /// </summary>
    /// <param name="connector"></param>
    public void OnConnectorDeselected(Connector connector)
    {
        if (PreventAction())
        {
            return;
        }

        int index = Connectors.LastIndexOf(connector) + 1;
        int count = Connectors.Count - index;

        // Avoid attempting to remove beyond the last item
        Connectors.RemoveRange(index, count);

        DrawConnections();
    }

    /// <summary>
    /// Updates <see cref="m_mouseOnConnector"/> to true
    /// </summary>
    /// <param name="connector"></param>
    public void OnMouseEnterConnector(Connector connector)
    {
        if (PreventAction())
        {
            return;
        }

        // Default to true
        m_mouseOnConnector = true;

        // If we are only allowing only single connections
        // then we don't want to recognize this action if the connector is already in the list
        if (GameManager.instance.SingleConnections && Connectors.Contains(connector))
        {
            m_mouseOnConnector = false;
        }
    }

    /// <summary>
    /// Updates <see cref="m_mouseOnConnector"/> to false
    /// </summary>
    /// <param name="connector"></param>
    public void OnMouseExitConnector(Connector connector)
    {
        if (PreventAction())
        {
            return;
        }

        m_mouseOnConnector = false;
    }

    /// <summary>
    /// True when the player is not in a state of availability
    /// </summary>
    /// <returns></returns>
    bool PreventAction()
    {
        return !GameManager.instance.IsLevelLoaded || m_player.IsMoving || m_player.IsPlayerDead || GameManager.instance.IsLevelCompleted;
    }

    /// <summary>
    /// Returns true of there are no obstacles in the way of the ray
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    bool IsConnectionPossible(Vector2 start, Vector2 end)
    {
        var hit = Physics2D.Linecast(start, end, m_obstacleMask);
        return hit.collider == null;
    }

    /// <summary>
    /// Resets line renderer's position count to match current connection count
    /// Adds first the player's current position and the connectors' position
    /// Updates the line renderer to draw all new positions
    /// </summary>
    public void DrawConnections()
    {
        if (m_activePathRenderer == null)
        {
            Debug.LogWarning("WARNING! Player is missing a reference to the line renderer");
            return;
        }

        // Reset positions to current count
        m_activePathRenderer.positionCount = Connectors.Count + 1;

        // + 1 to account for the player's position
        Vector3[] positions = new Vector3[m_activePathRenderer.positionCount];

        // First position is always the player's
        positions[0] = PlayerPosition;
        for (int i = 1; i <= Connectors.Count; i++)
        {
            Connector connector = Connectors[i - 1];
            Vector3 position = new Vector3(connector.transform.position.x, connector.transform.position.y, m_lineZPosition);

            // Place the Z away from the player
            positions[i] = position;
        }

        m_activePathRenderer.SetPositions(positions);
    }

    /// <summary>
    /// Updates the position in the line renderer that represent the player's current position
    /// </summary>
    public void UpdatePlayerPositionInLineRenderer()
    {
        if (m_activePathRenderer == null)
        {
            Debug.LogWarning("WARNING! Player is missing a reference to the line renderer");
            return;
        }

        m_activePathRenderer.SetPosition(0, PlayerPosition);
    }
}
