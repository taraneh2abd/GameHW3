using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private float baseSpawnInterval = 0.9f;
    [SerializeField] private float baseFallSpeed = 2.2f;
    [SerializeField] private float minScale = 0.7f;
    [SerializeField] private float maxScale = 1.3f;
    [SerializeField] private float maxDrift = 0.8f;
    [SerializeField] private float maxSpin = 40f;
    [SerializeField] private float topMargin = 1.5f;
    [SerializeField] private float sideMargin = 1f;

    private readonly List<Asteroid> active = new List<Asteroid>();
    private Coroutine spawnRoutine;
    private int pendingSpawns;

    public event Action FieldCleared;

    public int ActiveCount => active.Count;

    public void SpawnField(int count, float difficultyMultiplier)
    {
        if (asteroidPrefab == null || PoolManager.Instance == null)
        {
            Debug.LogError("AsteroidSpawner needs an asteroid prefab and a PoolManager in the scene.");
            return;
        }

        ClearAll();
        spawnRoutine = StartCoroutine(SpawnRoutine(Mathf.Max(0, count), Mathf.Max(0.1f, difficultyMultiplier)));
    }

    public void ClearAll()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        pendingSpawns = 0;

        foreach (Asteroid asteroid in active.ToArray())
        {
            if (asteroid != null)
            {
                asteroid.Finished -= HandleAsteroidFinished;
                asteroid.Release();
            }
        }

        active.Clear();
    }

    private IEnumerator SpawnRoutine(int count, float multiplier)
    {
        pendingSpawns = count;
        float interval = baseSpawnInterval / multiplier;

        for (int i = 0; i < count; i++)
        {
            SpawnAsteroid(multiplier);
            pendingSpawns--;

            if (i < count - 1)
            {
                yield return new WaitForSeconds(interval);
            }
        }

        spawnRoutine = null;
        CheckCleared();
    }

    private void SpawnAsteroid(float multiplier)
    {
        Camera cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        Vector3 center = cam.transform.position;

        float x = center.x + UnityEngine.Random.Range(-halfWidth + sideMargin, halfWidth - sideMargin);
        float y = center.y + halfHeight + topMargin;

        GameObject instance = PoolManager.Instance.Get(asteroidPrefab, new Vector3(x, y, 0f), Quaternion.identity);
        Asteroid asteroid = instance.GetComponent<Asteroid>();

        if (asteroid == null)
        {
            throw new InvalidOperationException("Asteroid prefab needs an Asteroid component.");
        }

        float speed = baseFallSpeed * multiplier * UnityEngine.Random.Range(0.8f, 1.2f);
        float drift = UnityEngine.Random.Range(-maxDrift, maxDrift);
        float spin = UnityEngine.Random.Range(-maxSpin, maxSpin);
        float scale = UnityEngine.Random.Range(minScale, maxScale);

        asteroid.Init(speed, drift, spin, scale);
        asteroid.Finished += HandleAsteroidFinished;
        active.Add(asteroid);
    }

    private void HandleAsteroidFinished(Asteroid asteroid)
    {
        asteroid.Finished -= HandleAsteroidFinished;
        active.Remove(asteroid);
        CheckCleared();
    }

    private void CheckCleared()
    {
        if (spawnRoutine == null && pendingSpawns == 0 && active.Count == 0)
        {
            FieldCleared?.Invoke();
        }
    }
}
