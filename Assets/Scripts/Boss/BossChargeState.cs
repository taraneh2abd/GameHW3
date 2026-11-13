using UnityEngine;

public class BossChargeState : BossState
{
    private enum Step
    {
        Telegraph,
        Dive,
        Return
    }

    private const float TelegraphTime = 0.9f;
    private const float ShakeAmount = 0.08f;
    private const float ShakeSpeed = 60f;
    private const float BottomClearance = 1.5f;

    private Step step;
    private float timer;
    private Vector3 anchor;
    private Vector2 target;

    public BossChargeState(Boss owner) : base(owner)
    {
    }

    public override void Enter()
    {
        step = Step.Telegraph;
        timer = TelegraphTime;
        anchor = boss.transform.position;
        boss.SetAlert(true);
    }

    public override void Exit()
    {
        boss.SetAlert(false);
    }

    public override void Tick(float dt)
    {
        switch (step)
        {
            case Step.Telegraph:
                TickTelegraph(dt);
                break;
            case Step.Dive:
                if (boss.MoveTowards(target, boss.ChargeSpeed, dt))
                {
                    step = Step.Return;
                }
                break;
            default:
                Vector2 home = new Vector2(anchor.x, boss.HoverY);
                if (boss.MoveTowards(home, boss.ChargeSpeed * 0.5f, dt))
                {
                    boss.NextAttack();
                }
                break;
        }
    }

    private void TickTelegraph(float dt)
    {
        timer -= dt;
        boss.transform.position = anchor + Vector3.right * (Mathf.Sin(Time.time * ShakeSpeed) * ShakeAmount);

        if (timer > 0f)
        {
            return;
        }

        boss.transform.position = anchor;
        boss.SetAlert(false);

        Vector2 player = boss.PlayerPosition;
        float x = Mathf.Clamp(player.x, boss.MinX, boss.MaxX);
        float y = Mathf.Max(player.y, boss.BottomY + BottomClearance);
        target = new Vector2(x, y);
        step = Step.Dive;
    }
}
