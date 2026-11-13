public class BossStateMachine
{
    public BossState Current { get; private set; }

    public void ChangeState(BossState next)
    {
        if (Current != null)
        {
            Current.Exit();
        }

        Current = next;

        if (Current != null)
        {
            Current.Enter();
        }
    }

    public void Tick(float dt)
    {
        if (Current != null)
        {
            Current.Tick(dt);
        }
    }
}
