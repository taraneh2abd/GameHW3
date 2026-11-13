using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private LevelDataSO[] levels;
    [SerializeField] private int startLives = 3;
    [SerializeField] private int startMissiles = 2;
    [SerializeField] private int maxPowerLevel = 3;
    [SerializeField] private int foodPerMissile = 20;
    [SerializeField] private float levelCompleteDelay = 2.5f;

    private const string MenuSceneName = "MainMenu";

    private bool gameOverRaised;
    private bool levelTransitioning;

    public int LevelIndex { get; private set; }
    public int RoundIndex { get; private set; }
    public int Score { get; private set; }
    public int Lives { get; private set; }
    public int Missiles { get; private set; }
    public int PowerLevel { get; private set; }
    public int FoodMeter { get; private set; }

    public LevelDataSO CurrentLevel
    {
        get
        {
            if (levels == null || LevelIndex < 0 || LevelIndex >= levels.Length)
            {
                return null;
            }

            return levels[LevelIndex];
        }
    }

    private int CurrentLevelNumber
    {
        get
        {
            LevelDataSO level = CurrentLevel;
            return level != null ? level.LevelNumber : LevelIndex + 1;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResetState();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject prefab = Resources.Load<GameObject>("GameManager");
        if (prefab == null)
        {
            Debug.LogError("Resources/GameManager prefab is missing.");
            return;
        }

        Instantiate(prefab);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void ResetState()
    {
        LevelIndex = 0;
        RoundIndex = 0;
        Score = 0;
        Lives = startLives;
        Missiles = startMissiles;
        PowerLevel = 1;
        FoodMeter = 0;
        gameOverRaised = false;
        levelTransitioning = false;
    }

    public void NewGame()
    {
        ResetState();
        SaveSystem.Delete();
        LoadCurrentLevelScene();
    }

    public bool ContinueGame()
    {
        if (!SaveSystem.TryLoad(out SaveData data))
        {
            return false;
        }

        ResetState();
        LevelIndex = Mathf.Clamp(data.levelIndex, 0, Mathf.Max(0, levels.Length - 1));
        RoundIndex = data.roundIndex;
        Score = data.score;
        Lives = data.lives;
        Missiles = data.missiles;
        PowerLevel = Mathf.Clamp(data.powerLevel, 1, maxPowerLevel);
        FoodMeter = Mathf.Clamp(data.foodMeter, 0, foodPerMissile - 1);

        LoadCurrentLevelScene();
        return true;
    }

    public void CompleteLevel()
    {
        if (levelTransitioning || gameOverRaised)
        {
            return;
        }

        levelTransitioning = true;
        StartCoroutine(CompleteLevelRoutine());
    }

    public void Retry()
    {
        RoundIndex = 0;
        Lives = startLives;
        Missiles = startMissiles;
        PowerLevel = 1;
        FoodMeter = 0;
        gameOverRaised = false;
        levelTransitioning = false;
        Autosave();
        LoadCurrentLevelScene();
    }

    public void ReturnToMenu()
    {
        gameOverRaised = false;
        levelTransitioning = false;
        SceneManager.LoadScene(MenuSceneName);
    }

    public void AddScore(int amount)
    {
        Score += amount;
        GameEvents.RaiseScoreChanged(Score);
    }

    public void LoseLife()
    {
        if (gameOverRaised || levelTransitioning || Lives <= 0)
        {
            return;
        }

        Lives--;
        PowerLevel = Mathf.Max(1, PowerLevel - 1);

        GameEvents.RaisePlayerHit();
        GameEvents.RaiseLivesChanged(Lives);
        GameEvents.RaisePowerLevelChanged(PowerLevel);

        if (Lives <= 0)
        {
            HandleGameOver();
        }
    }

    public void UpgradeWeapon()
    {
        PowerLevel = Mathf.Min(maxPowerLevel, PowerLevel + 1);
        GameEvents.RaisePowerLevelChanged(PowerLevel);
    }

    public void AddMissile()
    {
        Missiles++;
        GameEvents.RaiseMissilesChanged(Missiles);
    }

    public bool TryUseMissile()
    {
        if (Missiles <= 0)
        {
            return false;
        }

        Missiles--;
        GameEvents.RaiseMissilesChanged(Missiles);
        return true;
    }

    public void AddFood(int amount)
    {
        FoodMeter += amount;

        if (FoodMeter >= foodPerMissile)
        {
            FoodMeter = 0;
            AddMissile();
        }

        GameEvents.RaiseFoodMeterChanged(NormalizedFood());
    }

    public void BeginRound(int roundIndex)
    {
        RoundIndex = roundIndex;
        GameEvents.RaiseRoundStarted(CurrentLevelNumber, RoundIndex + 1);
        Autosave();
    }

    public void BroadcastState()
    {
        GameEvents.RaiseScoreChanged(Score);
        GameEvents.RaiseLivesChanged(Lives);
        GameEvents.RaiseMissilesChanged(Missiles);
        GameEvents.RaisePowerLevelChanged(PowerLevel);
        GameEvents.RaiseFoodMeterChanged(NormalizedFood());
    }

    public void Autosave()
    {
        SaveSystem.Save(new SaveData
        {
            levelIndex = LevelIndex,
            roundIndex = RoundIndex,
            score = Score,
            lives = Lives,
            missiles = Missiles,
            powerLevel = PowerLevel,
            foodMeter = FoodMeter
        });
    }

    private IEnumerator CompleteLevelRoutine()
    {
        GameEvents.RaiseLevelCompleted(CurrentLevelNumber);
        yield return new WaitForSecondsRealtime(levelCompleteDelay);

        LevelDataSO next = CurrentLevel != null ? CurrentLevel.NextLevel : null;
        if (next == null)
        {
            SaveSystem.Delete();
            GameEvents.RaiseVictory();
            yield break;
        }

        LevelIndex++;
        RoundIndex = 0;
        Autosave();
        levelTransitioning = false;
        SceneManager.LoadScene(next.SceneName);
    }

    private void LoadCurrentLevelScene()
    {
        LevelDataSO level = CurrentLevel;
        if (level == null)
        {
            Debug.LogError("No level data for level index " + LevelIndex);
            return;
        }

        SceneManager.LoadScene(level.SceneName);
    }

    private float NormalizedFood()
    {
        return Mathf.Clamp01((float)FoodMeter / foodPerMissile);
    }

    private void HandleGameOver()
    {
        if (gameOverRaised)
        {
            return;
        }

        gameOverRaised = true;

        SaveSystem.Save(new SaveData
        {
            levelIndex = LevelIndex,
            roundIndex = 0,
            score = Score,
            lives = startLives,
            missiles = startMissiles,
            powerLevel = 1,
            foodMeter = 0
        });

        GameEvents.RaisePlayerDied();
        GameEvents.RaiseGameOver();
    }
}
