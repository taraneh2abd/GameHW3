public class BossEnragedState : BossState
{
    private const float HoverSpeedScale = 1.3f;
    private const int ExtraProjectiles = 2;

    private float fireTimer;
    private float resummonTimer;

    public BossEnragedState(Boss owner) : base(owner)
    {
    }

    public override void Enter()
    {
        fireTimer = boss.FireInterval * boss.EnragedFireScale;
        resummonTimer = 0f;
        boss.SummonMinions();
    }

    public override void Tick(float dt)
    {
        boss.Hover(dt, HoverSpeedScale);

        fireTimer -= dt;
        if (fireTimer <= 0f)
        {
            boss.FireFan(boss.FanCount + ExtraProjectiles, boss.FanSpread, boss.ProjectileSpeed * 1.15f);
            fireTimer = boss.FireInterval * boss.EnragedFireScale;
        }

        if (boss.AliveMinions > 0)
        {
            resummonTimer = 0f;
            return;
        }

        resummonTimer += dt;
        if (resummonTimer >= boss.ResummonDelay)
        {
            boss.SummonMinions();
            resummonTimer = 0f;
        }
    }
}
