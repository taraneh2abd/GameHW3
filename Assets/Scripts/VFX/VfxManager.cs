using UnityEngine;

public class VfxManager : MonoBehaviour
{
    [SerializeField] private GameObject featherBurstPrefab;
    [SerializeField] private int prewarmCount = 6;

    private void OnEnable()
    {
        GameEvents.EnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        GameEvents.EnemyKilled -= HandleEnemyKilled;
    }

    private void Start()
    {
        if (PoolManager.Instance != null && featherBurstPrefab != null)
        {
            PoolManager.Instance.Prewarm(featherBurstPrefab, prewarmCount);
        }
    }

    private void HandleEnemyKilled(Vector3 position)
    {
        Spawn(featherBurstPrefab, position);
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
