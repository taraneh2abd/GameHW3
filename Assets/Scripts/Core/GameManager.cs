using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private LevelDataSO[] levels;
    [SerializeField] private int startLives = 3;
    [SerializeField] private int startMissiles = 2;
    [SerializeField] private int maxPowerLevel = 3;
    [SerializeField] private int foodPerMissile = 20;

    private bool gameOverRaised;

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
    }

    public void AddScore(int amount)
    {
        Score += amount;
        GameEvents.RaiseScoreChanged(Score);
    }

    public void LoseLife()
    {
        if (gameOverRaised || Lives <= 0)
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
