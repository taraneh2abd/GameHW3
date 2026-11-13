using UnityEngine;

[RequireComponent(typeof(Chicken), typeof(ChickenMover))]
public class ChickenDiver : MonoBehaviour
{
    [SerializeField] private float checkInterval = 3f;
    [SerializeField] private float baseDiveSpeed = 7f;
    [SerializeField] private float diveSpeedMultiplier = 1f;

    private Chicken chicken;
    private ChickenMover mover;
    private WaveSpawner owner;
    private float diveChance;
    private float speedScale = 1f;
    private float nextCheckTime;
    private bool diving;

    public bool IsDiving => diving;

    private void Awake()
    {
        chicken = GetComponent<Chicken>();
        mover = GetComponent<ChickenMover>();
        nextCheckTime = Time.time + Random.Range(0.5f, checkInterval);
    }

    private void OnDestroy()
    {
        EndDive();
    }

    public void Configure(WaveSpawner spawner, float chance, float difficultyMultiplier)
    {
        owner = spawner;
        diveChance = Mathf.Clamp01(chance);
        speedScale = difficultyMultiplier;
    }

    private void Update()
    {
        if (diving)
        {
            if (mover.State == ChickenState.InFormation)
            {
                EndDive();
            }

            return;
        }

        if (mover.State != ChickenState.InFormation || !chicken.IsAlive || diveChance <= 0f || Time.time < nextCheckTime)
        {
            return;
        }

        nextCheckTime = Time.time + Random.Range(checkInterval * 0.7f, checkInterval * 1.3f);

        if (Random.value < diveChance)
        {
            TryDive();
        }
    }

    private void TryDive()
    {
        PlayerHealth player = FindAnyObjectByType<PlayerHealth>();
        Camera cam = Camera.main;

        if (player == null || cam == null || owner == null || !owner.TryAcquireDive())
        {
            return;
        }

        Vector3 start = transform.position;
        Vector2 slot = mover.Slot;
        Vector3[] divePath = EntryPath.BuildDive(start, player.transform.position, cam).ToArray();
        Vector3[] returnPath = EntryPath.Build(EntryPattern.TopLoop, slot, cam).ToArray();

        diving = true;
        mover.BeginDive(divePath, returnPath, baseDiveSpeed * diveSpeedMultiplier * speedScale);
    }

    private void EndDive()
    {
        if (!diving)
        {
            return;
        }

        diving = false;
        nextCheckTime = Time.time + Random.Range(checkInterval * 0.7f, checkInterval * 1.3f);

        if (owner != null)
        {
            owner.ReleaseDive();
        }
    }
}
