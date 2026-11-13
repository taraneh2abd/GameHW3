using UnityEngine;

public class VfxManager : MonoBehaviour
{
    [SerializeField] private GameObject featherBurstPrefab;
    [SerializeField] private GameObject eggSplatPrefab;
    [SerializeField] private GameObject shockwavePrefab;
    [SerializeField] private ScreenFlash screenFlash;
    [SerializeField] private int prewarmCount = 6;

    private void OnEnable()
    {
        GameEvents.EnemyKilled += HandleEnemyKilled;
        GameEvents.EggBroken += HandleEggBroken;
        GameEvents.MissileDetonated += HandleMissileDetonated;
    }

    private void OnDisable()
    {
        GameEvents.EnemyKilled -= HandleEnemyKilled;
        GameEvents.EggBroken -= HandleEggBroken;
        GameEvents.MissileDetonated -= HandleMissileDetonated;
    }

    private void Start()
    {
        if (PoolManager.Instance == null)
        {
            return;
        }

        Prewarm(featherBurstPrefab, prewarmCount);
        Prewarm(eggSplatPrefab, prewarmCount);
        Prewarm(shockwavePrefab, 2);
    }

    private void HandleEnemyKilled(Vector3 position)
    {
        Spawn(featherBurstPrefab, position);
    }

    private void HandleEggBroken(Vector3 position)
    {
        Spawn(eggSplatPrefab, position);
    }

    private void HandleMissileDetonated(Vector3 position)
    {
        Spawn(shockwavePrefab, position);

        if (screenFlash != null)
        {
            screenFlash.Play();
        }
    }

    private void Prewarm(GameObject prefab, int count)
    {
        if (prefab != null)
        {
            PoolManager.Instance.Prewarm(prefab, count);
        }
    }

    private void Spawn(GameObject prefab, Vector3 position)
    {
        if (PoolManager.Instance == null || prefab == null)
        {
            return;
        }

        PoolManager.Instance.Get(prefab, position, Quaternion.identity);
    }
}
