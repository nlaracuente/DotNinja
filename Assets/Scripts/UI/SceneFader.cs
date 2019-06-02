using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles fading in/out
/// </summary>
public class SceneFader : MonoBehaviour
{
    /// <summary>
    /// The UI image that sits on top of the entire scene to hide/reveal it
    /// </summary>
    [SerializeField]
    Image m_faderImage;

    /// <summary>
    /// The color to use when fading the scene
    /// </summary>
    [SerializeField]
    Color m_fadeColor = Color.black;

    /// <summary>
    /// Default time to use when fading
    /// </summary>
    [SerializeField]
    float m_defaultFadeTime = 2f;

    /// <summary>
    /// Default wait time before starting to fade
    /// </summary>
    [SerializeField]
    float m_defaultFadeStartDelay = .25f;

    /// <summary>
    /// Default wait time after the fade is done before ending the routine
    /// </summary>
    [SerializeField]
    float m_defaultFadeEndDelay = .25f;

    /// <summary>
    /// For debugging as there is a bug where the screen fader never finishes
    /// though it looks like it finishes
    /// </summary>
    bool m_isFading = false;

   /// <summary>
   /// Handles the fading routine
   /// </summary>
   /// <param name="start"></param>
   /// <param name="end"></param>
   /// <returns></returns>
    public IEnumerator FadeRoutine(float start, float end, float speed = 1f)
    {
        m_isFading = true;

        Color startingColor = new Color(m_fadeColor.r, m_fadeColor.g, m_fadeColor.b, start);
        m_faderImage.color = startingColor;

        // Wait before we start fading to show the start alpha
        yield return new WaitForSeconds(m_defaultFadeStartDelay);

        float increment = (end - start / speed) * Time.deltaTime;
        float current = m_faderImage.color.a;

        while (!Mathf.Approximately(current, end))
        {
            yield return new WaitForEndOfFrame();
            current = Mathf.Clamp01(current + increment);
            Color newColor = new Color(m_fadeColor.r, m_fadeColor.g, m_fadeColor.b, current);
            m_faderImage.color = newColor;
        }

        // Make sure the fader is set to the desired alpha
        Color finalColor = new Color(m_fadeColor.r, m_fadeColor.g, m_fadeColor.b, end);
        m_faderImage.color = finalColor;

        yield return new WaitForSeconds(m_defaultFadeEndDelay);
        m_isFading = false;
    }
}
