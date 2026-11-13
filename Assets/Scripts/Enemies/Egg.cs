using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Egg : MonoBehaviour, IDamageable
{
    [SerializeField] private float fallSpeed = 4f;
    [SerializeField] private float bottomMargin = 0.3f;
    [SerializeField] private float brokenDuration = 0.3f;
    [SerializeField] private Sprite intactSprite;
    [SerializeField] private Sprite brokenSprite;

    private SpriteRenderer spriteRenderer;
    private Collider2D hitbox;
    private bool falling;
    private bool released;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        hitbox = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        falling = true;
        released = false;
        spriteRenderer.sprite = intactSprite;
        hitbox.enabled = true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        if (!falling)
        {
            return;
        }

        transform.position += Vector3.down * (fallSpeed * Time.deltaTime);

        Camera cam = Camera.main;
        if (cam != null && transform.position.y <= cam.transform.position.y - cam.orthographicSize + bottomMargin)
        {
            StartCoroutine(BreakRoutine());
        }
    }

    public void TakeDamage(int amount)
    {
        if (falling)
        {
            Release();
        }
    }

    public void Release()
    {
        if (released)
        {
            return;
        }

        released = true;
        falling = false;

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

    private IEnumerator BreakRoutine()
    {
        falling = false;
        hitbox.enabled = false;
        spriteRenderer.sprite = brokenSprite;
        yield return new WaitForSeconds(brokenDuration);
        Release();
    }
}
