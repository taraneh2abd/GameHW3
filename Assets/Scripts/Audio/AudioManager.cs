using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource[] sfxSources;
    [SerializeField] private AudioClip titleMusic;
    [SerializeField] private AudioClip bossMusic1;
    [SerializeField] private AudioClip bossMusic2;
    [SerializeField] private AudioClip endingMusic;
    [SerializeField] private int secondBossLevel = 3;
    [SerializeField] private AudioClip[] shotClips;
    [SerializeField] private AudioClip missileLaunchClip;
    [SerializeField] private AudioClip missileBlastClip;
    [SerializeField] private AudioClip[] killClips;
    [SerializeField] private AudioClip playerHitClip;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip powerUpClip;
    [SerializeField] private AudioClip missileUpClip;
    [SerializeField] private AudioClip levelCompleteClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip bossAlarmClip;
    [SerializeField] private AudioClip bossDefeatedClip;
    [SerializeField] private AudioClip[] cluckClips;
    [SerializeField] private AudioClip uiButtonClip;
    [SerializeField] private AudioClip uiMoveClip;
    [SerializeField] private AudioClip uiSelectClip;
    [SerializeField] private float shotInterval = 0.09f;
    [SerializeField] private float eggCluckInterval = 0.15f;
    [SerializeField] private float killCluckInterval = 0.06f;
    [SerializeField] private float fadeTime = 0.6f;

    private const string MenuSceneName = "MainMenu";

    private float[] sfxScales;
    private int nextSfxSource;
    private float fadeScale;
    private float fadeTarget;
    private bool musicActive;
    private AudioClip pendingClip;
    private float nextShotTime;
    private float nextEggCluckTime;
    private float nextKillCluckTime;
    private int lastPowerLevel = 1;
    private int lastMissiles;

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
        GameEvents.BossSpawned += HandleBossSpawned;
        GameEvents.Victory += HandleVictory;
        GameEvents.GameOver += HandleGameOver;
        GameEvents.PlayerFired += HandlePlayerFired;
        GameEvents.MissileLaunched += HandleMissileLaunched;
        GameEvents.MissileDetonated += HandleMissileDetonated;
        GameEvents.EnemyKilled += HandleEnemyKilled;
        GameEvents.EggDropped += HandleEggDropped;
        GameEvents.PlayerHit += HandlePlayerHit;
        GameEvents.PickupCollected += HandlePickupCollected;
        GameEvents.PowerLevelChanged += HandlePowerLevelChanged;
        GameEvents.MissilesChanged += HandleMissilesChanged;
        GameEvents.LevelCompleted += HandleLevelCompleted;
        GameEvents.BossDefeated += HandleBossDefeated;
    }

    private void OnDisable()
    {
        if (Instance != this)
        {
            return;
        }

        GameSettings.Changed -= ApplyVolumes;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        GameEvents.BossSpawned -= HandleBossSpawned;
        GameEvents.Victory -= HandleVictory;
        GameEvents.GameOver -= HandleGameOver;
        GameEvents.PlayerFired -= HandlePlayerFired;
        GameEvents.MissileLaunched -= HandleMissileLaunched;
        GameEvents.MissileDetonated -= HandleMissileDetonated;
        GameEvents.EnemyKilled -= HandleEnemyKilled;
        GameEvents.EggDropped -= HandleEggDropped;
        GameEvents.PlayerHit -= HandlePlayerHit;
        GameEvents.PickupCollected -= HandlePickupCollected;
        GameEvents.PowerLevelChanged -= HandlePowerLevelChanged;
        GameEvents.MissilesChanged -= HandleMissilesChanged;
        GameEvents.LevelCompleted -= HandleLevelCompleted;
        GameEvents.BossDefeated -= HandleBossDefeated;
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
            SyncCounters();
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

    public void PlaySfx(AudioClip clip, float volumeScale = 1f, float pitchVariation = 0f, float basePitch = 1f)
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
        source.pitch = basePitch + Random.Range(-pitchVariation, pitchVariation);
        source.volume = GameSettings.SfxVolume * volumeScale;
        source.Play();

        LastSfx = clip;
        SfxPlayCount++;
    }

    public void PlayUiButton()
    {
        PlaySfx(uiButtonClip, 0.8f);
    }

    public void PlayUiMove()
    {
        PlaySfx(uiMoveClip, 0.6f);
    }

    public void PlayUiSelect()
    {
        PlaySfx(uiSelectClip, 0.8f);
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
        SyncCounters();
        PlayMusicForScene(scene);
    }

    private void SyncCounters()
    {
        GameManager state = GameManager.Instance;
        lastPowerLevel = state != null ? state.PowerLevel : 1;
        lastMissiles = state != null ? state.Missiles : 0;
    }

    private void PlayMusicForScene(Scene scene)
    {
        if (scene.name == MenuSceneName)
        {
            PlayMusic(titleMusic);
            return;
        }

        GameManager manager = GameManager.Instance;
        if (manager != null && manager.CurrentLevel != null)
        {
            PlayMusic(manager.CurrentLevel.Music);
        }
    }

    private void HandleBossSpawned(string bossName, int maxHealth)
    {
        PlaySfx(bossAlarmClip);
        GameManager manager = GameManager.Instance;
        int levelNumber = manager != null && manager.CurrentLevel != null ? manager.CurrentLevel.LevelNumber : 1;
        PlayMusic(levelNumber >= secondBossLevel ? bossMusic2 : bossMusic1);
    }

    private void HandleVictory()
    {
        PlayMusic(endingMusic);
    }

    private void HandleGameOver()
    {
        StopMusic();
        PlaySfx(gameOverClip);
    }

    private void HandlePlayerFired()
    {
        if (Time.unscaledTime < nextShotTime)
        {
            return;
        }

        nextShotTime = Time.unscaledTime + shotInterval;
        PlaySfx(RandomClip(shotClips), 0.5f, 0.08f);
    }

    private void HandleMissileLaunched()
    {
        PlaySfx(missileLaunchClip, 0.9f, 0f, 0.6f);
    }

    private void HandleMissileDetonated(Vector3 position)
    {
        PlaySfx(missileBlastClip);
    }

    private void HandleEnemyKilled(Vector3 position)
    {
        PlaySfx(RandomClip(killClips), 0.6f, 0.1f);

        if (Time.unscaledTime >= nextKillCluckTime)
        {
            nextKillCluckTime = Time.unscaledTime + killCluckInterval;
            PlaySfx(RandomClip(cluckClips), 0.7f, 0.1f, 1.15f);
        }
    }

    private void HandleEggDropped()
    {
        if (Time.unscaledTime < nextEggCluckTime)
        {
            return;
        }

        nextEggCluckTime = Time.unscaledTime + eggCluckInterval;
        PlaySfx(RandomClip(cluckClips), 0.45f, 0.1f);
    }

    private void HandlePlayerHit()
    {
        PlaySfx(playerHitClip);
    }

    private void HandlePickupCollected()
    {
        PlaySfx(pickupClip, 0.8f);
    }

    private void HandlePowerLevelChanged(int level)
    {
        if (level > lastPowerLevel)
        {
            PlaySfx(powerUpClip);
        }

        lastPowerLevel = level;
    }

    private void HandleMissilesChanged(int missiles)
    {
        if (missiles > lastMissiles)
        {
            PlaySfx(missileUpClip);
        }

        lastMissiles = missiles;
    }

    private void HandleLevelCompleted(int levelNumber)
    {
        PlaySfx(levelCompleteClip);
    }

    private void HandleBossDefeated()
    {
        PlaySfx(bossDefeatedClip);
    }

    private static AudioClip RandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            return null;
        }

        return clips[Random.Range(0, clips.Length)];
    }
}
