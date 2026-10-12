using Raylib_cs;

namespace Breakout;

abstract class World
{
    private readonly List<GameObject> _objects = [];
    private readonly List<GameObject> _dispatchList = [];
    private int _nextGameObjectId = 1;
    private bool _setupComplete;
    private Scene? _pendingScene;

    public EventBus Events { get; } = new();

    public Scene? CurrentScene { get; private set; }

    internal AudioSystem? Audio { get; private set; }

    // Flat list of every object in the world, children included (parents come before their children).
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

    // Adds a root object together with its whole subtree.
    public void AddObject(GameObject gameObject)
    {
        if (gameObject.Parent is not null || gameObject.World is not null)
        {
            throw new InvalidOperationException($"GameObject '{gameObject.Name}' already has a parent or belongs to a world.");
        }

        Register(gameObject);
        CurrentScene?.Roots.Add(gameObject);
    }

    // Removes an object together with its whole subtree.
    public void RemoveObject(GameObject gameObject)
    {
        if (gameObject.World != this)
        {
            return;
        }

        gameObject.Unlink();
        Unregister(gameObject);
        CurrentScene?.Roots.Remove(gameObject);
    }

    // Request only: the switch runs at the end of the frame (FlushSceneChange), never mid-callback.
    // A later request replaces an earlier one. Objects added during Setup persist across scenes.
    public void LoadScene(Scene scene)
    {
        _pendingScene = scene;
    }

    // First object of type T, or null. Look up in Start, not in OnAttach.
    public T? Find<T>() where T : GameObject
    {
        foreach (GameObject gameObject in _objects)
        {
            if (gameObject is T match)
            {
                return match;
            }
        }

        return null;
    }

    // ponytail: linear scan, add a per-type index if scenes get big.
    // Do not add or remove objects while enumerating; copy with ToList() first.
    public IEnumerable<T> FindAll<T>() where T : GameObject
    {
        foreach (GameObject gameObject in _objects)
        {
            if (gameObject is T match)
            {
                yield return match;
            }
        }
    }

    internal void Register(GameObject gameObject)
    {
        if (gameObject.Id == 0)
        {
            gameObject.AssignId(_nextGameObjectId++);
        }

        _objects.Add(gameObject);
        gameObject.AttachToWorld(this);

        for (int i = 0; i < gameObject.Children.Count; i++)
        {
            GameObject child = gameObject.Children[i];

            if (child.World is null)
            {
                Register(child);
            }
        }
    }

    internal void UpdateObjects(float dt)
    {
        Snapshot();

        foreach (GameObject gameObject in _dispatchList)
        {
            if (IsLive(gameObject) && gameObject.TryStart())
            {
                gameObject.Start();
            }
        }

        foreach (GameObject gameObject in _dispatchList)
        {
            if (IsLive(gameObject))
            {
                gameObject.Update(dt);
            }
        }
    }

    internal void PhysicsUpdateObjects(float dt)
    {
        Snapshot();

        foreach (GameObject gameObject in _dispatchList)
        {
            if (IsLive(gameObject))
            {
                gameObject.PhysicsUpdate(dt);
            }
        }
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

    internal void FlushSceneChange()
    {
        if (_pendingScene is not { } next)
        {
            return;
        }

        _pendingScene = null;
        string? previousPath = CurrentScene?.Path;

        if (CurrentScene is not null)
        {
            Events.Publish(new SceneUnloadingEvent(CurrentScene.Path));
            CurrentScene.Unload(this);
        }

        CurrentScene = next;
        next.Load();
        Events.Publish(new SceneLoadedEvent(next.Path, previousPath));
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

    private void Unregister(GameObject gameObject)
    {
        foreach (GameObject child in gameObject.Children)
        {
            if (child.World == this)
            {
                Unregister(child);
            }
        }

        _objects.Remove(gameObject);
        gameObject.DetachFromWorld();
    }

    private void Snapshot()
    {
        _dispatchList.Clear();

        foreach (GameObject gameObject in _objects)
        {
            if (gameObject.ActiveInHierarchy)
            {
                _dispatchList.Add(gameObject);
            }
        }
    }

    private bool IsLive(GameObject gameObject)
    {
        return gameObject.World == this && gameObject.ActiveInHierarchy;
    }
}
