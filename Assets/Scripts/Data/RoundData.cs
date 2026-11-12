using System;
using UnityEngine;

[Serializable]
public class RoundData
{
    [SerializeField] private RoundType type;
    [SerializeField] private WaveDataSO wave;
    [SerializeField] private float difficultyMultiplier = 1f;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private int asteroidCount;

    public RoundType Type => type;
    public WaveDataSO Wave => wave;
    public float DifficultyMultiplier => difficultyMultiplier;
    public GameObject BossPrefab => bossPrefab;
    public int AsteroidCount => asteroidCount;
}
