using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 0.25f;
    [SerializeField] private float entrySpeedUnit = 5f;
    [SerializeField] private float dropRateScale = 0.2f;

    private readonly List<Chicken> chickens = new List<Chicken>();
    private Coroutine spawnRoutine;
    private int pendingSpawns;

    public event Action WaveCleared;

    public int AliveCount => chickens.Count;
    public int RemainingCount => chickens.Count + pendingSpawns;

    public void SpawnWave(WaveDataSO wave, float difficultyMultiplier)
    {
        if (wave == null || wave.EnemyPrefab == null)
        {
            Debug.LogError("WaveSpawner needs a wave with an enemy prefab.");
            return;
        }

        ClearAll();
        spawnRoutine = StartCoroutine(SpawnRoutine(wave, Mathf.Max(0.1f, difficultyMultiplier)));
    }

    public void ClearAll()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        pendingSpawns = 0;

        foreach (Chicken chicken in chickens)
        {
            if (chicken != null)
            {
                chicken.Died -= HandleChickenDied;
                Destroy(chicken.gameObject);
            }
        }

        chickens.Clear();

        foreach (Egg egg in FindObjectsByType<Egg>(FindObjectsSortMode.None))
        {
            egg.Release();
        }
    }

    private IEnumerator SpawnRoutine(WaveDataSO wave, float multiplier)
    {
        Camera cam = Camera.main;
        float spacingScale = Mathf.Max(1f, wave.EnemyPrefab.transform.localScale.x);
        List<Vector2> slots = Formation.GetSlots(wave.Formation, wave.Count, cam, spacingScale);
        pendingSpawns = wave.Count;

        for (int i = 0; i < wave.Count; i++)
        {
            SpawnChicken(wave, multiplier, cam, slots[i], i);
            pendingSpawns--;

            if (i < wave.Count - 1)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        spawnRoutine = null;
        CheckCleared();
    }

    private void SpawnChicken(WaveDataSO wave, float multiplier, Camera cam, Vector2 slot, int index)
    {
        GameObject instance = Instantiate(wave.EnemyPrefab, transform);
        Chicken chicken = instance.GetComponent<Chicken>();
        ChickenMover mover = instance.GetComponent<ChickenMover>();

        if (chicken == null || mover == null)
        {
            throw new InvalidOperationException("Enemy prefab needs Chicken and ChickenMover.");
        }

        chicken.Died += HandleChickenDied;
        chickens.Add(chicken);

        List<Vector3> path = EntryPath.Build(EntryPath.PatternAt(index), slot, cam);
        mover.BeginEntry(path.ToArray(), slot, wave.MoveSpeed * multiplier * entrySpeedUnit);

        EggDropper dropper = instance.GetComponent<EggDropper>();
        if (dropper != null)
        {
            dropper.SetDropRate(wave.FireRate * multiplier * dropRateScale);
        }
    }

    private void HandleChickenDied(Chicken chicken)
    {
        chicken.Died -= HandleChickenDied;
        chickens.Remove(chicken);
        CheckCleared();
    }

    private void CheckCleared()
    {
        if (spawnRoutine == null && pendingSpawns == 0 && chickens.Count == 0)
        {
            WaveCleared?.Invoke();
        }
    }
}
