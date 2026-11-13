using System.Collections.Generic;
using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private bool rotate90;
    [SerializeField] private bool tileHorizontally = true;
    [SerializeField] private float columnShift = 0.382f;
    [SerializeField] private Color tint = Color.white;
    [SerializeField] private float speed = 1f;
    [SerializeField] private int sortingOrder = -100;
    [SerializeField] private float spriteScale = 1f;
    [SerializeField] private float gap;
    [SerializeField] private float xOffset;
    [SerializeField] private float startOffset;
    [SerializeField] private int scatterCount;
    [SerializeField] private float cellHeight = 30f;
    [SerializeField] private Vector2 scatterScale = new Vector2(0.5f, 1f);
    [SerializeField] private int seed = 1;
    [SerializeField] private float coverMargin = 1.5f;
    [SerializeField] private float coverAspect = 2.4f;

    private readonly List<Transform> rows = new List<Transform>();
    private readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>();
    private float cell;
    private float bottom;
    private float scroll;

    public float Speed => speed;
    public float Scroll => scroll;
    public float CellHeight => cell;
    public bool CoversView => scatterCount == 0 && gap <= 0f && tileHorizontally;
    public int SortingOrder => sortingOrder;
    public Sprite[] Sprites => sprites;
    public IReadOnlyList<SpriteRenderer> Renderers => renderers;

    private void Awake()
    {
        Build();
    }

    private void Update()
    {
        Advance(speed * Time.deltaTime);
    }

    public Bounds GetCoveredBounds()
    {
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        bool first = true;

        foreach (SpriteRenderer sprite in renderers)
        {
            if (first)
            {
                bounds = sprite.bounds;
                first = false;
            }
            else
            {
                bounds.Encapsulate(sprite.bounds);
            }
        }

        return bounds;
    }

    private void Advance(float distance)
    {
        scroll = Mathf.Repeat(scroll + distance, cell);
        PlaceRows();
    }

    private void PlaceRows()
    {
        for (int i = 0; i < rows.Count; i++)
        {
            rows[i].localPosition = new Vector3(0f, bottom + cell * (i + 0.5f) - scroll, 0f);
        }
    }

    private void Build()
    {
        if (sprites == null || sprites.Length == 0 || sprites[0] == null)
        {
            return;
        }

        Camera cam = Camera.main;
        float halfHeight = cam != null ? cam.orthographicSize : 5f;
        float aspect = cam != null ? Mathf.Max(cam.aspect, coverAspect) : coverAspect;
        float coverHeight = (halfHeight + coverMargin) * 2f;
        float coverWidth = (halfHeight * aspect + coverMargin) * 2f;
        bottom = -(halfHeight + coverMargin);

        Vector2 size = sprites[0].bounds.size * spriteScale;
        Vector2 footprint = rotate90 ? new Vector2(size.y, size.x) : size;

        if (scatterCount > 0)
        {
            cell = cellHeight;
        }
        else
        {
            cell = footprint.y + gap;
        }

        bool staggered = scatterCount == 0 && tileHorizontally && columnShift > 0f;
        int rowCount = Mathf.CeilToInt(coverHeight / cell) + (staggered ? 2 : 1);

        for (int r = 0; r < rowCount; r++)
        {
            GameObject row = new GameObject("Row" + r);
            row.transform.SetParent(transform, false);
            rows.Add(row.transform);

            if (scatterCount > 0)
            {
                BuildScatterRow(row.transform, coverWidth);
            }
            else
            {
                BuildTiledRow(row.transform, footprint.x, coverWidth);
            }
        }

        scroll = Mathf.Repeat(startOffset, cell);
        PlaceRows();
    }

    private void BuildTiledRow(Transform row, float width, float coverWidth)
    {
        int columns = tileHorizontally ? Mathf.CeilToInt(coverWidth / width) + 1 : 1;

        for (int c = 0; c < columns; c++)
        {
            float x = xOffset + (c - (columns - 1) * 0.5f) * width;
            float y = columns > 1 ? -Mathf.Repeat(c * columnShift, 1f) * cell : 0f;
            CreateSprite(row, sprites[0], new Vector3(x, y, 0f), spriteScale, rotate90 ? 90f : 0f);
        }
    }

    private void BuildScatterRow(Transform row, float coverWidth)
    {
        System.Random random = new System.Random(seed);

        for (int i = 0; i < scatterCount; i++)
        {
            Sprite sprite = sprites[random.Next(sprites.Length)];
            float x = xOffset + ((float)random.NextDouble() - 0.5f) * coverWidth;
            float y = ((float)random.NextDouble() - 0.5f) * cell;
            float scale = Mathf.Lerp(scatterScale.x, scatterScale.y, (float)random.NextDouble()) * spriteScale;
            float angle = (float)random.NextDouble() * 360f;
            CreateSprite(row, sprite, new Vector3(x, y, 0f), scale, angle);
        }
    }

    private SpriteRenderer CreateSprite(Transform parent, Sprite sprite, Vector3 localPosition, float scale, float angle)
    {
        GameObject tile = new GameObject(sprite.name);
        tile.transform.SetParent(parent, false);
        tile.transform.localPosition = localPosition;
        tile.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        tile.transform.localScale = Vector3.one * scale;

        SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = tint;
        renderer.sortingOrder = sortingOrder;
        renderers.Add(renderer);
        return renderer;
    }
}
