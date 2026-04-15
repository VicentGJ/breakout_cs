namespace Breakout;

sealed class GameObject
{
    private readonly Dictionary<Type, object> _components = [];

    public GameObject(int id, string name)
    {
        Id = id;
        Name = name;
        IsActive = true;
        Transform2D = global::Breakout.Transform2D.Default;
    }

    public int Id { get; }

    public string Name { get; }

    public bool IsActive { get; set; }

    public Transform2D Transform2D { get; set; }

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
}
