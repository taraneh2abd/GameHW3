public abstract class BossState
{
    protected readonly Boss boss;

    protected BossState(Boss owner)
    {
        boss = owner;
    }

    public virtual void Enter()
    {
    }

    public abstract void Tick(float dt);

    public virtual void Exit()
    {
    }
}
