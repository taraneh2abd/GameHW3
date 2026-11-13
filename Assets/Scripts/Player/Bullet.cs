using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float viewMargin = 1f;

    private int damage;
    private float speed;
    private bool released;

    private void OnEnable()
    {
        damage = 1;
        speed = 0f;
        released = false;
    }

    public void Launch(int bulletDamage, float bulletSpeed)
    {
        damage = bulletDamage;
        speed = bulletSpeed;
    }

    private void Update()
    {
        transform.position += transform.up * (speed * Time.deltaTime);

        if (IsOutsideView())
        {
            Release();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (released)
        {
            return;
        }

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null)
        {
            return;
        }

        target.TakeDamage(damage);
        Release();
    }

    private bool IsOutsideView()
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

        return position.x < center.x - halfWidth - viewMargin
            || position.x > center.x + halfWidth + viewMargin
            || position.y < center.y - halfHeight - viewMargin
            || position.y > center.y + halfHeight + viewMargin;
    }

    private void Release()
    {
        if (released)
        {
            return;
        }

        released = true;

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
}
