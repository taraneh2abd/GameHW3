using UnityEngine;

[CreateAssetMenu(fileName = "NewWave", menuName = "Game/Wave Data")]
public class WaveDataSO : ScriptableObject
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int count = 10;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private FormationType formation;
    [SerializeField, Range(0f, 1f)] private float diveBombChance;

    public GameObject EnemyPrefab => enemyPrefab;
    public int Count => count;
    public float MoveSpeed => moveSpeed;
    public float FireRate => fireRate;
    public FormationType Formation => formation;
    public float DiveBombChance => diveBombChance;
}
