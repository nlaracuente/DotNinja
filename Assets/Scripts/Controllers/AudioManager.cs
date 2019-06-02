using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles playing sound and music
/// Music plays from the AudioManager as it is persistent
/// Sounds use the PlaySoundAt function
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// A reference to self
    /// </summary>
    public static AudioManager instance;

    /// <summary>
    /// A reference to the audio source for playing music
    /// </summary>
    AudioSource m_audioSource;

    /// <summary>
    /// Master music volume
    /// </summary>
    [SerializeField, Range(0f, 1f)]
    float m_musicVolume = .5f;

    /// <summary>
    /// Master Fxs volume
    /// </summary>
    [SerializeField, Range(0f, 1f)]
    float m_fxsVolume = .5f;

    /// <summary>
    /// Where to play the sound clips by default
    /// This is captured each time since it changes when the level changes
    /// </summary>
    Transform DefaultAudioSourceTransform { get { return Camera.main.transform; } }

    // The following AudioClips represents all the music/sounds effects that can be played
    [SerializeField]
    AudioClip m_musicClip;

    [SerializeField]
    AudioClip m_doorClip;

    [SerializeField]
    AudioClip m_connectClip;

    [SerializeField]
    AudioClip m_releaseClip;

    [SerializeField]
    AudioClip m_keyClip;

    [SerializeField]
    AudioClip m_hitClip;

    [SerializeField]
    AudioClip m_startMovingClip;

    [SerializeField]
    AudioClip m_levelCompletedClip;

    /// <summary>
    /// Creates the singleton instance
    /// </summary>
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets up the music player and starts the music
    /// </summary>
    private void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
        m_audioSource.loop = true;
        m_audioSource.clip = m_musicClip;
        m_audioSource.volume = m_musicVolume;

        // Ensures the music always plays from the top
        m_audioSource.Stop();
        m_audioSource.Play();
    }

    /// <summary>
    /// Plays the door opening fx
    /// Returns the lenght of the sound clip
    /// </summary>
    /// <returns></returns>
    public float PlayDoorSound(Transform source = null)
    {
        PlaySoundAt(m_doorClip, source);
        return m_doorClip.length;
    }

    /// <summary>
    /// Plays the hook connected to a connector fx
    /// Returns the lenght of the sound clip
    /// </summary>
    /// <returns></returns>
    public float PlayConnectSound(Transform source = null)
    {
        PlaySoundAt(m_connectClip, source);
        return m_connectClip.length;
    }

    /// <summary>
    /// Plays the hook removed from a connection fx
    /// Returns the lenght of the sound clip
    /// </summary>
    /// <returns></returns>
    public float PlayReleaseSound(Transform source = null)
    {
        PlaySoundAt(m_releaseClip, source);
        return m_releaseClip.length;
    }

    /// <summary>
    /// Plays the key collected fx
    /// Returns the lenght of the sound clip
    /// </summary>
    /// <returns></returns>
    public float PlayKeySound(Transform source = null)
    {
        PlaySoundAt(m_keyClip, source);
        return m_keyClip.length;
    }

    /// <summary>
    /// Plays the player hitting an obstacle fx
    /// Returns the lenght of the sound clip
    /// </summary>
    /// <returns></returns>
    public float PlayHitSound(Transform source = null)
    {
        PlaySoundAt(m_hitClip, source);
        return m_hitClip.length;
    }

    /// <summary>
    /// Plays the moving to the first connector fx
    /// Returns the lenght of the sound clip
    /// </summary>
    /// <returns></returns>
    public float PlayStartMovingSound(Transform source = null)
    {
        PlaySoundAt(m_startMovingClip, source);
        return m_startMovingClip.length;
    }

    /// <summary>
    /// Plays the level was completed fx
    /// Returns the lenght of the sound clip
    /// </summary>
    /// <returns></returns>
    public float PlayLevelCompletedSound(Transform source = null)
    {
        PlaySoundAt(m_levelCompletedClip, source);
        return m_levelCompletedClip.length;
    }

    /// <summary>
    /// Plays the given sound clip at the given location
    /// The volume functions as a multiplier for the master <see cref="m_fxsVolume"/> control
    /// Where 1 is loudest and 0 is no sound
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="source"></param>
    /// <param name="volume"></param>
    void PlaySoundAt(AudioClip clip, Transform source, float volume = 1f)
    {
        if (source == null) {
            source = DefaultAudioSourceTransform;
        }

        volume = Mathf.Clamp01(volume * m_fxsVolume);
        AudioSource.PlayClipAtPoint(clip, source.position, volume);
    }
}
