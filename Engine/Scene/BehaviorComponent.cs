namespace Breakout;

abstract class BehaviorComponent : IComponent
{
    private bool _started;

    public GameObject Owner { get; private set; } = null!;

    public Scene Scene { get; private set; } = null!;

    public bool Enabled { get; set; } = true;

    internal void Attach(GameObject owner, Scene scene)
    {
        Owner = owner;
        Scene = scene;
        OnAttach();
    }

    internal void Detach()
    {
        OnDetach();
        _started = false;
        Owner = null!;
        Scene = null!;
    }

    internal void Start()
    {
        if (_started)
        {
            return;
        }

        _started = true;
        OnStart();
    }

    protected virtual void OnAttach()
    {
    }

    protected virtual void OnDetach()
    {
    }

    protected virtual void OnStart()
    {
    }

    public virtual void Update(float dt)
    {
    }

    public virtual void PhysicsUpdate(float dt)
    {
    }
}
