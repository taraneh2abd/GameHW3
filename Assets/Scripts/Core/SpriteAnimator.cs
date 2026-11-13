using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 12f;
    [SerializeField] private bool loop = true;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private int index;

    public bool Finished { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        timer = 0f;
        index = 0;
        Finished = false;
        ShowFrame();
    }

    private void Update()
    {
        if (Finished || frames == null || frames.Length < 2 || fps <= 0f)
        {
            return;
        }

        timer += Time.deltaTime;
        float frameTime = 1f / fps;

        while (timer >= frameTime)
        {
            timer -= frameTime;
            index++;

            if (index >= frames.Length)
            {
                if (loop)
                {
                    index = 0;
                }
                else
                {
                    index = frames.Length - 1;
                    Finished = true;
                    break;
                }
            }
        }

        ShowFrame();
    }

    public void Play(Sprite[] newFrames, float newFps, bool shouldLoop)
    {
        frames = newFrames;
        fps = newFps;
        loop = shouldLoop;
        OnEnable();
    }

    private void ShowFrame()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (frames != null && frames.Length > 0)
        {
            spriteRenderer.sprite = frames[index];
        }
    }
}
