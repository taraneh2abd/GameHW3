public class BossBarrageState : BossState
{
    private const float FirstShotDelay = 0.8f;

    private float timer;

    public BossBarrageState(Boss owner) : base(owner)
    {
    }

    public override void Enter()
    {
        timer = FirstShotDelay;
    }

    public override void Tick(float dt)
    {
        boss.Hover(dt, 1f);

        timer -= dt;
        if (timer > 0f)
        {
            return;
        }

        boss.FireFan(boss.FanCount, boss.FanSpread, boss.ProjectileSpeed);
        timer = boss.FireInterval;
    }
}
