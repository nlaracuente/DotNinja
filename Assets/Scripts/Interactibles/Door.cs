using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    /// <summary>
    /// A reference to the renderer component
    /// </summary>
    [SerializeField]
    SpriteRenderer m_renderer;

    /// <summary>
    /// The sprite to use when the door is opened
    /// </summary>
    [SerializeField]
    Sprite m_doorOpenedSprite;

    /// <summary>
    /// How long to wait while playing the "open" animation
    /// </summary>
    [SerializeField]
    float m_openDelay = 0.25f;

    /// <summary>
    /// Set references
    /// </summary>
    void Start()
    {
        if(m_doorOpenedSprite == null)
        {
            Debug.LogWarning("WARNING! Door is missing sprite for when it is openeded");
        }

        if(m_renderer == null)
        {
            m_renderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    /// <summary>
    /// Triggers the opening of the door (changing of sprite)
    /// </summary>
    /// <returns></returns>
    public IEnumerator OpenRoutine()
    {
        // Wait to visually register player reached the door
        yield return new WaitForSeconds(m_openDelay);

        float length = AudioManager.instance.PlayDoorSound(transform);
        if (m_renderer != null && m_doorOpenedSprite != null)
        {
            m_renderer.sprite = m_doorOpenedSprite;
        }

        // Wait for the door sounds to finish
        yield return new WaitForSeconds(length);
    }
}
