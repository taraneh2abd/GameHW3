using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class EngineExhaust : MonoBehaviour
{
    [SerializeField] private Transform ship;
    [SerializeField] private float idleRate = 22f;
    [SerializeField] private float boostRate = 70f;
    [SerializeField] private float smoothing = 12f;
    [SerializeField] private float climbSpeed = 0.5f;

    private ParticleSystem particles;
    private Vector3 lastPosition;
    private float rate;

    public float CurrentRate => rate;

    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();
        rate = idleRate;
    }

    private void OnEnable()
    {
        GameEvents.GameOver += HandleGameOver;
        lastPosition = ship.position;
        rate = idleRate;
        ApplyRate();

        if (!particles.isPlaying)
        {
            particles.Play();
        }
    }

    private void OnDisable()
    {
        GameEvents.GameOver -= HandleGameOver;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f)
        {
            return;
        }

        float climb = (ship.position.y - lastPosition.y) / dt;
        lastPosition = ship.position;

        float target = climb > climbSpeed ? boostRate : idleRate;
        rate = Mathf.Lerp(rate, target, 1f - Mathf.Exp(-smoothing * dt));
        ApplyRate();
    }

    private void ApplyRate()
    {
        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = rate;
    }

    private void HandleGameOver()
    {
        particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        enabled = false;
    }
}
