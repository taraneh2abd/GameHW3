using UnityEngine;

[RequireComponent(typeof(ChickenMover))]
public class EggDropper : MonoBehaviour
{
    [SerializeField] private GameObject eggPrefab;
    [SerializeField] private Vector3 dropOffset = new Vector3(0f, -0.5f, 0f);
    [SerializeField] private float minIntervalFactor = 0.3f;
    [SerializeField] private float maxIntervalFactor = 1.7f;

    private ChickenMover mover;
    private float dropsPerSecond;
    private float nextDropTime;
    private bool scheduled;

    private void Awake()
    {
        mover = GetComponent<ChickenMover>();
    }

    public void SetDropRate(float rate)
    {
        dropsPerSecond = rate;
        scheduled = false;
    }

    private void Update()
    {
        if (mover.State != ChickenState.InFormation || dropsPerSecond <= 0f)
        {
            scheduled = false;
            return;
        }

        if (!scheduled)
        {
            ScheduleNextDrop();
            return;
        }

        if (Time.time >= nextDropTime)
        {
            DropEgg();
            ScheduleNextDrop();
        }
    }

    private void ScheduleNextDrop()
    {
        float meanInterval = 1f / dropsPerSecond;
        nextDropTime = Time.time + Random.Range(meanInterval * minIntervalFactor, meanInterval * maxIntervalFactor);
        scheduled = true;
    }

    private void DropEgg()
    {
        if (PoolManager.Instance == null || eggPrefab == null)
        {
            return;
        }

        PoolManager.Instance.Get(eggPrefab, transform.position + dropOffset, Quaternion.identity);
        GameEvents.RaiseEggDropped();
    }
}
