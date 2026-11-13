using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public abstract class Pickup : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float bobAmount = 0.15f;
    [SerializeField] private float bobSpeed = 4f;
    [SerializeField] private float tiltAngle = 10f;
    [SerializeField] private float bottomMargin = 0.6f;

    private float baseX;
    private float phase;
    private bool collected;

    private void OnEnable()
    {
        collected = false;
        baseX = transform.position.x;
        phase = Random.Range(0f, Mathf.PI * 2f);
        transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        float wave = Mathf.Sin(Time.time * bobSpeed + phase);
        Vector3 position = transform.position;
        position.y -= fallSpeed * Time.deltaTime;
        position.x = baseX + wave * bobAmount;
        transform.position = position;
        transform.rotation = Quaternion.Euler(0f, 0f, wave * tiltAngle);

        Camera cam = Camera.main;
        if (cam != null && position.y < cam.transform.position.y - cam.orthographicSize - bottomMargin)
        {
            Release();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || other.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            return;
        }

        collected = true;
        Collect();
        GameEvents.RaisePickupCollected();
        Release();
    }

    public void Release()
    {
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

    protected abstract void Collect();
}
