using UnityEngine;

public class DropSpawner : MonoBehaviour
{
    [SerializeField] private GameObject drumstickPrefab;
    [SerializeField] private GameObject burgerPrefab;
    [SerializeField] private GameObject giftBoxPrefab;
    [SerializeField, Range(0f, 1f)] private float drumstickChance = 0.45f;
    [SerializeField, Range(0f, 1f)] private float burgerChance = 0.15f;
    [SerializeField, Range(0f, 1f)] private float giftBoxChance = 0.05f;
    [SerializeField] private int guaranteedGiftBoxKills = 25;

    private int killsSinceGiftBox;

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
        foreach (Pickup pickup in FindObjectsByType<Pickup>())
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

        killsSinceGiftBox++;

        GameObject prefab = ChooseDrop();
        if (killsSinceGiftBox >= guaranteedGiftBoxKills)
        {
            prefab = giftBoxPrefab;
        }

        if (prefab == null)
        {
            return;
        }

        if (prefab == giftBoxPrefab)
        {
            killsSinceGiftBox = 0;
        }

        PoolManager.Instance.Get(prefab, position, Quaternion.identity);
    }

    private GameObject ChooseDrop()
    {
        float roll = Random.value;

        if (roll < giftBoxChance)
        {
            return giftBoxPrefab;
        }

        roll -= giftBoxChance;
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
