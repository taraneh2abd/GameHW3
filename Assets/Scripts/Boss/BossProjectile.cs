using UnityEngine;

[RequireComponent(typeof(SpriteAnimator))]
public class BossProjectile : MonoBehaviour, IScreenClearTarget
{
    [SerializeField] private float rotationOffset;
    [SerializeField] private float viewMargin = 1f;

    private Vector3 direction;
    private float speed;
    private bool released;

    private void OnEnable()
    {
        direction = Vector3.down;
        speed = 0f;
        released = false;
    }

    public void Launch(Vector2 moveDirection, float moveSpeed)
    {
        direction = moveDirection.normalized;
        speed = moveSpeed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f + rotationOffset;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Update()
    {
        transform.position += direction * (speed * Time.deltaTime);

        if (IsOutsideView())
        {
            Release();
        }
    }

    public void OnScreenClear()
    {
        Release();
    }

    public void Release()
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
}
