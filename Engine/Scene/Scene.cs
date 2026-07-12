using Raylib_cs;

namespace Breakout;

abstract class Scene
{
    private readonly List<GameObject> _objects = [];
    private readonly List<DispatchEntry> _dispatchEntries = [];
    private int _nextGameObjectId = 1;
    private bool _setupComplete;

    public EventBus Events { get; } = new();

    internal AudioSystem? Audio { get; private set; }

    public IReadOnlyList<GameObject> Objects => _objects;

    public Color BackgroundColor { get; set; } = Color.Black;

    public virtual void Setup()
    {
    }

    public virtual void Update(float dt)
    {
    }

    public virtual void PhysicsUpdate(float dt)
    {
    }

    public void AddObject(GameObject gameObject)
    {
        if (gameObject.Id == 0)
        {
            gameObject.AssignId(_nextGameObjectId++);
        }

        _objects.Add(gameObject);
        gameObject.AttachToScene(this);
    }

    public void RemoveObject(GameObject gameObject)
    {
        if (_objects.Remove(gameObject))
        {
            gameObject.DetachFromScene();
        }
    }

    internal void UpdateComponents(float dt)
    {
        DispatchComponents(dt, physicsPhase: false);
    }

    internal void PhysicsUpdateComponents(float dt)
    {
        DispatchComponents(dt, physicsPhase: true);
    }

    internal void SetupOnce()
    {
        if (_setupComplete)
        {
            return;
        }

        _setupComplete = true;
        Setup();
    }

    internal void BindAudio(AudioSystem audio)
    {
        Audio = audio;
    }

    internal void UnbindAudio(AudioSystem audio)
    {
        if (Audio == audio)
        {
            Audio = null;
        }
    }

    private void DispatchComponents(float dt, bool physicsPhase)
    {
        _dispatchEntries.Clear();

        foreach (GameObject gameObject in _objects)
        {
            if (!gameObject.Active)
            {
                continue;
            }

            foreach (Component component in gameObject.Components)
            {
                _dispatchEntries.Add(new DispatchEntry(gameObject, component));
            }
        }

        foreach (DispatchEntry entry in _dispatchEntries)
        {
            if (!entry.GameObject.Active || !entry.Component.Enabled)
            {
                continue;
            }

            if (!physicsPhase && entry.Component is IStartable startable && entry.Component.TryStart())
            {
                startable.Start();
            }

            if (physicsPhase)
            {
                if (entry.Component is IPhysicsUpdatable physicsUpdatable)
                {
                    physicsUpdatable.PhysicsUpdate(dt);
                }
            }
            else if (entry.Component is IUpdatable updatable)
            {
                updatable.Update(dt);
            }
        }
    }

    private readonly record struct DispatchEntry(GameObject GameObject, Component Component);
}
