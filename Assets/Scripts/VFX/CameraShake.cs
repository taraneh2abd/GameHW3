using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float maxOffset = 0.5f;
    [SerializeField] private float frequency = 24f;
    [SerializeField] private float decayPerSecond = 1.8f;
    [SerializeField] private float playerHitTrauma = 0.4f;
    [SerializeField] private float missileTrauma = 0.65f;
    [SerializeField] private float bossDefeatedTrauma = 1f;
    [SerializeField] private float bossHitTrauma = 0.15f;

    private Vector3 restPosition;
    private float trauma;
    private float noiseTime;
    private bool offset;

    public float Trauma => trauma;
    public Vector3 RestPosition => restPosition;

    private void Awake()
    {
        restPosition = transform.position;
    }

    private void OnEnable()
    {
        GameEvents.PlayerHit += HandlePlayerHit;
        GameEvents.MissileDetonated += HandleMissileDetonated;
        GameEvents.BossDefeated += HandleBossDefeated;
        GameEvents.BossHealthChanged += HandleBossHealthChanged;
    }

    private void OnDisable()
    {
        GameEvents.PlayerHit -= HandlePlayerHit;
        GameEvents.MissileDetonated -= HandleMissileDetonated;
        GameEvents.BossDefeated -= HandleBossDefeated;
        GameEvents.BossHealthChanged -= HandleBossHealthChanged;
        trauma = 0f;
        Restore();
    }

    private void LateUpdate()
    {
        if (trauma <= 0f)
        {
            Restore();
            return;
        }

        float dt = Time.unscaledDeltaTime;
        noiseTime += dt * frequency;
        trauma = Mathf.Max(0f, trauma - decayPerSecond * dt);

        if (trauma <= 0f)
        {
            Restore();
            return;
        }

        float strength = Mathf.Pow(trauma, 1.5f) * maxOffset;
        float x = (Mathf.PerlinNoise(noiseTime, 0.3f) * 2f - 1f) * strength;
        float y = (Mathf.PerlinNoise(0.7f, noiseTime) * 2f - 1f) * strength;
        transform.position = restPosition + new Vector3(x, y, 0f);
        offset = true;
    }

    public void AddTrauma(float amount)
    {
        trauma = Mathf.Clamp01(trauma + amount);
    }

    private void Restore()
    {
        if (!offset)
        {
            return;
        }

        transform.position = restPosition;
        offset = false;
    }

    private void HandlePlayerHit()
    {
        AddTrauma(playerHitTrauma);
    }

    private void HandleMissileDetonated(Vector3 position)
    {
        AddTrauma(missileTrauma);
    }

    private void HandleBossDefeated()
    {
        AddTrauma(bossDefeatedTrauma);
    }

    private void HandleBossHealthChanged(int current, int max)
    {
        AddTrauma(bossHitTrauma);
    }
}
