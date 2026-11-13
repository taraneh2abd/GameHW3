using System.Collections;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [SerializeField] private LevelDataSO level;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private AsteroidSpawner asteroidSpawner;
    [SerializeField] private float introDelay = 1.5f;
    [SerializeField] private float bossSpawnHeight = 2f;

    private Coroutine sequence;
    private GameObject activeBoss;
    private bool roundCleared;

    public int CurrentRound { get; private set; }
    public bool Running { get; private set; }

    private void OnEnable()
    {
        waveSpawner.WaveCleared += HandleRoundCleared;
        asteroidSpawner.FieldCleared += HandleRoundCleared;
        GameEvents.BossDefeated += HandleRoundCleared;
        GameEvents.GameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        waveSpawner.WaveCleared -= HandleRoundCleared;
        asteroidSpawner.FieldCleared -= HandleRoundCleared;
        GameEvents.BossDefeated -= HandleRoundCleared;
        GameEvents.GameOver -= HandleGameOver;
    }

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("RoundManager needs a GameManager.");
            return;
        }

        if (level == null || level.Rounds == null || level.Rounds.Length == 0)
        {
            Debug.LogError("RoundManager needs a level with rounds.");
            return;
        }

        int start = Mathf.Clamp(GameManager.Instance.RoundIndex, 0, level.Rounds.Length - 1);
        sequence = StartCoroutine(RunLevel(start));
    }

    private IEnumerator RunLevel(int startIndex)
    {
        Running = true;

        for (int i = startIndex; i < level.Rounds.Length; i++)
        {
            CurrentRound = i;
            GameManager.Instance.BeginRound(i);
            yield return new WaitForSeconds(introDelay);
            yield return RunRound(level.Rounds[i]);
        }

        Running = false;
        sequence = null;
        GameManager.Instance.CompleteLevel();
    }

    private IEnumerator RunRound(RoundData round)
    {
        roundCleared = false;

        switch (round.Type)
        {
            case RoundType.Wave:
                waveSpawner.SpawnWave(round.Wave, round.DifficultyMultiplier);
                break;
            case RoundType.Asteroids:
                asteroidSpawner.SpawnField(round.AsteroidCount, round.DifficultyMultiplier);
                break;
            case RoundType.Boss:
                if (round.BossPrefab == null)
                {
                    Debug.LogWarning("Boss round has no boss prefab, skipping it.");
                    roundCleared = true;
                }
                else
                {
                    SpawnBoss(round.BossPrefab);
                }
                break;
        }

        while (!roundCleared)
        {
            yield return null;
        }

        activeBoss = null;
    }

    private void SpawnBoss(GameObject prefab)
    {
        Camera cam = Camera.main;
        Vector3 center = cam.transform.position;
        Vector3 position = new Vector3(center.x, center.y + cam.orthographicSize + bossSpawnHeight, 0f);
        activeBoss = Instantiate(prefab, position, Quaternion.identity);
    }

    private void HandleRoundCleared()
    {
        roundCleared = true;
    }

    private void HandleGameOver()
    {
        if (sequence != null)
        {
            StopCoroutine(sequence);
            sequence = null;
        }

        Running = false;
        waveSpawner.ClearAll();
        asteroidSpawner.ClearAll();

        if (activeBoss != null)
        {
            Destroy(activeBoss);
            activeBoss = null;
        }
    }
}
