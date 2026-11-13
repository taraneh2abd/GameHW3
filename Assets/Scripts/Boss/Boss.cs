using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour, IDamageable, IScreenClearTarget
{
    [SerializeField] private string bossName = "Boss";
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int scoreValue = 5000;
    [SerializeField] private float missileMinFraction = 0.15f;
    [SerializeField] private float missileMaxFraction = 0.2f;

    [SerializeField] private float hoverTopOffset = 2.4f;
    [SerializeField] private float enterSpeed = 2f;

    [SerializeField] private SpriteAnimator henAnimator;
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] hitFrames;
    [SerializeField] private Sprite[] diedFrames;
    [SerializeField] private float animationFps = 8f;
    [SerializeField] private float hitDuration = 0.2f;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionScale = 0.1f;
    [SerializeField] private float explosionLifetime = 0.8f;
    [SerializeField] private float explosionInterval = 0.12f;
    [SerializeField] private Vector2 explosionSpread = new Vector2(2f, 1.5f);
    [SerializeField] private float deathDuration = 2f;

    private Camera cam;
    private Collider2D body;
    private SpriteRenderer[] renderers;
    private Coroutine hitRoutine;
    private int health;
    private bool entered;
    private bool dead;

    public string BossName => bossName;
    public int Health => health;
    public int MaxHealth => maxHealth;
    public float HealthFraction => maxHealth > 0 ? (float)health / maxHealth : 0f;
    public bool HasEntered => entered;
    public bool IsDead => dead;

    private Camera Cam
    {
        get
        {
            if (cam == null)
            {
                cam = Camera.main;
            }

            return cam;
        }
    }

    public float HoverY => Cam.transform.position.y + Cam.orthographicSize - hoverTopOffset;

    private void Awake()
    {
        health = maxHealth;
        body = GetComponent<Collider2D>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        PlayHenAnimation(idleFrames, true);
    }

    private void Update()
    {
        if (entered || dead)
        {
            return;
        }

        Vector3 target = new Vector3(transform.position.x, HoverY, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, target, enterSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            entered = true;
            GameEvents.RaiseBossSpawned(bossName, maxHealth);
        }
    }

    public void TakeDamage(int amount)
    {
        ApplyDamage(amount);
    }

    public void OnScreenClear()
    {
        if (dead || !entered)
        {
            return;
        }

        float fraction = Random.Range(missileMinFraction, missileMaxFraction);
        ApplyDamage(Mathf.Max(1, Mathf.RoundToInt(maxHealth * fraction)));
    }

    public void BeginDeath()
    {
        if (hitRoutine != null)
        {
            StopCoroutine(hitRoutine);
            hitRoutine = null;
        }

        PlayHenAnimation(diedFrames, false);
    }

    public void SpawnExplosion(float scaleMultiplier)
    {
        if (explosionPrefab == null)
        {
            return;
        }

        Vector3 offset = new Vector3(Random.Range(-explosionSpread.x, explosionSpread.x), Random.Range(-explosionSpread.y, explosionSpread.y), 0f);
        GameObject blast = Instantiate(explosionPrefab, transform.position + offset, Quaternion.identity);
        blast.transform.localScale = Vector3.one * (explosionScale * scaleMultiplier);
        Destroy(blast, explosionLifetime);
    }

    public void HideBody()
    {
        foreach (SpriteRenderer sprite in renderers)
        {
            sprite.enabled = false;
        }
    }

    public void FinishDeath()
    {
        GameEvents.RaiseBossDefeated();
        Destroy(gameObject);
    }

    private void ApplyDamage(int amount)
    {
        if (dead || !entered || amount <= 0)
        {
            return;
        }

        health = Mathf.Max(0, health - amount);
        GameEvents.RaiseBossHealthChanged(health, maxHealth);

        if (health <= 0)
        {
            Die();
            return;
        }

        if (hitRoutine != null)
        {
            StopCoroutine(hitRoutine);
        }

        hitRoutine = StartCoroutine(HitRoutine());
    }

    private void Die()
    {
        dead = true;
        body.enabled = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        BeginDeath();
        float elapsed = 0f;
        float explosionTimer = 0f;
        bool burst = false;

        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;
            explosionTimer -= Time.deltaTime;

            if (!burst && explosionTimer <= 0f)
            {
                SpawnExplosion(1f);
                explosionTimer = explosionInterval;
            }

            if (!burst && elapsed >= deathDuration * 0.8f)
            {
                burst = true;
                SpawnExplosion(3f);
                HideBody();
            }

            yield return null;
        }

        FinishDeath();
    }

    private void PlayHenAnimation(Sprite[] frames, bool loop)
    {
        if (henAnimator != null && frames != null && frames.Length > 0)
        {
            henAnimator.Play(frames, animationFps, loop);
        }
    }

    private IEnumerator HitRoutine()
    {
        PlayHenAnimation(hitFrames, true);
        yield return new WaitForSeconds(hitDuration);
        hitRoutine = null;
        PlayHenAnimation(idleFrames, true);
    }
}
