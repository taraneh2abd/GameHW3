using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteAnimator))]
public class Chicken : MonoBehaviour, IDamageable, IScreenClearTarget
{
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private float animationFps = 6f;
    [SerializeField] private float hurtDuration = 0.15f;
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] walkFrames;
    [SerializeField] private Sprite[] hurtFrames;
    [SerializeField] private Sprite[] angryFrames;

    private SpriteAnimator animator;
    private HitFlash hitFlash;
    private Sprite[] currentFrames;
    private int health;
    private Coroutine hurtRoutine;

    public event Action<Chicken> Died;

    public bool IsAlive { get; private set; } = true;
    public int Health => health;
    public int MaxHealth => maxHealth;
    public int ScoreValue => scoreValue;

    private void Awake()
    {
        animator = GetComponent<SpriteAnimator>();
        hitFlash = GetComponent<HitFlash>();
        health = maxHealth;
        currentFrames = idleFrames;
    }

    public void PlayIdle()
    {
        Play(idleFrames);
    }

    public void PlayWalk()
    {
        Play(walkFrames);
    }

    public void PlayAngry()
    {
        Play(angryFrames);
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
            Die();
            return;
        }

        if (hitFlash != null)
        {
            hitFlash.Flash();
        }

        if (hurtRoutine != null)
        {
            StopCoroutine(hurtRoutine);
        }

        hurtRoutine = StartCoroutine(HurtRoutine());
    }

    public void OnScreenClear()
    {
        if (IsAlive)
        {
            Die();
        }
    }

    private void Play(Sprite[] frames)
    {
        currentFrames = frames;

        if (hurtRoutine == null)
        {
            animator.Play(frames, animationFps, true);
        }
    }

    private IEnumerator HurtRoutine()
    {
        animator.Play(hurtFrames, animationFps * 2f, true);
        yield return new WaitForSeconds(hurtDuration);
        hurtRoutine = null;
        animator.Play(currentFrames, animationFps, true);
    }

    private void Die()
    {
        IsAlive = false;

        Collider2D body = GetComponent<Collider2D>();
        if (body != null)
        {
            body.enabled = false;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        GameEvents.RaiseEnemyKilled(transform.position);
        Died?.Invoke(this);
        Destroy(gameObject);
    }
}
