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
    /// How much to offest the cursor graphic by
    /// </summary>
    [SerializeField]
    Vector2 m_cursorOffset;

    /// <summary>
    /// Default cursor to use on mouse exit
    /// </summary>
    [SerializeField]
    Texture2D m_defaultCursor;

    /// <summary>
    /// Cursor for when a connector can be selected
    /// </summary>
    [SerializeField]
    Texture2D m_selectConnectionCursor;

    /// <summary>
    /// Cursor for when a connector can be removed
    /// </summary>
    [SerializeField]
    Texture2D m_removeConnectionCursor;

    /// <summary>
    /// Cursor for when the connector is the target (last one)
    /// </summary>
    [SerializeField]
    Texture2D m_goToConnectionCursor;

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
    /// Returns the player's current connector
    /// </summary>
    Connector PlayerConnector { get { return m_player.CurrentConnector; } }

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
    /// We will not render on Mobile
    /// </summary>
    private void Update()
    {
        if(Application.platform != RuntimePlatform.Android) {
            PreviewPath();
        }
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
    public void ResetConnections(Door door = null)
    {
        // There are no connections
        if (Connectors.Count == 0) {
            return;
        }

        // Disconnect first to update sprites
        for (int i = 0; i < Connectors.Count; i++) {
            Connectors[i].Disconnected();
        }

        Connectors.Clear();

        if (m_activePathRenderer) {
            m_activePathRenderer.positionCount = 0;
        }

        ResetCursor();
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
            connector.OnMouseEnterEvent += OnMouseEnterConnector;
            connector.OnMouseOverEvent += OnMouseOverConnector;
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

        float distance = Vector3.Distance(start, end);
        m_previewPathRenderer.material.mainTextureScale = new Vector2(distance * 2, 1f);
    }

    /// <summary>
    /// Registers the connector as a new connection when it does not exist
    /// When the connector exist it removes everything after it
    /// </summary>
    /// <param name="connector"></param>
    public void OnConnectorSelected(Connector connector)
    {
        if (PreventAction()) {
            return;
        }

        // Last Connector
        Connector lastConnetor = Connectors.LastOrDefault();

        // A new connection trying to be added
        if (!Connectors.Contains(connector)) {

            // To validate the connection is good we need to line cast
            // from either the player's current position or the last connector on the list
            // to the connector passed in
            Vector2 start = lastConnetor ? lastConnetor.transform.position : PlayerPosition;
            Vector2 end = connector.transform.position;

            if (IsConnectionPossible(start, end)) {
                AudioManager.instance.PlayConnectSound();
                Connectors.Add(connector);
            }

            DrawConnections();

        // An existing connection the player wants to reset their connections to
        } else if (lastConnetor != connector) {
            OnConnectorDeselected(connector);

        // Last connector therefore player wants to move to it
        } else {
            m_player.Move();
        }
    }

    /// <summary>
    /// Removes the given connector from the list of connections and anything after it
    /// </summary>
    /// <param name="connector"></param>
    void OnConnectorDeselected(Connector connector)
    {
        // Already removed
        if (!Connectors.Contains(connector)) {
            connector.Disconnected();
            return;
        }

        int index = Connectors.LastIndexOf(connector) + 1;
        int count = Connectors.Count - index;

        if (count > 0) {
            AudioManager.instance.PlayConnectSound();
        }

        // Get a list of all the connectors about to be removed 
        // so that we can update their sprites
        List<Connector> disconnected = Connectors.GetRange(index, count);
        disconnected.ForEach(d => d.Disconnected());

        // Avoid attempting to remove beyond the last item
        if (count >= 0) {
            Connectors.RemoveRange(index, count);
        }

        DrawConnections();
    }

    /// <summary>
    /// Updates <see cref="m_mouseOnConnector"/> to true
    /// Plays hover sound
    /// </summary>
    /// <param name="connector"></param>
    public void OnMouseEnterConnector(Connector connector)
    {
        if (PreventAction())
        {
            return;
        }

        AudioManager.instance.PlayHoverSound();
        UpdateCursorOnConnector(connector);
        m_mouseOnConnector = true;
    }

    /// <summary>
    /// Updates <see cref="m_mouseOnConnector"/> to true
    /// And the cursor
    /// </summary>
    /// <param name="connector"></param>
    public void OnMouseOverConnector(Connector connector)
    {
        if (PreventAction()) {
            return;
        }

        UpdateCursorOnConnector(connector);
        m_mouseOnConnector = true;
    }

    /// <summary>
    /// Changes the cursor over the connector to indicate what action is allowed
    /// </summary>
    /// <param name="connector"></param>
    void UpdateCursorOnConnector(Connector connector)
    {
        // Connector can be added
        if (!Connectors.Contains(connector)) {
            Cursor.SetCursor(m_selectConnectionCursor, m_cursorOffset, CursorMode.Auto);

        // Connector already on the list
        } else {
            Connector lastConnector = Connectors.LastOrDefault();

            // Can move to this connector
            if (lastConnector == connector) {
                Cursor.SetCursor(m_goToConnectionCursor, m_cursorOffset, CursorMode.Auto);

            // Can reset connections to this connector
            } else {
                Cursor.SetCursor(m_removeConnectionCursor, m_cursorOffset, CursorMode.Auto);
            }
        }
    }

    /// <summary>
    /// Sets the default cursor texture
    /// </summary>
    public void ResetCursor()
    {
        Cursor.SetCursor(m_defaultCursor, m_cursorOffset, CursorMode.Auto);
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
        Cursor.SetCursor(m_defaultCursor, m_cursorOffset, CursorMode.Auto);
    }

    /// <summary>
    /// True when the player is not in a state of availability
    /// </summary>
    /// <returns></returns>
    bool PreventAction()
    {
        return !GameManager.instance.IsLevelLoaded || 
               m_player.IsMoving ||
               m_player.IsPlayerDead || 
               GameManager.instance.IsLevelCompleted ||
               GameManager.instance.IsGamePaused;
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

        // We want to know the last connector to keep it as the target
        Connector lastConnector = Connectors.LastOrDefault();

        // Reset positions to current count
        m_activePathRenderer.positionCount = Connectors.Count + 1;

        // + 1 to account for the player's position
        Vector3[] positions = new Vector3[m_activePathRenderer.positionCount];

        // First position is always the player's
        // or their connector when we have one
        positions[0] = PlayerConnector ? PlayerConnector.transform.position : PlayerPosition;

        for (int i = 1; i <= Connectors.Count; i++) {
            Connector connector = Connectors[i - 1];
            Vector3 position = new Vector3(connector.transform.position.x, connector.transform.position.y, m_lineZPosition);

            // Place the Z away from the player
            positions[i] = position;

            // Connector is set as target if it's the last one
            if (connector == lastConnector) {
                lastConnector.ConnectorTargeted();
            } else {
                connector.ConnectorTethered();
            }
        }

        // Update the player's connector accordingly
        if (PlayerConnector) {

            // Is the target
            if (Connectors.Count == 0) {
                PlayerConnector.ConnectorTargeted();

            // Is tethered
            } else if (PlayerConnector != lastConnector) {
                PlayerConnector.ConnectorTethered();
            }
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

    /// <summary>
    /// Removes the given connector marking it as disconnected
    /// Except if the connector is the one the player is currently at
    /// then it is marked as targeted
    /// </summary>
    /// <param name="connector"></param>
    public void RemoveConnector(Connector connector)
    {
        if (m_player.CurrentConnector == connector) {
            connector.ConnectorTargeted();
        } else {
            connector.Disconnected();
        }

        Connectors.Remove(connector);
        DrawConnections();
    }
}
