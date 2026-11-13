using UnityEngine;

public class BossSweepState : BossState
{
    private const float FirstRowDelay = 0.3f;
    private const float GapJitter = 3f;

    private float targetX;
    private float rowTimer;

    public BossSweepState(Boss owner) : base(owner)
    {
    }

    public override void Enter()
    {
        targetX = boss.transform.position.x < boss.CenterX ? boss.MaxX : boss.MinX;
        rowTimer = FirstRowDelay;
    }

    public override void Tick(float dt)
    {
        Vector2 target = new Vector2(targetX, boss.HoverY);

        rowTimer -= dt;
        if (rowTimer <= 0f)
        {
            float gapX = boss.PlayerPosition.x + Random.Range(-GapJitter, GapJitter);
            boss.FireCurtain(gapX, boss.CurtainSpacing, boss.ProjectileSpeed * 0.8f);
            rowTimer = boss.CurtainInterval;
        }

        if (boss.MoveTowards(target, boss.SweepSpeed, dt))
        {
            boss.NextAttack();
        }
    }
}
