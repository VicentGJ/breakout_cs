using System.Numerics;
using Raylib_cs;

namespace Breakout;

internal enum GamePhase
{
    Serving,
    Playing,
    Won,
    Lost
}

internal interface IComponent
{
}

internal struct Transform2DComponent : IComponent
{
    public Vector2 Position;
    public float Rotation;
    public Vector2 Scale;

    public static Transform2DComponent Default => new()
    {
        Scale = Vector2.One
    };
}

internal struct CanvasItemComponent : IComponent
{
    public bool Visible;
    public int ZIndex;
    public Color Modulate;
}

internal struct PaddleComponent : IComponent
{
    public float Speed;
}

internal struct BallComponent : IComponent
{
    public Vector2 PreviousPosition;
    public Vector2 Velocity;
}

internal struct BrickComponent : IComponent
{
    public int Points;
}

internal struct RectangleRendererComponent : IComponent
{
    public Vector2 Size;
    public Vector2 Offset = Vector2.Zero;
    public Vector2 Origin = Vector2.Zero;
    public Color FillColor;

    public Color? StrokeColor = null;

    public RectangleRendererComponent()
    {
    }
}

internal struct CircleRendererComponent : IComponent
{
    public float Radius;
    public Vector2 Offset = Vector2.Zero;
    public Color FillColor;

    public Color? StrokeColor = null;

    public CircleRendererComponent()
    {
    }
}

internal struct TextRendererComponent : IComponent
{
    public string Text = string.Empty;
    public int FontSize = 16;
    public Vector2 Offset = Vector2.Zero;
    public Color Color = Color.White;

    public TextRendererComponent()
    {
    }
}

internal sealed class GameObject
{
    private readonly Dictionary<Type, object> _components = [];

    public GameObject(int id, string name)
    {
        Id = id;
        Name = name;
        IsActive = true;
    }

    public int Id { get; }

    public string Name { get; }

    public bool IsActive { get; set; }

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
