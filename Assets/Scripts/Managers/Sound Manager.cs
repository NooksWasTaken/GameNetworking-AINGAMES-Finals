using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    WALK, RUN, JUMP, GRAB, DROP,
    TRASH_ITEM, SWAP, VACUUM, LITTERPICKER, BGM, BGM2
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] soundList;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;    // Short SFX
    [SerializeField] private AudioSource loopSource;   // Dedicated looping sound (can assign in inspector)
    [SerializeField] private AudioSource musicSource;  // Optional background music

    private Dictionary<SoundType, AudioClip> clipDict;
    private Dictionary<SoundType, AudioSource> loopSources = new Dictionary<SoundType, AudioSource>();

    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float loopVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

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

    public static void PlayLoopingSound(SoundType sound, float volume)
    {
        if (Instance == null || !Instance.clipDict.ContainsKey(sound)) return;

        AudioSource src;

        if (Instance.loopSource.clip == null)
        {
            src = Instance.loopSource;
        }
        else if (!Instance.loopSources.ContainsKey(sound))
        {
            src = Instance.gameObject.AddComponent<AudioSource>();
            src.loop = true;
            Instance.loopSources[sound] = src;
        }
        else
        {
            src = Instance.loopSources[sound];
        }

        src.clip = Instance.clipDict[sound];
        src.volume = Mathf.Clamp01(volume) * Instance.loopVolume;
        if (!src.isPlaying) src.Play();
    }


    public static void StopLoopingSound(SoundType sound)
    {
        if (Instance == null) return;

        if (Instance.loopSources.ContainsKey(sound))
        {
            AudioSource src = Instance.loopSources[sound];
            if (src.isPlaying) src.Stop();
        }
        else if (Instance.loopSource.clip != null && Instance.loopSource.clip == Instance.clipDict[sound])
        {
            Instance.loopSource.Stop();
        }
    }

    public void PlayMusic(SoundType sound)
    {
        if (!clipDict.ContainsKey(sound)) return;
        musicSource.clip = clipDict[sound];
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying) musicSource.Stop();
    }

    public void SetSFXVolume(float volume) { sfxSource.volume = Mathf.Clamp01(volume); }

    public void SetLoopVolume(float volume)
    {
        loopVolume = Mathf.Clamp01(volume);
        loopSource.volume = loopVolume;
        foreach (var src in loopSources.Values)
            src.volume = loopVolume;
    }

    public void SetMusicVolume(float volume) { musicSource.volume = Mathf.Clamp01(volume); }
}
