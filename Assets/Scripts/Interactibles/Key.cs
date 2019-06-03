using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Key : MonoBehaviour
{
    /// <summary>
    /// How long to linger after being collected before being destroyed
    /// </summary>
    [SerializeField]
    float m_pickupDelay = .75f;

    /// <summary>
    /// Minimum size the key can shrink to
    /// </summary>
    [SerializeField]
    float m_minSize = 0.75f;

    /// <summary>
    /// Maximum size the key can grow to
    /// </summary>
    [SerializeField]
    float m_maxSize = 1f;

    /// <summary>
    /// How fast the key grows/shrinks
    /// </summary>
    [SerializeField]
    float m_growSpeed = 1f;

    /// <summary>
    /// A reference to the player object
    /// </summary>
    Player m_player;

    /// <summary>
    /// A reference to the sprite renderer component
    /// </summary>
    SpriteRenderer m_renderer;

    /// <summary>
    /// 
    /// </summary>
    Sprite m_originalSprite;

    /// <summary>
    /// True when the player collides with the key
    /// </summary>
    public bool IsCollected { get; set; } = false;

    /// <summary>
    /// Set references
    /// </summary>
    void Start() 
    {
        m_player = FindObjectOfType<Player>();
        m_renderer = GetComponent<SpriteRenderer>();
        m_originalSprite = m_renderer.sprite;
    }

    /// <summary>
    /// Ensures the key is hidden when collected and not hidden when not collected
    /// </summary>
    void Update()
    {
        // Show it
        if(!IsCollected && m_renderer.sprite == null) {
            m_renderer.sprite = m_originalSprite;
        }

        // Hide it
        if (IsCollected && m_renderer.sprite != null) {
            m_renderer.sprite = null;
        }

        // Make it grow/shrink
        float range = m_maxSize - m_minSize;
        float scale = (float)((Mathf.Sin(Time.time * m_growSpeed) + 1.0) / 2.0 * range + m_minSize);
        transform.localScale = Vector2.one * scale;
    }


    /// <summary>
    /// On player trigger destroys itself
    /// Notifies player of key picked up
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!IsCollected && collision.CompareTag("Player"))
        {
            if (!m_player)
            {
                Debug.LogWarning("WARNING! Key is missing reference to the player");
                return;
            } 

            IsCollected = true;
            AudioManager.instance.PlayKeySound(transform);
        }
    }
}
