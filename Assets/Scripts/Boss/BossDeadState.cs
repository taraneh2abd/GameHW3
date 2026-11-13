public class BossDeadState : BossState
{
    private const float FinalBurstPoint = 0.8f;
    private const float FinalBurstScale = 3f;

    private float elapsed;
    private float explosionTimer;
    private bool burst;

    public BossDeadState(Boss owner) : base(owner)
    {
    }

    public override void Enter()
    {
        elapsed = 0f;
        explosionTimer = 0f;
        burst = false;
        boss.BeginDeath();
    }

    public override void Tick(float dt)
    {
        elapsed += dt;
        explosionTimer -= dt;

        if (!burst && explosionTimer <= 0f)
        {
            boss.SpawnExplosion(1f);
            explosionTimer = boss.ExplosionInterval;
        }

        if (!burst && elapsed >= boss.DeathDuration * FinalBurstPoint)
        {
            burst = true;
            boss.SpawnExplosion(FinalBurstScale);
            boss.HideBody();
        }

        if (elapsed >= boss.DeathDuration)
        {
            boss.FinishDeath();
        }
    }
}
