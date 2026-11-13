using System.Collections.Generic;
using UnityEngine;

public static class Formation
{
    public const float SwayAmplitude = 0.6f;
    public const float SwaySpeed = 0.8f;
    public const float SideMargin = 1f;
    public const float TopMargin = 0.8f;
    public const float RegionFraction = 0.45f;

    private const float GridStepX = 1.5f;
    private const float GridStepY = 1.15f;
    private const float DenseStepX = 1.1f;
    private const float DenseStepY = 0.9f;
    private const float VStepX = 1.2f;
    private const float VStepY = 0.3f;
    private const float VLayerGap = 1.05f;

    public static float SwayOffset => Mathf.Sin(Time.time * SwaySpeed) * SwayAmplitude;

    public static List<Vector2> GetSlots(FormationType type, int count, Camera cam, float spacingScale = 1f)
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        Vector2 center = cam.transform.position;

        float usableHalfWidth = halfWidth - SideMargin - SwayAmplitude;
        float topY = center.y + halfHeight - TopMargin;
        float bottomY = center.y + halfHeight - RegionFraction * 2f * halfHeight;

        List<Vector2> slots;

        switch (type)
        {
            case FormationType.VShape:
                slots = VSlots(count, usableHalfWidth, spacingScale);
                break;
            case FormationType.Dense:
                slots = RowSlots(count, usableHalfWidth, DenseStepX * spacingScale, DenseStepY * spacingScale, true);
                break;
            default:
                slots = RowSlots(count, usableHalfWidth, GridStepX * spacingScale, GridStepY * spacingScale, false);
                break;
        }

        FitVertically(slots, topY, bottomY);

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i] = new Vector2(center.x + slots[i].x, slots[i].y);
        }

        return slots;
    }

    private static List<Vector2> RowSlots(int count, float usableHalfWidth, float stepX, float stepY, bool stagger)
    {
        float reservedForStagger = stagger ? stepX * 0.5f : 0f;
        int maxColumns = Mathf.Max(1, Mathf.FloorToInt((usableHalfWidth * 2f - reservedForStagger) / stepX) + 1);
        int rows = Mathf.CeilToInt((float)count / maxColumns);
        int columns = Mathf.CeilToInt((float)count / rows);
        List<Vector2> slots = new List<Vector2>(count);

        for (int r = 0; r < rows && slots.Count < count; r++)
        {
            int inRow = Mathf.Min(columns, count - slots.Count);
            float startX = -(inRow - 1) * stepX * 0.5f;

            if (stagger)
            {
                startX = -(columns - 1) * stepX * 0.5f - stepX * 0.25f + (r % 2) * stepX * 0.5f;
            }

            for (int c = 0; c < inRow; c++)
            {
                slots.Add(new Vector2(startX + c * stepX, -r * stepY));
            }
        }

        return slots;
    }

    private static List<Vector2> VSlots(int count, float usableHalfWidth, float spacingScale)
    {
        float stepX = VStepX * spacingScale;
        float stepY = VStepY * spacingScale;
        float layerGap = VLayerGap * spacingScale;

        int maxArm = Mathf.Max(1, Mathf.FloorToInt(usableHalfWidth / stepX));
        int perLayerMax = 1 + 2 * maxArm;
        int layers = Mathf.CeilToInt((float)count / perLayerMax);
        int perLayer = Mathf.CeilToInt((float)count / layers);
        List<Vector2> slots = new List<Vector2>(count);

        for (int layer = 0; layer < layers; layer++)
        {
            int inLayer = Mathf.Min(perLayer, count - slots.Count);
            int arm = perLayer / 2;
            float baseY = -layer * layerGap - arm * stepY;

            for (int i = 0; i < inLayer; i++)
            {
                int k = (i + 1) / 2;
                float side = i % 2 == 1 ? -1f : 1f;
                slots.Add(new Vector2(side * k * stepX, baseY + k * stepY));
            }
        }

        return slots;
    }

    private static void FitVertically(List<Vector2> slots, float topY, float bottomY)
    {
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (Vector2 slot in slots)
        {
            minY = Mathf.Min(minY, slot.y);
            maxY = Mathf.Max(maxY, slot.y);
        }

        float available = topY - bottomY;
        float height = maxY - minY;
        float factor = height > available ? available / height : 1f;

        for (int i = 0; i < slots.Count; i++)
        {
            float y = topY + (slots[i].y - maxY) * factor;
            slots[i] = new Vector2(slots[i].x, y);
        }
    }
}
