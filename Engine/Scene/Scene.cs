namespace Breakout;

abstract class Scene
{
    private readonly List<GameObject> _objects = [];
    private int _nextGameObjectId = 1;
    private bool _setupComplete;

    public EventBus Events { get; } = new();

    public IReadOnlyList<GameObject> Objects => _objects;

    public virtual void Setup()
    {
    }

    public virtual void Update(float dt)
    {
    }

    public virtual void PhysicsUpdate(float dt)
    {
    }

    public virtual void Draw()
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

    internal void UpdateBehaviors(float dt)
    {
        DispatchBehaviors(dt, physicsPhase: false);
    }

    internal void PhysicsUpdateBehaviors(float dt)
    {
        DispatchBehaviors(dt, physicsPhase: true);
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

    private void DispatchBehaviors(float dt, bool physicsPhase)
    {
        for (int objectIndex = 0; objectIndex < _objects.Count; objectIndex++)
        {
            GameObject gameObject = _objects[objectIndex];

            if (!gameObject.Active)
            {
                continue;
            }

            IReadOnlyList<BehaviorComponent> behaviors = gameObject.Behaviors;

            for (int behaviorIndex = 0; behaviorIndex < behaviors.Count; behaviorIndex++)
            {
                BehaviorComponent behavior = behaviors[behaviorIndex];

                if (!behavior.Enabled)
                {
                    continue;
                }

                behavior.Start();

                if (physicsPhase)
                {
                    behavior.PhysicsUpdate(dt);
                }
                else
                {
                    behavior.Update(dt);
                }
            }
        }
    }
}
