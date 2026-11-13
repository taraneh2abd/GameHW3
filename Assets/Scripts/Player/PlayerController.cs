using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private SpriteRenderer shipRenderer;
    [SerializeField] private float speed = 7f;

    private void OnEnable()
    {
        inputReader.EnableGameplay();
    }

    private void Update()
    {
        Vector2 input = Vector2.ClampMagnitude(inputReader.Move, 1f);
        Vector3 position = transform.position + (Vector3)(input * (speed * Time.deltaTime));
        transform.position = ClampToView(position);
    }

    private Vector3 ClampToView(Vector3 position)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return position;
        }

        Vector3 extents = shipRenderer.sprite.bounds.extents;
        Vector3 scale = shipRenderer.transform.lossyScale;
        float halfWidth = extents.x * Mathf.Abs(scale.x);
        float halfHeight = extents.y * Mathf.Abs(scale.y);

        float viewHalfHeight = cam.orthographicSize;
        float viewHalfWidth = viewHalfHeight * cam.aspect;
        Vector3 center = cam.transform.position;

        position.x = Mathf.Clamp(position.x, center.x - viewHalfWidth + halfWidth, center.x + viewHalfWidth - halfWidth);
        position.y = Mathf.Clamp(position.y, center.y - viewHalfHeight + halfHeight, center.y + viewHalfHeight - halfHeight);
        return position;
    }
}
