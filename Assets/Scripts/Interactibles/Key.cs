using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    /// <summary>
    /// How long to linger after being collected before being destroyed
    /// </summary>
    [SerializeField]
    float m_pickupDelay = .75f;

    /// <summary>
    /// A reference to the player object
    /// </summary>
    Player m_player;

    /// <summary>
    /// True when the player collides with the key
    /// </summary>
    bool m_isCollected = false;

    /// <summary>
    /// Set references
    /// </summary>
    void Start() 
    {
        m_player = FindObjectOfType<Player>();
    }

    /// <summary>
    /// On player trigger destroys itself
    /// Notifies player of key picked up
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!m_isCollected && collision.CompareTag("Player"))
        {
            if (!m_player)
            {
                Debug.LogWarning("WARNING! Key is missing reference to the player");
                return;
            } 

            m_player.HasKey = true;
            m_isCollected = true;
            AudioManager.instance.PlayKeySound(transform);
            Destroy(gameObject, m_pickupDelay);
        }
    }
}
