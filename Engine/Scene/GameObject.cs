namespace Breakout;

sealed class GameObject
{
    private readonly Dictionary<Type, object> _components = [];
    private readonly List<BehaviorComponent> _behaviors = [];
    private Scene? _scene;

    public GameObject(string name)
    {
        Name = name;
    }

    public int Id { get; private set; }

    public string Name { get; }

    public bool Active { get; set; } = true;

    public Transform2D Transform2D { get; set; } = global::Breakout.Transform2D.Default;

    public IReadOnlyList<BehaviorComponent> Behaviors => _behaviors;

    public void Add<T>(T component) where T : struct, IComponent
    {
        _components.Add(typeof(T), component);
    }

    public void Set<T>(T component) where T : struct, IComponent
    {
        _components[typeof(T)] = component;
    }

    public bool Has<T>() where T : struct, IComponent
    {
        return _components.ContainsKey(typeof(T));
    }

    public T Get<T>() where T : struct, IComponent
    {
        if (_components.TryGetValue(typeof(T), out object? component))
        {
            return (T)component;
        }

        throw new InvalidOperationException($"GameObject '{Name}' missing component '{typeof(T).Name}'.");
    }

    public bool TryGet<T>(out T component) where T : struct, IComponent
    {
        if (_components.TryGetValue(typeof(T), out object? value))
        {
            component = (T)value;
            return true;
        }

        component = default;
        return false;
    }

    public T AddBehavior<T>(T behavior) where T : BehaviorComponent
    {
        _behaviors.Add(behavior);

        if (_scene is not null)
        {
            behavior.Attach(this, _scene);
        }

        return behavior;
    }

    public T GetBehavior<T>() where T : BehaviorComponent
    {
        if (TryGetBehavior(out T? behavior) && behavior is not null)
        {
            return behavior;
        }

        throw new InvalidOperationException($"GameObject '{Name}' missing behavior '{typeof(T).Name}'.");
    }

    public bool TryGetBehavior<T>(out T? behavior) where T : BehaviorComponent
    {
        foreach (BehaviorComponent candidate in _behaviors)
        {
            if (candidate is T match)
            {
                behavior = match;
                return true;
            }
        }

        behavior = null;
        return false;
    }

    internal void AttachToScene(Scene scene)
    {
        _scene = scene;

        foreach (BehaviorComponent behavior in _behaviors)
        {
            behavior.Attach(this, scene);
        }
    }

    internal void DetachFromScene()
    {
        foreach (BehaviorComponent behavior in _behaviors)
        {
            behavior.Detach();
        }

        _scene = null;
    }

    internal void AssignId(int id)
    {
        Id = id;
    }
}
