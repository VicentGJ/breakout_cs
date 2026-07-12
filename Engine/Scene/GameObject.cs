using Raylib_cs;

namespace Breakout;

sealed class GameObject(string name)
{
    private readonly Dictionary<Type, Component> _components = [];
    private readonly List<Component> _componentList = [];
    private Scene? _scene;

    public int Id { get; private set; }

    public string Name { get; } = name;

    public bool Active { get; set; } = true;

    public bool Visible { get; set; } = true;

    public int ZIndex { get; set; }

    public Color Modulate { get; set; } = Color.White;

    public Transform2D Transform2D { get; set; } = global::Breakout.Transform2D.Default;

    public IReadOnlyList<Component> Components => _componentList;

    public T Add<T>(T component) where T : Component
    {
        _components.Add(typeof(T), component);

        _componentList.Add(component);

        if (_scene is not null)
        {
            component.Attach(this, _scene);
        }

        return component;
    }

    public bool Has<T>() where T : Component
    {
        return _components.ContainsKey(typeof(T));
    }

    public T Get<T>() where T : Component
    {
        if (_components.TryGetValue(typeof(T), out Component? component))
        {
            return (T)component;
        }

        throw new InvalidOperationException($"GameObject '{Name}' missing component '{typeof(T).Name}'.");
    }

    public bool TryGet<T>(out T component) where T : Component
    {
        if (_components.TryGetValue(typeof(T), out Component? value))
        {
            component = (T)value;
            return true;
        }

        component = default!;
        return false;
    }

    internal void AttachToScene(Scene scene)
    {
        _scene = scene;

        foreach (Component component in new List<Component>(_componentList))
        {
            component.Attach(this, scene);
        }
    }

    internal void DetachFromScene()
    {
        _scene = null;

        foreach (Component component in new List<Component>(_componentList))
        {
            component.Detach();
        }
    }

    internal void AssignId(int id)
    {
        Id = id;
    }

}
