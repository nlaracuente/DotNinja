using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Handles custom mouse click events
/// This is to allow us to use the UI.OnClick() events in the editor with an 
/// </summary>
public class MouseClickEvents : MonoBehaviour, IPointerDownHandler
{
    /// <summary>
    /// The event to dispatch on click
    /// </summary>
    public UnityEvent OnClickEvent;

    /// <summary>
    /// Dispatch the onclick event
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        OnClickEvent?.Invoke();
    }
}
