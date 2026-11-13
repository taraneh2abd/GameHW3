using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<int> ScoreChanged;
    public static event Action<int> LivesChanged;
    public static event Action<int> MissilesChanged;
    public static event Action<int> PowerLevelChanged;
    public static event Action<float> FoodMeterChanged;
    public static event Action<int, int> RoundStarted;
    public static event Action<int> LevelCompleted;
    public static event Action<string, int> BossSpawned;
    public static event Action<int, int> BossHealthChanged;
    public static event Action BossDefeated;
    public static event Action PlayerHit;
    public static event Action PlayerDied;
    public static event Action PlayerFired;
    public static event Action<Vector3> EnemyKilled;
    public static event Action EggDropped;
    public static event Action<Vector3> EggBroken;
    public static event Action PickupCollected;
    public static event Action MissileLaunched;
    public static event Action<Vector3> MissileDetonated;
    public static event Action GameOver;
    public static event Action Victory;

    public static void RaiseScoreChanged(int score) => ScoreChanged?.Invoke(score);
    public static void RaiseLivesChanged(int lives) => LivesChanged?.Invoke(lives);
    public static void RaiseMissilesChanged(int missiles) => MissilesChanged?.Invoke(missiles);
    public static void RaisePowerLevelChanged(int level) => PowerLevelChanged?.Invoke(level);
    public static void RaiseFoodMeterChanged(float normalized) => FoodMeterChanged?.Invoke(normalized);
    public static void RaiseRoundStarted(int levelNumber, int roundNumber) => RoundStarted?.Invoke(levelNumber, roundNumber);
    public static void RaiseLevelCompleted(int levelNumber) => LevelCompleted?.Invoke(levelNumber);
    public static void RaiseBossSpawned(string bossName, int maxHealth) => BossSpawned?.Invoke(bossName, maxHealth);
    public static void RaiseBossHealthChanged(int current, int max) => BossHealthChanged?.Invoke(current, max);
    public static void RaiseBossDefeated() => BossDefeated?.Invoke();
    public static void RaisePlayerHit() => PlayerHit?.Invoke();
    public static void RaisePlayerDied() => PlayerDied?.Invoke();
    public static void RaisePlayerFired() => PlayerFired?.Invoke();
    public static void RaiseEnemyKilled(Vector3 position) => EnemyKilled?.Invoke(position);
    public static void RaiseEggDropped() => EggDropped?.Invoke();
    public static void RaiseEggBroken(Vector3 position) => EggBroken?.Invoke(position);
    public static void RaisePickupCollected() => PickupCollected?.Invoke();
    public static void RaiseMissileLaunched() => MissileLaunched?.Invoke();
    public static void RaiseMissileDetonated(Vector3 position) => MissileDetonated?.Invoke(position);
    public static void RaiseGameOver() => GameOver?.Invoke();
    public static void RaiseVictory() => Victory?.Invoke();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetAll()
    {
        ScoreChanged = null;
        LivesChanged = null;
        MissilesChanged = null;
        PowerLevelChanged = null;
        FoodMeterChanged = null;
        RoundStarted = null;
        LevelCompleted = null;
        BossSpawned = null;
        BossHealthChanged = null;
        BossDefeated = null;
        PlayerHit = null;
        PlayerDied = null;
        PlayerFired = null;
        EnemyKilled = null;
        EggDropped = null;
        EggBroken = null;
        PickupCollected = null;
        MissileLaunched = null;
        MissileDetonated = null;
        GameOver = null;
        Victory = null;
    }
}
