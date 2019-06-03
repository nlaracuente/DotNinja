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
    /// The prefab that enables us to play 2D sounds
    /// </summary>
    [SerializeField]
    SingleShot2DAudio m_audioPrefab;

    /// <summary>
    /// Holds the currently playing effect demo
    /// </summary>
    SingleShot2DAudio m_demoAudio;

    /// <summary>
    /// Master music volume
    /// </summary>
    [SerializeField, Range(0f, 1f)]
    float m_musicVolume = .5f;

    /// <summary>
    /// Updates the current music volume
    /// </summary>
    public float MusicVolume
    {
        get { return m_musicVolume; }
        set {
            m_musicVolume = Mathf.Clamp01(value);
            m_audioSource.volume = m_musicVolume;
        }
    }

    /// <summary>
    /// Master Fxs volume
    /// </summary>
    [SerializeField, Range(0f, 1f)]
    float m_fxsVolume = .5f;

    /// <summary>
    /// Sets the effects volume
    /// </summary>
    public float FxVolume
    {
        get { return m_fxsVolume; }
        set {
            m_fxsVolume = Mathf.Clamp01(value);

            // Avoid playing too many at one time
            if(m_fxVolumeChangeDemoClip != null  && (m_demoAudio == null || !m_demoAudio.IsPlaying)) {
                m_demoAudio = Instantiate(m_audioPrefab).GetComponent<SingleShot2DAudio>();
                m_demoAudio.PlaySound(m_fxVolumeChangeDemoClip, m_fxsVolume);
            }            
        }
    }

    /// <summary>
    /// The audio clip to play to show how the volume has changed
    /// </summary>
    [SerializeField]
    AudioClip m_fxVolumeChangeDemoClip;

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
    AudioClip[] m_movingClips;

    [SerializeField]
    AudioClip m_levelCompletedClip;

    [SerializeField]
    AudioClip m_connectorRetracted;

    [SerializeField]
    AudioClip m_connectorReset;

    /// <summary>
    /// Random generator
    /// </summary>
    Random m_random;

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

        m_random = new Random();
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
        int index = Random.Range(0, m_movingClips.Length);
        AudioClip clip = m_movingClips[index];
        PlaySoundAt(clip, source);
        return clip.length;
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
    /// Plays the sound for when a connector retracts
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public float PlayConnectorRetracted(Transform source = null)
    {
        PlaySoundAt(m_connectorRetracted, source);
        return m_connectorRetracted.length;
    }

    /// <summary>
    /// Plays the sound for when a connector resets
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public float PlayConnectorReset(Transform source = null)
    {
        PlaySoundAt(m_connectorReset, source);
        return m_connectorReset.length;
    }

    /// <summary>
    /// Was originally using AudioSource.PlayClipAtPoint but this makes the sound 3D and 
    /// we were having sound level issues so we are using <see cref="SingleShot2DAudio"/> instead
    /// However, this means we need to spawn a new object for each sound.
    /// We could argue using object pooling here but that's for a later enhancement
    /// 
    /// This was change late during the jam hence why the signature is not changed
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="source"></param>
    /// <param name="volume"></param>
    void PlaySoundAt(AudioClip clip, Transform source, float volume = 1f)
    {
        if (source == null) {
            source = DefaultAudioSourceTransform;
        }

        if(clip != null) {
            volume = Mathf.Clamp01(volume * m_fxsVolume);
            SingleShot2DAudio audio = Instantiate(m_audioPrefab).GetComponent<SingleShot2DAudio>();
            audio.PlaySound(clip, volume);
        }
    }
}
