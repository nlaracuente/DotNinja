using System.Collections;
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
    /// A refenece to the path renderer object
    /// </summary>
    PathRenderer m_pathRenderer;

    /// <summary>
    /// True while the player is moving along the connection
    /// </summary>
    public bool IsMoving { get; private set; }

    /// <summary>
    /// True when the key has been collected
    /// </summary>
    public bool HasKey { get; set; }

    /// <summary>
    /// True when the player reaches the door and has the key
    /// </summary>
    public bool LevelCompleted { get; private set; }

    /// <summary>
    /// Subscribes to all connectors
    /// </summary>
    void Start()
    {
        m_pathRenderer = FindObjectOfType<PathRenderer>();
        if(m_pathRenderer == null)
        {
            Debug.LogWarning("WARNING! Player is missing reference to path renderer");
        }
    }

    /// <summary>
    /// Checks for commitment to movement
    /// </summary>
    private void Update()
    {
        if (IsMoving || LevelCompleted)
        {
            return;
        }

        if (Input.GetButtonDown("Jump"))
        {
            Move();
        }
    }

    /// <summary>
    /// Removes all active connections
    /// </summary>
    void OnMouseDown()
    {
        // Ignore when moving
        if (IsMoving || LevelCompleted)
        {
            return;
        }

        ResetConnections();
    }

    /// <summary>
    /// Clears all active connections and updates the line renderer
    /// </summary>
    private void ResetConnections()
    {
        m_pathRenderer.ResetConnections();
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
        
        while (m_pathRenderer.Connectors.Count > 0)
        {
            Connector connector = m_pathRenderer.Connectors[0];
            Vector2 destination = connector.Anchor.position;

            while (Vector2.Distance(transform.position, destination) > .001f)
            {
                Vector3 position = Vector2.MoveTowards(transform.position, destination, m_moveSpeed * Time.deltaTime);

                // Keep the player's current Z position
                position.z = transform.position.z;
                transform.position = position;

                m_pathRenderer.UpdatePlayerPositionInLineRenderer();
                yield return new WaitForEndOfFrame();
            }

            transform.position = destination;

            // If this is a door and we have the key then stop all further connections
            // to trigger the door animation and end of level
            Door door = connector.GetComponentInParent<Door>();
            if(HasKey && door != null)
            {
                ResetConnections();
                StartCoroutine(OpenDoorRoutine(door));

            // Re-draw connections to reflect the removed one
            } else {
                m_pathRenderer.Connectors.RemoveAt(0);
                m_pathRenderer.DrawConnections();
                yield return new WaitForSeconds(m_connectionDealy);
            }
        }

        IsMoving = false;
    }

    /// <summary>
    /// Waits for the door to open before triggering level completed
    /// </summary>
    /// <param name="door"></param>
    /// <returns></returns>
    IEnumerator OpenDoorRoutine(Door door)
    {
        LevelCompleted = true;
        yield return StartCoroutine(door.OpenRoutine());
        GameManager.instance.LevelCompleted();
    }
}
