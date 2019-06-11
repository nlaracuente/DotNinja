using System.Collections;
using UnityEngine;

/// <summary>
/// The player controlled avatar
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
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
    /// How quickly to fall
    /// </summary>
    [SerializeField]
    float m_fallSpeed = 5f;

    /// <summary>
    /// How long to fall before respawning the player
    /// </summary>
    [SerializeField]
    float m_fallTime = 2f;

    /// <summary>
    /// How many degrees to rotate while falling
    /// </summary>
    [SerializeField]
    float m_fallSpinRate = 45f;

    /// <summary>
    /// A reference to the sprite renderer
    /// </summary>
    SpriteRenderer m_renderer;

    /// <summary>
    /// A refenece to the path renderer object
    /// </summary>
    PathRenderer m_pathRenderer;

    /// <summary>
    /// True while the player is moving along the connection
    /// </summary>
    public bool IsMoving { get; private set; } = false;

    /// <summary>
    /// True when the key has been collected
    /// </summary>
    public bool HasKey { get; set; } = false;

    /// <summary>
    /// True when the player reaches the door and has the key
    /// </summary>
    public bool IsPlayerDead { get; private set; } = false;

    /// <summary>
    /// A reference to the rigidbody
    /// </summary>
    Rigidbody2D m_rigidbody;

    /// <summary>
    /// Holds the initial position of the player
    /// Used to respawn the player at this position
    /// </summary>
    Vector3 m_initialPosition;

    /// <summary>
    /// The player moves towards this when exiting the level
    /// Using a transform so that I can animate the transform
    /// and give the player the illusion it is walking
    /// </summary>
    [SerializeField]
    Transform m_exitTarget;

    /// <summary>
    /// Speed at which the player exits
    /// </summary>
    [SerializeField]
    float m_exitSpeed = 5f;

    /// <summary>
    /// How long to take while exiting
    /// </summary>
    [SerializeField]
    float m_exitTime = 2f;

    /// <summary>
    /// Layers to set the sprite renderer when falling
    /// </summary>
    [SerializeField]
    int m_fallingLayer;

    /// <summary>
    /// Default sorting layer
    /// </summary>
    int m_defaultLayer;

    /// <summary>
    /// The connector the player is current at
    /// Used for rendering the roap preview, path, and updating the connector's graphic
    /// </summary>
    public Connector CurrentConnector { get; set; } = null;

    /// <summary>
    /// Subscribes to all connectors
    /// </summary>
    void Start()
    {
        m_initialPosition = transform.position;
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_renderer = GetComponent<SpriteRenderer>();
        m_pathRenderer = FindObjectOfType<PathRenderer>();
        m_defaultLayer = m_renderer.sortingLayerID;
        m_fallingLayer = SortingLayer.NameToID("PlayerFalling");
        if (m_pathRenderer == null)
        {
            Debug.LogWarning("WARNING! Player is missing reference to path renderer");
        }
    }

    /// <summary>
    /// True when one of the following conditions is met
    /// </summary>
    /// <returns></returns>
    private bool PreventAction()
    {
        return IsPlayerDead && !GameManager.instance.IsLevelLoaded || IsMoving || GameManager.instance.IsLevelCompleted;
    }

    /// <summary>
    /// Removes all active connections
    /// </summary>
    void OnMouseDown()
    {
        // Ignore when moving
        if (PreventAction())
        {
            return;
        }

        ResetConnections();
    }

    /// <summary>
    /// Clears all active connections and updates the line renderer
    /// </summary>
    private void ResetConnections(Door door = null)
    {
        // Notify if the player is tethered or not
        m_pathRenderer.ResetConnections(null);
    }

    /// <summary>
    /// Triggers the movement routine
    /// </summary>
    public void Move()
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
        m_pathRenderer.ResetCursor();
        AudioManager.instance.PlayReleaseSound();

        while (m_pathRenderer.Connectors.Count > 0) {
            Connector connector = m_pathRenderer.Connectors[0];
            Vector2 destination = connector.Anchor.position;
            bool skipDelay = true;

            // Need to move to the connector
            if (connector != CurrentConnector) {
                skipDelay = false;
                AudioManager.instance.PlayStartMovingSound();
                GameManager.instance.TotalMoves++;
            }

            LookAtConnector(connector);

            // Since the player is moving
            // we want to disconnect the current connector
            if (CurrentConnector) {
                CurrentConnector.Disconnected();
                CurrentConnector = null;
            }

            while (Vector2.Distance(transform.position, destination) > .001f) {
                Vector3 position = Vector2.MoveTowards(transform.position, destination, m_moveSpeed * Time.deltaTime);

                // Keep the player's current Z position
                position.z = transform.position.z;
                m_rigidbody.MovePosition(position);

                m_pathRenderer.UpdatePlayerPositionInLineRenderer();
                yield return new WaitForFixedUpdate();

                // Moving to a connector that has retracted
                // Trigger death which cancels this routine
                if (connector.IsRetracted) {
                    TriggerDeath();
                    yield return new WaitForEndOfFrame();
                }
            }

            ResetPlayerRotation();
            transform.position = destination;

            // Now that the player has landed we can trigger this routine
            connector.TriggerRetractRoutine();

            // If we are at the door with all keys 
            // Then trigger level completed routine
            Door door = connector.GetComponentInParent<Door>();
            if (door != null && AllKeysCollected()) {
                CurrentConnector = null;
                ResetConnections(door);
                GameManager.instance.LevelCompleted(door);
                break;
            }

            // Save the last connector as the current
            // before any disconnects so that we don't see
            // the incorrect sprite change
            if (m_pathRenderer.Connectors.Count == 1) {
                CurrentConnector = connector;
            }

            // Removes the connector from the list of connections
            DisconnectConnector(connector);

            if (!skipDelay) {
                yield return new WaitForSeconds(m_connectionDealy);
            }            
        }

        IsMoving = false;
    }

    /// <summary>
    /// Removes the given connection from the list
    /// </summary>
    /// <param name="connector"></param>
    void DisconnectConnector(Connector connector)
    {
        m_pathRenderer.RemoveConnector(connector);
    }

    /// <summary>
    /// Rotates the player based on their currrent position
    /// This is sort of a hack but it works
    /// </summary>
    void ResetPlayerRotation()
    {
        // Is on the bottom row or at the door
        if (transform.position.y <= 1) {
            transform.rotation = Quaternion.identity;

        // Is on the top row
        } else {
            transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }
    }

    /// <summary>
    /// Rotates the player to make it look at the connector
    /// </summary>
    /// <param name="connector"></param>
    void LookAtConnector(Connector connector)
    {
        var dir = connector.transform.position - transform.position;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    /// <summary>
    /// Stops movement, resets connections, and triggers player death
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsPlayerDead && collision.collider.CompareTag("MovingObstacle"))
        {
            AudioManager.instance.PlayHitSound();
            TriggerDeath();
        }
        
    }

    /// <summary>
    /// Triggers the deaths of the player
    /// </summary>
    public void TriggerDeath()
    {
        IsPlayerDead = true;

        // Ensures the current connector's sprite resets to disconnected
        if (CurrentConnector) {
            CurrentConnector.Disconnected();
        }

        CurrentConnector = null;

        StopAllCoroutines();
        ResetConnections();
        StartCoroutine(RespawnRoutine());
    }

    /// <summary>
    /// Handles the player falling and respawning routine
    /// </summary>
    /// <returns></returns>
    IEnumerator RespawnRoutine()
    {
        Vector3 targetScale = Vector3.one * 0.01f;

        m_pathRenderer.ResetCursor();

        // Update the layer so that the player looks like it is falling
        m_renderer.sortingLayerID = m_fallingLayer;

        // Disable collisions while falling
        m_rigidbody.simulated = false;

        AudioManager.instance.PlayReleaseSound();

        float totalFallTime = Time.time + m_fallTime;
        while (Time.time < totalFallTime)
        {
            yield return null;

            // Shrink scaling to simulate falling
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * m_fallSpeed);
        }

        RemoveCollectedKeys();

        // Reset player
        m_renderer.sortingLayerID = m_defaultLayer;
        transform.position = m_initialPosition;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // Reset Variables
        IsMoving = false;
        IsPlayerDead = false;

        // Re-enable collisions
        m_rigidbody.simulated = true;
    }

    /// <summary>
    /// Makes the player move to the right while he laughs/exits the level
    /// </summary>
    /// <returns></returns>
    public IEnumerator LevelCompletedAnimationRoutine()
    {
        float targetTime = Time.time + m_exitTime;

        // Mak the player move
        while (Time.time < targetTime) {
            Vector3 targetPosition = Vector3.MoveTowards(transform.position,
                                                         m_exitTarget.position, 
                                                         m_exitSpeed * Time.deltaTime);

            // Keep the player's current Z position
            targetPosition.z = transform.position.z;
            m_rigidbody.MovePosition(targetPosition);
            yield return null;
        }
    }

    /// <summary>
    /// Triggers all keys to be marked as not collected
    /// </summary>
    void RemoveCollectedKeys()
    {
        // Reset moves since the player fell
        GameManager.instance.TotalMoves = 0;

        // Marks all keys as not collected
        foreach (Key key in FindObjectsOfType<Key>()) {
            key.IsCollected = false;
        }
    }

    /// <summary>
    /// True when all the keys have been marked as collected
    /// </summary>
    /// <returns></returns>
    bool AllKeysCollected()
    {
        bool collected = true;

        // Marks all keys as not collected
        foreach (Key key in FindObjectsOfType<Key>()) {
            if (!key.IsCollected) {
                collected = false;
                break;
            }
        }

        return collected;
    }
}
