﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

        ResetConnections(transform.position != m_initialPosition);
    }

    /// <summary>
    /// Clears all active connections and updates the line renderer
    /// </summary>
    private void ResetConnections(bool isTethered = false, Door door = null)
    {
        // Notify if the player is tethered or not
        m_pathRenderer.ResetConnections(isTethered, null);
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
        AudioManager.instance.PlayReleaseSound(transform);

        while (m_pathRenderer.Connectors.Count > 0)
        {
            Connector connector = m_pathRenderer.Connectors[0];
            Vector2 destination = connector.Anchor.position;
            bool skipDelay = true;

            // When we are already there then don't play the sound
            // This is a side effect of not removing the last connector to keep the sprite that shows it is connected
            if (transform.position != connector.Anchor.position) {
                skipDelay = false;
                AudioManager.instance.PlayStartMovingSound(transform);
            }

            // Make player look at the direction it is going to
            var dir = connector.transform.position - transform.position;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            while (Vector2.Distance(transform.position, destination) > .001f)
            {
                Vector3 position = Vector2.MoveTowards(transform.position, destination, m_moveSpeed * Time.deltaTime);

                // Keep the player's current Z position
                position.z = transform.position.z;
                m_rigidbody.MovePosition(position);

                m_pathRenderer.UpdatePlayerPositionInLineRenderer();
                yield return new WaitForFixedUpdate();

                // If moving to a retractable one and it retracts
                // Then we want to trigger a fall
                // Wait for the death to be registered
                if (connector.IsRetracted) {
                    TriggerDeath();
                    yield return new WaitForEndOfFrame();
                }
            }

            // HACKS!
            // Reset rotation

            // Landed on the bottom row or the door
            if (transform.position.y <= 1 ) {
                transform.rotation = Quaternion.identity;

            // Landed on the top row
            } else {
                transform.rotation = Quaternion.Euler(0f, 0f, 180f);
            }
            
            transform.position = destination;

            // Now that the player has landed we can trigger this routine
            connector.TriggerRetractRoutine();

            // If this is a door and we have the key then stop all further connections
            // to trigger the door animation and end of level
            Door door = connector.GetComponentInParent<Door>();

            // Reached the door with the key
            if (door != null && AllKeysCollected()) {
                ResetConnections(false, door);
                GameManager.instance.LevelCompleted(door);
                break;
            }

            //// The last connection will only be removed when it is the door
            //// This is so that the connector remains as the target 
            //if (m_pathRenderer.Connectors.Count == 1 && door == null) {
            //    connector.ConnectorTargeted();
            //    break;
            //}

            // Disconnect connector
            connector.Disconnected();
            m_pathRenderer.Connectors.Remove(connector);
            m_pathRenderer.DrawConnections();

            if (!skipDelay) {
                yield return new WaitForSeconds(m_connectionDealy);
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
            AudioManager.instance.PlayHitSound(transform);
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

        m_pathRenderer.ResetCursor();

        // Update the layer so that the player looks like it is falling
        m_renderer.sortingLayerID = m_fallingLayer;

        // Disable collisions while falling
        m_rigidbody.simulated = false;

        AudioManager.instance.PlayReleaseSound(transform);

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
