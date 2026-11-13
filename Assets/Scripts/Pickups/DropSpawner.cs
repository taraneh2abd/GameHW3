using UnityEngine;

public class DropSpawner : MonoBehaviour
{
    [SerializeField] private GameObject drumstickPrefab;
    [SerializeField] private GameObject burgerPrefab;
    [SerializeField, Range(0f, 1f)] private float drumstickChance = 0.45f;
    [SerializeField, Range(0f, 1f)] private float burgerChance = 0.15f;

    private void OnEnable()
    {
        GameEvents.EnemyKilled += HandleEnemyKilled;
        GameEvents.GameOver += ClearAll;
    }

    private void OnDisable()
    {
        GameEvents.EnemyKilled -= HandleEnemyKilled;
        GameEvents.GameOver -= ClearAll;
    }

    public void ClearAll()
    {
        foreach (Pickup pickup in FindObjectsByType<Pickup>(FindObjectsSortMode.None))
        {
            pickup.Release();
        }
    }

    private void HandleEnemyKilled(Vector3 position)
    {
        if (PoolManager.Instance == null)
        {
            return;
        }

        GameObject prefab = ChooseDrop();
        if (prefab == null)
        {
            return;
        }

        PoolManager.Instance.Get(prefab, position, Quaternion.identity);
    }

    private GameObject ChooseDrop()
    {
        float roll = Random.value;

        if (roll < burgerChance)
        {
            return burgerPrefab;
        }

        roll -= burgerChance;
        if (roll < drumstickChance)
        {
            return drumstickPrefab;
        }

        return null;
    }
}
