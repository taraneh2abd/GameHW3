using UnityEngine;

public class BossEnterState : BossState
{
    public BossEnterState(Boss owner) : base(owner)
    {
    }

    public override void Tick(float dt)
    {
        Vector2 target = new Vector2(boss.transform.position.x, boss.HoverY);

        if (boss.MoveTowards(target, boss.EnterSpeed, dt))
        {
            boss.CompleteEntry();
        }
    }
}
