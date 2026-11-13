using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private SpriteRenderer shipRenderer;
    [SerializeField] private Sprite fullHealthSprite;
    [SerializeField] private Sprite slightDamageSprite;
    [SerializeField] private Sprite damagedSprite;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private LayerMask enemyBulletLayers;
    [SerializeField] private float invincibleDuration = 2f;
    [SerializeField] private float blinkInterval = 0.1f;

    private bool invincible;
    private bool dead;

    public bool Invincible => invincible;

    private void OnEnable()
    {
        dead = false;
        invincible = false;
        shipRenderer.enabled = true;
        GameEvents.LivesChanged += HandleLivesChanged;
        GameEvents.GameOver += HandleGameOver;

        if (GameManager.Instance != null)
        {
            ApplyDamageSprite(GameManager.Instance.Lives);
        }
    }

    private void OnDisable()
    {
        GameEvents.LivesChanged -= HandleLivesChanged;
        GameEvents.GameOver -= HandleGameOver;
    }

    public void TakeDamage(int amount)
    {
        if (invincible || dead || GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.LoseLife();

        if (dead)
        {
            return;
        }

        StartCoroutine(BlinkRoutine());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleContact(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsInMask(other.gameObject.layer, enemyLayers))
        {
            HandleContact(other);
        }
    }

    private void HandleContact(Collider2D other)
    {
        int layer = other.gameObject.layer;

        if (IsInMask(layer, enemyBulletLayers))
        {
            TakeDamage(1);
            ReleaseBullet(other.gameObject);
        }
        else if (IsInMask(layer, enemyLayers))
        {
            TakeDamage(1);
        }
    }

    private static bool IsInMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    private static void ReleaseBullet(GameObject bullet)
    {
        PooledObject pooled = bullet.GetComponentInParent<PooledObject>();
        if (pooled != null)
        {
            pooled.Release();
        }
        else
        {
            Destroy(bullet);
        }
    }

    private IEnumerator BlinkRoutine()
    {
        invincible = true;
        float elapsed = 0f;

        while (elapsed < invincibleDuration)
        {
            shipRenderer.enabled = !shipRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        shipRenderer.enabled = true;
        invincible = false;
    }

    private void HandleLivesChanged(int lives)
    {
        ApplyDamageSprite(lives);
    }

    private void ApplyDamageSprite(int lives)
    {
        if (lives >= 3)
        {
            shipRenderer.sprite = fullHealthSprite;
        }
        else if (lives == 2)
        {
            shipRenderer.sprite = slightDamageSprite;
        }
        else
        {
            shipRenderer.sprite = damagedSprite;
        }
    }

    private void HandleGameOver()
    {
        dead = true;
        invincible = false;
        StopAllCoroutines();

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        PlayerWeapon weapon = GetComponent<PlayerWeapon>();
        if (weapon != null)
        {
            weapon.enabled = false;
        }

        Collider2D hitbox = GetComponent<Collider2D>();
        if (hitbox != null)
        {
            hitbox.enabled = false;
        }

        foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>(true))
        {
            sprite.enabled = false;
        }
    }
}
