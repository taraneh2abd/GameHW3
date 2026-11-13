using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [SerializeField] private float duration = 0.08f;

    private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

    private SpriteRenderer[] renderers;
    private MaterialPropertyBlock block;
    private float remaining;

    public float Amount { get; private set; }
    public SpriteRenderer[] Renderers => renderers;

    private void Awake()
    {
        Collect();
    }

    private void OnDisable()
    {
        remaining = 0f;
        Apply(0f);
    }

    private void Update()
    {
        if (remaining <= 0f)
        {
            return;
        }

        remaining -= Time.unscaledDeltaTime;

        if (remaining <= 0f)
        {
            Apply(0f);
        }
    }

    public void Flash()
    {
        remaining = duration;
        Apply(1f);
    }

    private void Collect()
    {
        if (renderers == null)
        {
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
            block = new MaterialPropertyBlock();
        }
    }

    private void Apply(float amount)
    {
        Collect();
        Amount = amount;

        foreach (SpriteRenderer sprite in renderers)
        {
            if (sprite == null)
            {
                continue;
            }

            sprite.GetPropertyBlock(block);
            block.SetFloat(FlashAmountId, amount);
            sprite.SetPropertyBlock(block);
        }
    }
}
