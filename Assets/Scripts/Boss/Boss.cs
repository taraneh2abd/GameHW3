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
    [SerializeField] private float sideMargin = 2.5f;
    [SerializeField] private float enterSpeed = 2f;
    [SerializeField] private float hoverSpeed = 2.5f;

    [SerializeField] private GameObject[] projectilePrefabs;
    [SerializeField] private float muzzleOffset = 1.2f;
    [SerializeField] private float projectileSpeed = 4.5f;
    [SerializeField] private float fireInterval = 1.5f;
    [SerializeField] private int fanCount = 3;
    [SerializeField] private float fanSpread = 15f;

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

    private readonly BossStateMachine machine = new BossStateMachine();

    private BossEnterState enterState;
    private BossBarrageState barrageState;

    private Camera cam;
    private PlayerHealth player;
    private Collider2D body;
    private SpriteRenderer[] renderers;
    private Coroutine hitRoutine;
    private int health;
    private int hoverDirection;
    private bool entered;
    private bool dead;
    private bool stopped;

    public string BossName => bossName;
    public int Health => health;
    public int MaxHealth => maxHealth;
    public float HealthFraction => maxHealth > 0 ? (float)health / maxHealth : 0f;
    public bool HasEntered => entered;
    public bool IsDead => dead;
    public int VolleysFired { get; private set; }
    public int ProjectilesFired { get; private set; }
    public string CurrentStateName => machine.Current != null ? machine.Current.GetType().Name : string.Empty;

    public float EnterSpeed => enterSpeed;
    public float ProjectileSpeed => projectileSpeed;
    public float FireInterval => fireInterval;
    public int FanCount => fanCount;
    public float FanSpread => fanSpread;
    public float ExplosionInterval => explosionInterval;
    public float DeathDuration => deathDuration;

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

    private float HalfHeight => Cam.orthographicSize;
    private float HalfWidth => Cam.orthographicSize * Cam.aspect;

    public float CenterX => Cam.transform.position.x;
    public float HoverY => Cam.transform.position.y + HalfHeight - hoverTopOffset;
    public float BottomY => Cam.transform.position.y - HalfHeight;
    public float MinX => CenterX - Mathf.Max(0f, HalfWidth - sideMargin);
    public float MaxX => CenterX + Mathf.Max(0f, HalfWidth - sideMargin);

    public Vector2 PlayerPosition
    {
        get
        {
            if (player == null)
            {
                player = FindFirstObjectByType<PlayerHealth>();
            }

            if (player == null)
            {
                return new Vector2(CenterX, BottomY + 1f);
            }

            return player.transform.position;
        }
    }

    private void Awake()
    {
        health = maxHealth;
        body = GetComponent<Collider2D>();
        hoverDirection = Random.value < 0.5f ? -1 : 1;

        renderers = GetComponentsInChildren<SpriteRenderer>();

        enterState = new BossEnterState(this);
        barrageState = new BossBarrageState(this);
    }

    private void Start()
    {
        PlayHenAnimation(idleFrames, true);
        machine.ChangeState(enterState);
    }

    private void Update()
    {
        if (stopped)
        {
            return;
        }

        machine.Tick(Time.deltaTime);
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

    public bool MoveTowards(Vector2 target, float speed, float dt)
    {
        Vector3 next = Vector3.MoveTowards(transform.position, new Vector3(target.x, target.y, transform.position.z), speed * dt);
        transform.position = next;
        return Vector2.Distance(next, target) < 0.01f;
    }

    public void Hover(float dt, float speedScale)
    {
        Vector3 position = transform.position;
        position.x += hoverDirection * hoverSpeed * speedScale * dt;

        if (position.x >= MaxX)
        {
            position.x = MaxX;
            hoverDirection = -1;
        }
        else if (position.x <= MinX)
        {
            position.x = MinX;
            hoverDirection = 1;
        }

        position.y = Mathf.MoveTowards(position.y, HoverY, hoverSpeed * dt);
        transform.position = position;
    }

    public void CompleteEntry()
    {
        entered = true;
        GameEvents.RaiseBossSpawned(bossName, maxHealth);
        machine.ChangeState(barrageState);
    }

    public void FireFan(int count, float spread, float speed)
    {
        Vector3 origin = transform.position + Vector3.down * muzzleOffset;
        Vector2 aim = PlayerPosition - (Vector2)origin;
        if (aim.sqrMagnitude < 0.01f)
        {
            aim = Vector2.down;
        }

        for (int i = 0; i < count; i++)
        {
            float angle = (i - (count - 1) * 0.5f) * spread;
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * aim.normalized;
            Fire(0, origin, direction, speed);
        }

        VolleysFired++;
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
        stopped = true;
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

    private void Fire(int prefabIndex, Vector3 position, Vector2 direction, float speed)
    {
        if (PoolManager.Instance == null || projectilePrefabs == null || projectilePrefabs.Length == 0)
        {
            return;
        }

        GameObject prefab = projectilePrefabs[Mathf.Min(prefabIndex, projectilePrefabs.Length - 1)];
        GameObject instance = PoolManager.Instance.Get(prefab, position, Quaternion.identity);
        instance.GetComponent<BossProjectile>().Launch(direction, speed);
        ProjectilesFired++;
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
