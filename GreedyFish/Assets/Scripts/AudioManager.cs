using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip[] clips;

    [Header("Pool Settings")]
    [SerializeField] private int poolSize = 12;

    [Header("Volume")]
    [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;

    [Header("Music")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.4f;
    [SerializeField] private bool playMusicOnStart = true;

    private AudioSource musicSource;
    private readonly List<AudioSource> pool = new List<AudioSource>();
    private readonly Dictionary<string, AudioClip> clipLookup = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildLookup();
        BuildPool();
        BuildMusicSource();
    }

    private void Start()
    {
        if (playMusicOnStart && musicClip != null)
            PlayMusic(musicClip.name);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Plays a named clip from the pool at the listener's position.</summary>
    public void Play(string clipName)
    {
        AudioSource source = GetFreeSource();
        if (source == null || !TryGetClip(clipName, out AudioClip clip))
            return;

        source.transform.position = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        source.spatialBlend = 0f;
        source.clip = clip;
        source.volume = masterVolume;
        source.Play();
    }

    /// <summary>Plays a named clip spatially at a world position.</summary>
    public void PlayAtPoint(string clipName, Vector3 position)
    {
        AudioSource source = GetFreeSource();
        if (source == null || !TryGetClip(clipName, out AudioClip clip))
            return;

        source.transform.position = position;
        source.spatialBlend = 1f;
        source.clip = clip;
        source.volume = masterVolume;
        source.Play();
    }

    /// <summary>Sets the master volume applied to all pooled sources. Range 0–1.</summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }

    /// <summary>Plays a named clip as looping background music. Replaces any current track.</summary>
    public void PlayMusic(string clipName)
    {
        if (musicSource == null || !TryGetClip(clipName, out AudioClip clip))
            return;

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    /// <summary>Stops the background music.</summary>
    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    /// <summary>Sets the music volume independently from SFX. Range 0–1.</summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void BuildLookup()
    {
        clipLookup.Clear();
        if (clips == null)
            return;

        foreach (AudioClip clip in clips)
        {
            if (clip == null)
                continue;

            if (!clipLookup.ContainsKey(clip.name))
                clipLookup.Add(clip.name, clip);
        }
    }

    private void BuildPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var child = new GameObject($"AudioSource_{i}");
            child.transform.SetParent(transform);
            var source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            pool.Add(source);
        }
    }

    private void BuildMusicSource()
    {
        var child = new GameObject("MusicSource");
        child.transform.SetParent(transform);
        musicSource = child.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;
    }

    private AudioSource GetFreeSource()
    {
        foreach (AudioSource source in pool)
        {
            if (!source.isPlaying)
                return source;
        }

        // All sources busy — reuse the oldest (first in list)
        Debug.LogWarning("[AudioManager] Pool exhausted — reusing oldest source.");
        return pool[0];
    }

    private bool TryGetClip(string clipName, out AudioClip clip)
    {
        if (clipLookup.TryGetValue(clipName, out clip))
            return true;

        Debug.LogWarning($"[AudioManager] Clip not found: '{clipName}'");
        return false;
    }
}
