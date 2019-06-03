using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
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
    /// Subscribes to all connectors
    /// </summary>
    void Start()
    {
        m_initialPosition = transform.position;
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_pathRenderer = FindObjectOfType<PathRenderer>();
        if(m_pathRenderer == null)
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

        ResetConnections(transform.position != m_initialPosition);
    }

    /// <summary>
    /// Clears all active connections and updates the line renderer
    /// </summary>
    private void ResetConnections(bool isTethered = false)
    {
        // Notify if the player is tethered or not
        m_pathRenderer.ResetConnections(isTethered);
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
        
        while (m_pathRenderer.Connectors.Count > 0)
        {
            Connector connector = m_pathRenderer.Connectors[0];
            Vector2 destination = connector.Anchor.position;
            bool skipDelay = false;

            // Already there then don't play the sound
            // This is a side effect of not removing the last connector to keep the sprite that shows it is connected
            if (transform.position != connector.Anchor.position) {
                skipDelay = true;
                AudioManager.instance.PlayStartMovingSound(transform);
            }

            while (Vector2.Distance(transform.position, destination) > .001f)
            {
                Vector3 position = Vector2.MoveTowards(transform.position, destination, m_moveSpeed * Time.deltaTime);

                // Keep the player's current Z position
                position.z = transform.position.z;
                m_rigidbody.MovePosition(position);

                m_pathRenderer.UpdatePlayerPositionInLineRenderer();
                yield return new WaitForFixedUpdate();
            }

            transform.position = destination;

            // Detache rope
            // Unless this is the last on the list
            if (m_pathRenderer.Connectors.Count > 1) {
                connector.Disconnected();
            }

            // If this is a door and we have the key then stop all further connections
            // to trigger the door animation and end of level
            Door door = connector.GetComponentInParent<Door>();
            if(HasKey && door != null)
            {
                ResetConnections();
                GameManager.instance.LevelCompleted(door);

            // Re-draw connections to reflect the removed one
            } else {

                // Last connection we want to not remove and leave it as a "target"
                // So that the icon remains as "targeted"
                if(m_pathRenderer.Connectors.Count == 1) {
                    connector.ConnectorTargeted();
                    break;
                } else {
                    m_pathRenderer.Connectors.RemoveAt(0);
                    m_pathRenderer.DrawConnections();

                    if (!skipDelay) {
                        yield return new WaitForSeconds(m_connectionDealy);
                    }
                }
            }
        }

        IsMoving = false;
    }

    /// <summary>
    /// Stops movement, resets connections, and triggers player death
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsPlayerDead && collision.collider.CompareTag("MovingObstacle"))
        {
            TriggerDeath();
        }
        
    }

    /// <summary>
    /// Triggers the deaths of the player
    /// </summary>
    public void TriggerDeath()
    {
        IsPlayerDead = true;
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

        // Disable collisions while falling
        m_rigidbody.simulated = false;

        AudioManager.instance.PlayHitSound(transform);

        float totalFallTime = Time.time + m_fallTime;
        while (Time.time < totalFallTime)
        {
            yield return null;

            // Shrink scaling to simulate falling
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * m_fallSpeed);
        }

        // Reset position and scale
        transform.position = m_initialPosition;
        transform.localScale = Vector3.one;

        // Reset Variables
        IsMoving = false;
        IsPlayerDead = false;

        // Re-enable collisions
        m_rigidbody.simulated = true;
    }
}
