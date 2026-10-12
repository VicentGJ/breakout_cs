using System.Numerics;
using Raylib_cs;

namespace Breakout;

class GameObject
{
    private readonly List<GameObject> _children = [];
    private bool _started;

    public GameObject()
    {
        Name = GetType().Name;
    }

    public int Id { get; private set; }

    public string Name { get; set; }

    public bool Active { get; set; } = true;

    public bool Visible { get; set; } = true;

    public int ZIndex { get; set; }

    public Color Modulate { get; set; } = Color.White;

    // Local transform: relative to Parent. For root objects local == world.
    public Transform2D Transform2D { get; set; } = global::Breakout.Transform2D.Default;

    public World World { get; private set; } = null!;

    public GameObject? Parent { get; private set; }

    public IReadOnlyList<GameObject> Children => _children;

    // Own flag AND every ancestor's flag.
    public bool ActiveInHierarchy => Active && (Parent?.ActiveInHierarchy ?? true);

    public bool VisibleInHierarchy => Visible && (Parent?.VisibleInHierarchy ?? true);

    public T AddChild<T>(T child) where T : GameObject
    {
        if (child.Parent is not null || child.World is not null)
        {
            throw new InvalidOperationException($"GameObject '{child.Name}' already has a parent or belongs to a world.");
        }

        for (GameObject? ancestor = this; ancestor is not null; ancestor = ancestor.Parent)
        {
            if (ancestor == child)
            {
                throw new InvalidOperationException($"GameObject '{child.Name}' cannot be its own ancestor.");
            }
        }

        child.Parent = this;
        _children.Add(child);

        if (World is not null)
        {
            World.Register(child);
        }

        return child;
    }

    public Transform2D GetWorldTransform()
    {
        if (Parent is null)
        {
            return Transform2D;
        }

        Transform2D parent = Parent.GetWorldTransform();
        Vector2 scaled = Transform2D.Position * parent.Scale;
        float cos = MathF.Cos(parent.Rotation);
        float sin = MathF.Sin(parent.Rotation);

        return new Transform2D
        {
            Position = parent.Position + new Vector2(scaled.X * cos - scaled.Y * sin, scaled.X * sin + scaled.Y * cos),
            Rotation = parent.Rotation + Transform2D.Rotation,
            Scale = parent.Scale * Transform2D.Scale
        };
    }

    // Lifecycle hooks. OnAttach/OnDetach touch only this object (and world events).
    // Start runs once at the beginning of the first update after the whole world is loaded,
    // so it may look at the rest of the world with World.Find.
    protected virtual void OnAttach()
    {
    }

    protected virtual void OnDetach()
    {
    }

    public virtual void Start()
    {
    }

    public virtual void Update(float dt)
    {
    }

    public virtual void PhysicsUpdate(float dt)
    {
    }

    internal virtual void Draw(RenderSystem renderer, in Transform2D world)
    {
    }

    internal void AttachToWorld(World world)
    {
        World = world;
        OnAttach();
    }

    internal void DetachFromWorld()
    {
        OnDetach();
        _started = false;
        World = null!;
    }

    internal void Unlink()
    {
        Parent?._children.Remove(this);
        Parent = null;
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

    internal void AssignId(int id)
    {
        Id = id;
    }
}
