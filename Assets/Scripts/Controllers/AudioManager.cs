using UnityEngine;

/// <summary>
/// Handles playing sound and music
/// Music plays from the AudioManager as it is persistent
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
    AudioSource m_musicAudioSource;

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
            if (m_musicAudioSource == null) {
                InitializeMusicSource();
            }

            m_musicVolume = Mathf.Clamp01(value);
            m_musicAudioSource.volume = m_musicVolume;
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

    /// <summary>
    /// Game music
    /// </summary>
    [SerializeField]
    AudioClip m_musicClip;

    /// <summary>
    /// Door opening
    /// </summary>
    [SerializeField]
    AudioClip m_doorClip;

    /// <summary>
    /// Hook connected
    /// </summary>
    [SerializeField]
    AudioClip m_connectClip;

    /// <summary>
    /// Player initiated moving sequence
    /// </summary>
    [SerializeField]
    AudioClip m_releaseClip;

    /// <summary>
    /// On Mouse hover
    /// </summary>
    [SerializeField]
    AudioClip m_hoverClip;

    /// <summary>
    /// Key collected
    /// </summary>
    [SerializeField]
    AudioClip m_keyClip;

    /// <summary>
    /// Player hit obstacle
    /// </summary>
    [SerializeField]
    AudioClip m_hitClip;

    /// <summary>
    /// A collection of clips to play when moving from connections to connections
    /// to add variations
    /// </summary>
    [SerializeField]
    AudioClip[] m_movingClips;

    /// <summary>
    /// Player's laugh on level complete
    /// </summary>
    [SerializeField]
    AudioClip m_playerLevelCompletionLaughClip;

    /// <summary>
    /// Connector retracted into the wall
    /// </summary>
    [SerializeField]
    AudioClip m_connectorRetractedClip;

    /// <summary>
    /// A retracted connector reset
    /// </summary>
    [SerializeField]
    AudioClip m_connectorResetClip;

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
    /// Initializes the AudioManager's master volumes,
    /// Audio source, random number generator, and starts the music
    /// </summary>
    public void Initialize(float musicVolume, float fxVolume)
    {
        m_musicVolume = musicVolume;
        m_fxsVolume = fxVolume;
        InitializeMusicSource();
        m_random = new Random();
    }

    /// <summary>
    /// Initializes the audio source that controls the music
    /// </summary>
    void InitializeMusicSource()
    {
        m_musicAudioSource = GetComponent<AudioSource>();
        m_musicAudioSource.loop = true;
        m_musicAudioSource.clip = m_musicClip;
        m_musicAudioSource.volume = m_musicVolume;

        // Ensures the music always plays from the top
        m_musicAudioSource.Stop();
        m_musicAudioSource.Play();
    }

    /// <summary>
    /// Plays the door opening fx
    /// Returns the length of the sound clip
    /// </summary>
    /// <returns></returns>
    public float PlayDoorSound()
    {
        PlaySoundAt(m_doorClip);
        return m_doorClip.length;
    }

    /// <summary>
    /// Plays the hook connected to a connector fx
    /// Returns the length of the sound clip
    /// </summary>
    /// <returns></returns>
    public float PlayConnectSound()
    {
        PlaySoundAt(m_connectClip);
        return m_connectClip.length;
    }

    /// <summary>
    /// Plays the hook removed from a connection fx
    /// Returns the length of the sound clip
    /// </summary>
    /// <returns></returns>
    public float PlayReleaseSound()
    {
        PlaySoundAt(m_releaseClip);
        return m_releaseClip.length;
    }

    /// <summary>
    /// Plays the hovering effect
    /// Returns the length of the sound clip
    /// </summary>
    /// <returns></returns>
    public float PlayHoverSound()
    {
        PlaySoundAt(m_hoverClip);
        return m_hoverClip.length;
    }

    /// <summary>
    /// Plays the key collected fx
    /// Returns the length of the sound clip
    /// </summary>
    /// <returns></returns>
    public float PlayKeySound()
    {
        PlaySoundAt(m_keyClip);
        return m_keyClip.length;
    }

    /// <summary>
    /// Plays the player hitting an obstacle fx
    /// Returns the length of the sound clip
    /// </summary>
    /// <returns></returns>
    public float PlayHitSound()
    {
        PlaySoundAt(m_hitClip);
        return m_hitClip.length;
    }

    /// <summary>
    /// Plays the moving to the first connector fx
    /// Returns the length of the sound clip
    /// </summary>
    /// <returns></returns>
    public float PlayStartMovingSound()
    {
        int index = Random.Range(0, m_movingClips.Length);
        AudioClip clip = m_movingClips[index];
        PlaySoundAt(clip);
        return clip.length;
    }

    /// <summary>
    /// Plays the level was completed fx
    /// Returns the length of the sound clip
    /// </summary>
    /// <returns></returns>
    public float PlayLevelCompletedSound()
    {
        PlaySoundAt(m_playerLevelCompletionLaughClip);
        return m_playerLevelCompletionLaughClip.length;
    }

    /// <summary>
    /// Plays the sound for when a connector retracts
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public float PlayConnectorRetracted()
    {
        PlaySoundAt(m_connectorRetractedClip);
        return m_connectorRetractedClip.length;
    }

    /// <summary>
    /// Plays the sound for when a connector resets
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public float PlayConnectorReset()
    {
        PlaySoundAt(m_connectorResetClip);
        return m_connectorResetClip.length;
    }

    /// <summary>
    /// Was originally using AudioSource.PlayClipAtPoint but this makes the sound 3D and 
    /// we were having sound level issues so we are using <see cref="SingleShot2DAudio"/> instead
    /// However, this means we need to spawn a new object for each sound.
    /// We could argue using object pooling here but that's for a later enhancement
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="volume"></param>
    void PlaySoundAt(AudioClip clip, float volume = 1f)
    {
        if(clip != null) {
            volume = Mathf.Clamp01(volume * m_fxsVolume);
            SingleShot2DAudio audio = Instantiate(m_audioPrefab).GetComponent<SingleShot2DAudio>();
            audio.PlaySound(clip, volume);
        }
    }
}
