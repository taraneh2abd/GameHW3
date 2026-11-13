using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource[] sfxSources;
    [SerializeField] private AudioClip titleMusic;
    [SerializeField] private float fadeTime = 0.6f;

    private const string MenuSceneName = "MainMenu";

    private float[] sfxScales;
    private int nextSfxSource;
    private float fadeScale;
    private float fadeTarget;
    private bool musicActive;
    private AudioClip pendingClip;

    public AudioClip CurrentMusic { get; private set; }
    public AudioClip LastSfx { get; private set; }
    public int SfxPlayCount { get; private set; }
    public AudioSource MusicSource => musicSource;
    public IReadOnlyList<AudioSource> SfxSources => sfxSources;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false;
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.ignoreListenerPause = true;

        sfxScales = new float[sfxSources.Length];
        for (int i = 0; i < sfxSources.Length; i++)
        {
            sfxSources[i].loop = false;
            sfxSources[i].playOnAwake = false;
            sfxSources[i].ignoreListenerPause = true;
            sfxScales[i] = 1f;
        }

        ApplyVolumes();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject prefab = Resources.Load<GameObject>("AudioManager");
        if (prefab == null)
        {
            Debug.LogError("Resources/AudioManager prefab is missing.");
            return;
        }

        Instantiate(prefab);
    }

    private void OnEnable()
    {
        if (Instance != this)
        {
            return;
        }

        GameSettings.Changed += ApplyVolumes;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        if (Instance != this)
        {
            return;
        }

        GameSettings.Changed -= ApplyVolumes;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (Instance == this)
        {
            PlayMusicForScene(SceneManager.GetActiveScene());
        }
    }

    private void Update()
    {
        float step = Time.unscaledDeltaTime / Mathf.Max(0.01f, fadeTime);
        fadeScale = Mathf.MoveTowards(fadeScale, fadeTarget, step);

        if (musicActive && fadeScale <= 0f)
        {
            if (pendingClip != null)
            {
                StartClip(pendingClip);
                pendingClip = null;
                fadeTarget = 1f;
            }
            else if (fadeTarget <= 0f)
            {
                musicSource.Stop();
                musicActive = false;
            }
        }

        musicSource.volume = GameSettings.MusicVolume * fadeScale;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || clip == CurrentMusic)
        {
            return;
        }

        CurrentMusic = clip;

        if (!musicActive)
        {
            StartClip(clip);
            pendingClip = null;
            fadeScale = 0f;
            fadeTarget = 1f;
        }
        else
        {
            pendingClip = clip;
            fadeTarget = 0f;
        }
    }

    public void StopMusic()
    {
        CurrentMusic = null;
        pendingClip = null;
        fadeTarget = 0f;
    }

    public void PlaySfx(AudioClip clip, float volumeScale = 1f, float pitchVariation = 0f)
    {
        if (clip == null || sfxSources.Length == 0)
        {
            return;
        }

        AudioSource source = sfxSources[nextSfxSource];
        sfxScales[nextSfxSource] = volumeScale;
        nextSfxSource = (nextSfxSource + 1) % sfxSources.Length;

        source.Stop();
        source.clip = clip;
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.volume = GameSettings.SfxVolume * volumeScale;
        source.Play();

        LastSfx = clip;
        SfxPlayCount++;
    }

    private void StartClip(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
        musicActive = true;
    }

    private void ApplyVolumes()
    {
        musicSource.volume = GameSettings.MusicVolume * fadeScale;

        for (int i = 0; i < sfxSources.Length; i++)
        {
            sfxSources[i].volume = GameSettings.SfxVolume * sfxScales[i];
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene);
    }

    private void PlayMusicForScene(Scene scene)
    {
        if (scene.name == MenuSceneName)
        {
            PlayMusic(titleMusic);
        }
    }
}
