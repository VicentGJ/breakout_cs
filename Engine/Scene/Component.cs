namespace Breakout;

abstract class Component
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

    internal bool TryStart()
    {
        if (_started)
        {
            return false;
        }

        _started = true;
        return true;
    }

    protected virtual void OnAttach()
    {
    }

    protected virtual void OnDetach()
    {
    }

}

interface IStartable
{
    void Start();
}

interface IUpdatable
{
    void Update(float dt);
}

interface IPhysicsUpdatable
{
    void PhysicsUpdate(float dt);
}
