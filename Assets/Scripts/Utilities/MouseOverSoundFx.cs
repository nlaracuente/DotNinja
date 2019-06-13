using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Triggers the hover sound effect when the mouse enters this object
/// </summary>
public class MouseOverSoundFx : MonoBehaviour, IPointerEnterHandler
{
    /// <summary>
    /// For UI buttons
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlaySoundFx();
    }

    /// <summary>
    /// Triggers the AudioManager to play hover sound effects
    /// </summary>
    void PlaySoundFx()
    {
        AudioManager.instance.PlayHoverSound();
    }
}
