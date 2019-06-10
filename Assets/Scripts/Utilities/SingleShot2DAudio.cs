using UnityEngine;

/// <summary>
/// Emulates AudioSource.PlayClipAtPoint for 2D sounds
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SingleShot2DAudio : MonoBehaviour
{
    /// <summary>
    /// A reference to the audio source
    /// </summary>
    AudioSource m_source;
    AudioSource Source
    {
        get {
            if (m_source == null) {
                m_source = GetComponent<AudioSource>();
            }

            return m_source;
        }
    }

    /// <summary>
    /// True while the audio source is playing
    /// </summary>
    public bool IsPlaying { get { return Source.isPlaying; } }

    /// <summary>
    /// True once the sound has been triggered to play
    /// </summary>
    bool m_soundPlayed = false;

    /// <summary>
    /// Ensures the audio is destroyed even when Time.timeScale is 0
    /// We could have used clip.length to destroy later but that is affected by Time.timeScale
    /// </summary>
    private void Update()
    {
        if (m_soundPlayed && !Source.isPlaying) {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Plays the given sound as a 2D sound
    /// Sets this object to destroy itself when completed
    /// This happens in the Update()
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="volume"></param>
    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip == null) {
            Destroy(gameObject);

        } else {
            m_soundPlayed = true;

            // Ensure reasonable sound range
            volume = Mathf.Clamp01(volume);

            gameObject.name = clip.name + "_AudioSource";

            Source.volume = volume;
            Source.clip = clip;
            Source.loop = false;
            Source.spatialBlend = 0; // makes it 2D
            Source.Play();
        }
    }    
}
