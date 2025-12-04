using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    WALK, RUN, JUMP, GRAB, DROP,
    TRASH_ITEM, SWAP, VACUUM,
    LITTERPICKER, BGM
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] soundList;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;   // Short SFX
    [SerializeField] private AudioSource loopSource;  // Looping sounds like WALK
    [SerializeField] private AudioSource musicSource; // Optional background music

    private Dictionary<SoundType, AudioClip> clipDict;

    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    public float loopVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup AudioSources
        if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();
        if (!loopSource)
        {
            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.loop = true;
            loopSource.playOnAwake = false;
        }
        if (!musicSource)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        // Build dictionary for faster lookup
        clipDict = new Dictionary<SoundType, AudioClip>();
        for (int i = 0; i < soundList.Length; i++)
        {
            if (i < System.Enum.GetValues(typeof(SoundType)).Length)
                clipDict[(SoundType)i] = soundList[i];
        }

        sfxSource.volume = sfxVolume;
        loopSource.volume = loopVolume;
        musicSource.volume = musicVolume;
    }

    public static void PlaySound(SoundType sound, float volume = 1f)
    {
        if (Instance == null || !Instance.clipDict.ContainsKey(sound)) return;
        Instance.sfxSource.PlayOneShot(Instance.clipDict[sound], volume * Instance.sfxVolume);
    }

    // --- STATIC LOOPING SOUND ---
    public static void PlayLoopingSound(SoundType sound)
    {
        if (Instance == null || !Instance.clipDict.ContainsKey(sound)) return;

        AudioClip clip = Instance.clipDict[sound];

        if (Instance.loopSource.clip != clip || !Instance.loopSource.isPlaying)
        {
            Instance.loopSource.clip = clip;
            Instance.loopSource.volume = Instance.loopVolume;
            Instance.loopSource.Play();
        }
    }

    public static void StopLoopingSound()
    {
        if (Instance != null && Instance.loopSource.isPlaying)
            Instance.loopSource.Stop();
    }


    // --- MUSIC CONTROL ---
    public void PlayMusic(SoundType sound)
    {
        if (!clipDict.ContainsKey(sound)) return;
        musicSource.clip = clipDict[sound];
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Stop();
    }

    // --- VOLUME CONTROLS ---
    public void SetSFXVolume(float volume) 
    { 
        sfxSource.volume = Mathf.Clamp01(volume); 
    }
    public void SetLoopVolume(float volume) 
    { 
        loopSource.volume = Mathf.Clamp01(volume); 
    }
    public void SetMusicVolume(float volume) 
    { 
        musicSource.volume = Mathf.Clamp01(volume); 
    }
}
