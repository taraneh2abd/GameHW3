using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(SpriteAnimator))]
public class Asteroid : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int scoreValue = 50;
    [SerializeField] private float explodeFps = 14f;
    [SerializeField] private float offscreenMargin = 1.5f;
    [SerializeField] private Sprite baseSprite;
    [SerializeField] private Sprite[] explodeFrames;

    private SpriteAnimator animator;
    private Collider2D hitbox;
    private int health;
    private float fallSpeed;
    private float drift;
    private float spinSpeed;
    private bool exploding;
    private bool finished;

    public event Action<Asteroid> Finished;

    public int Health => health;
    public int MaxHealth => maxHealth;
    public int ScoreValue => scoreValue;
    public bool IsAlive => !exploding && !finished;

    private void Awake()
    {
        animator = GetComponent<SpriteAnimator>();
        hitbox = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        ResetState();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void Init(float newFallSpeed, float newDrift, float newSpinSpeed, float scale)
    {
        ResetState();
        fallSpeed = newFallSpeed;
        drift = newDrift;
        spinSpeed = newSpinSpeed;
        transform.localScale = Vector3.one * scale;
    }

    private void Update()
    {
        if (!IsAlive)
        {
            return;
        }

        transform.position += new Vector3(drift, -fallSpeed, 0f) * Time.deltaTime;
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

        if (IsOffscreen())
        {
            Release();
        }
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive)
        {
            return;
        }

        health -= amount;

        if (health <= 0)
        {
            StartCoroutine(ExplodeRoutine());
        }
    }

    public void Release()
    {
        if (finished)
        {
            return;
        }

        finished = true;
        Finished?.Invoke(this);

        PooledObject pooled = GetComponent<PooledObject>();
        if (pooled != null)
        {
            pooled.Release();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ResetState()
    {
        if (animator == null)
        {
            animator = GetComponent<SpriteAnimator>();
            hitbox = GetComponent<Collider2D>();
        }

        StopAllCoroutines();
        health = maxHealth;
        exploding = false;
        finished = false;
        fallSpeed = 0f;
        drift = 0f;
        spinSpeed = 0f;
        hitbox.enabled = true;
        animator.Play(new[] { baseSprite }, 1f, false);
    }

    private IEnumerator ExplodeRoutine()
    {
        exploding = true;
        hitbox.enabled = false;
        transform.rotation = Quaternion.identity;
        animator.Play(explodeFrames, explodeFps, false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        while (!animator.Finished)
        {
            yield return null;
        }

        Release();
    }

    private bool IsOffscreen()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return false;
        }

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        Vector3 center = cam.transform.position;
        Vector3 position = transform.position;

        return position.y < center.y - halfHeight - offscreenMargin
            || position.x < center.x - halfWidth - offscreenMargin
            || position.x > center.x + halfWidth + offscreenMargin;
    }
}
