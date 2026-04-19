# BehaviorComponent and Scene EventBus Spec

## Purpose

This document defines a minimal gameplay-logic model for the engine based on two additions:

1. `BehaviorComponent`, a specialized component type for object-owned logic.
2. A minimal typed scene-level `EventBus` for decoupled gameplay communication.

The goal is to move the current architecture from scene-wide gameplay systems toward object-owned behavior in a way that remains simple, readable, and consistent with the existing engine shape. The design should feel natural in an OOP codebase, avoid premature complexity, and remain small enough to serve the current Breakout game without overcommitting the engine to a large framework.

This is not intended to be a full messaging architecture, a general-purpose scripting system, or a full ECS event bus. It is a minimal foundation for object-owned gameplay logic and lightweight decoupling.

---

## Design Goals

The design should satisfy a few concrete goals.

Behavior should live with the object that owns it. A paddle should own paddle behavior, a ball should own ball behavior, and a brick should own brick behavior. This keeps gameplay logic close to the object it affects.

The runtime should remain decoupled from concrete game logic. The runtime should know how to advance phases and dispatch behavior hooks, but it should not know about specific gameplay classes.

The communication model should be singular and minimal. The engine should not implement several overlapping communication systems. The default decoupling mechanism should be one small typed event system at scene scope.

The model should stay compatible with the existing component-oriented object model. `BehaviorComponent` should be understood as a special kind of component, not as a completely separate conceptual attachment model.

The system should remain intentionally incomplete. It should solve current needs cleanly and leave room for future refinement without forcing speculative features now.

---

## Non-Goals

This design does not try to solve every future engine problem.

It does not introduce a global engine-wide event bus. Events are scoped to a scene.

It does not introduce addressed messaging, async queues, wildcard routing, string-based event names, priorities, or command dispatch semantics.

It does not attempt to eliminate direct references between objects. If two objects already know about each other and a direct callback or method call is the clearest option, that remains valid.

It does not replace all existing systems immediately. Migration can happen incrementally.

It does not require every gameplay interaction to go through events. Events are a default decoupling tool, not a mandatory rule for all communication.

---

# Part I: BehaviorComponent

## Overview

`BehaviorComponent` is a specialized component type used for executable gameplay logic owned by a `GameObject`.

This is different from the engine’s current lightweight data components. Existing components are primarily plain state holders such as render data, collider data, score values, or movement values. `BehaviorComponent` is meant for logic with lifecycle, update hooks, optional subscriptions, and private mutable state.

The design keeps the existing idea that objects own components, but it acknowledges that not all components are the same. A render component describes how something looks. A collider component describes geometry. A behavior component describes what the object does.

---

## Role in the Architecture

`BehaviorComponent` sits between runtime phase dispatch and concrete gameplay logic.

The runtime controls frame phases. The scene owns objects. Objects own components. Behavior components participate in runtime phases through engine-defined hooks.

The runtime should dispatch through stable behavior contracts only. It should not depend on concrete gameplay classes such as `PaddleBehaviorComponent` or `BallBehaviorComponent`.

This gives the engine a simple orchestration model:

`Runtime -> Scene -> GameObjects -> BehaviorComponents`

That is the only execution relationship the engine needs to know about.

---

## Conceptual Model

A `GameObject` may contain both passive data components and active behavior components.

Passive components are used as object state and configuration. They are read and written by behaviors and by engine systems such as rendering and collision.

Behavior components own logic and may read or mutate object state, react to frame phases, and publish or subscribe to scene events through the scene’s `EventBus`.

This means the engine recognizes two internal categories of components:

- data components
- behavior components

These categories should remain compatible with the shared idea of object-owned components, but they do not need identical storage or identical semantics.

---

## Required Characteristics

A `BehaviorComponent` must have reference semantics. It should be a class, not a struct.

A `BehaviorComponent` must know its owner object once attached.

A `BehaviorComponent` must know the scene it belongs to once attached.

A `BehaviorComponent` may hold private mutable state.

A `BehaviorComponent` may subscribe to scene events during attachment or startup and must be able to clean up those subscriptions when detached.

A `BehaviorComponent` may opt into one or more engine phases, but the minimal version should only expose the phases the current game actually needs.

---

## Proposed Base Shape

The base type should be intentionally small.

```csharp
public abstract class BehaviorComponent : IComponent
{
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
    }

    protected virtual void OnAttach() { }
    protected virtual void OnDetach() { }

    public virtual void Start() { }
    public virtual void Update(float dt) { }
    public virtual void PhysicsUpdate(float dt) { }
}
```

This shape is deliberately conservative. It gives the engine only a few stable hooks and avoids trying to mirror every lifecycle callback seen in larger engines.

---

## Lifecycle

The lifecycle should be simple and deterministic.

### Attach

A behavior becomes attached when it is added to a `GameObject` that belongs to a scene. At that point, the engine assigns `Owner` and `Scene`, then calls `OnAttach()`.

`OnAttach()` is intended for immediate setup that depends on object and scene context.

### Start

`Start()` should run once, after attachment, before the first normal update phase in which the behavior participates.

This exists to support initialization that should happen after all relevant components and initial scene relationships are in place.

A behavior should never receive `Update()` or `PhysicsUpdate()` before `Start()` has run.

### Update

`Update(float dt)` is the normal per-frame logic hook. It is intended for input handling, non-physics gameplay logic, timers, state transitions, and visual logic.

### PhysicsUpdate

`PhysicsUpdate(float dt)` is the fixed or physics-phase hook. It is intended for collision-related motion and physics-sensitive logic.

The current engine already separates regular update from physics update, so this hook maps naturally onto the existing runtime phases.

### Detach

When a behavior is removed or its object leaves the scene, `OnDetach()` should run. This is where event subscriptions and other external bindings must be released.

---

## Enabled State

A behavior component should have its own `Enabled` flag.

This is separate from the `GameObject.Active` flag. If the object is inactive, none of its behavior components run. If the object is active but a specific behavior is disabled, that behavior does not receive phase callbacks.

This allows a single object to keep some behaviors dormant without disabling the entire object.

The minimal version does not need callback hooks for enable/disable transitions. A plain boolean gate is enough.

---

## Ownership and Access Rules

A behavior component has privileged access to its owner and its scene. It may read and write the owner’s transform and components.

It should not hold a direct runtime reference.

It may hold references to other objects when those relationships are structural and natural. For example, a ball behavior may safely reference the paddle if the game design makes that relationship explicit and stable.

It should prefer the scene `EventBus` when communication is better expressed as “something happened” rather than “call this specific object now.”

---

## Storage Model

Behavior components should be stored separately from passive data components internally, even though both are still conceptually components owned by the object.

This keeps the existing data-component model clean and avoids forcing lifecycle-driven logic into the same storage semantics as lightweight state.

A practical object shape would look like this:

```csharp
public sealed class GameObject
{
    private readonly Dictionary<Type, IComponent> _dataComponents = new();
    private readonly List<BehaviorComponent> _behaviorComponents = new();

    public Transform2D Transform { get; } = new();
    public bool Active { get; set; } = true;

    public IEnumerable<BehaviorComponent> Behaviors => _behaviorComponents;
}
```

This keeps the object model coherent without pretending that passive state and executable logic are identical internally.

---

## Attachment Rules

A `GameObject` may have zero or more behavior components.

Multiple behavior components of different kinds may coexist on the same object.

The engine should allow multiple behaviors because composition is one of the main benefits of the model. An object might eventually have behavior such as input, animation, lifetime, and reaction logic at the same time.

The minimal version does not need complicated ordering controls. Behaviors may execute in insertion order.

If ordering becomes important later, that can be introduced as a small explicit priority field or phase ordering policy, but it should not be part of the first implementation.

---

## Responsibilities That Fit BehaviorComponent Well

Behavior components are intended for object-local or object-owned gameplay logic.

Examples include paddle input, ball serving and launch logic, local timers, object reactions to collisions, animations, effects, cooldowns, and object-specific state transitions.

These behaviors should remain close to the object they affect.

---

## Responsibilities That Should Remain Outside BehaviorComponent

Not all logic should move into behavior components.

Rendering, broad collision traversal, level construction, and scene reset orchestration are still better treated as engine systems or scene/game services.

The purpose of this design is not to eliminate all systems. It is to move object-owned gameplay logic into a more natural place while keeping shared cross-cutting functionality centralized where that still makes sense.

---

# Part II: Scene EventBus

## Overview

The scene `EventBus` is the default decoupling mechanism for communication between behavior components when direct object references are not the clearest solution.

It is intentionally small. It exists to let behavior components publish typed facts about what happened in the scene and let interested behavior components react to them.

This is a scene-scoped event stream, not a global engine bus and not a command router.

---

## Purpose

The event system solves one problem only: decoupled notification.

A behavior should be able to say that something happened without knowing which other behaviors care. Other behaviors should be able to listen for relevant events without tightly coupling themselves to the sender’s concrete implementation.

That is the whole purpose of the system.

---

## Scope

Events are scoped to a single scene.

A scene owns its own `EventBus`. Objects and behaviors inside that scene may publish and subscribe.

Events do not cross scene boundaries automatically.

There is no engine-global event system in this design.

---

## Event Philosophy

Events should represent facts, not requests.

A fact says that something already happened or became true.

Examples:

- `BrickDestroyed`
- `BallLaunched`
- `BallOutOfBounds`
- `RoundStarted`

These are good event shapes because they describe observable gameplay facts.

A request says that something should be done.

Examples:

- `ResetLevelNow`
- `MovePaddle`
- `SpawnBrick`
- `AddScore`

These are not good default event shapes for this system. They make the event layer act like a command bus and blur ownership.

This design should stay fact-oriented.

---

## Dispatch Model

The minimal system should use synchronous dispatch.

When an event is published, all current subscribers for that event type are invoked immediately in subscription order.

This keeps the implementation straightforward and predictable. It also matches the current small-engine scope better than introducing queue semantics or staged delivery.

If queued delivery is needed later, that can be introduced deliberately as a change in semantics, not as hidden complexity from the start.

---

## Type Model

Events should be strongly typed.

There should be no string event names, no reflection-based event matching, and no loosely typed payload dictionaries.

A minimal event marker interface is enough:

```csharp
public interface ISceneEvent
{
}
```

Concrete events should be plain immutable structs or record structs.

```csharp
public readonly record struct BrickDestroyedEvent(GameObject Brick, int Score) : ISceneEvent;
public readonly record struct BallOutOfBoundsEvent(GameObject Ball) : ISceneEvent;
public readonly record struct BallLaunchedEvent(GameObject Ball) : ISceneEvent;
```

This keeps event definitions explicit, searchable, and cheap to reason about.

---

## EventBus API

The scene should expose one small typed dispatcher.

A minimal shape could be:

```csharp
public sealed class EventBus
{
    public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        where TEvent : struct, ISceneEvent;

    public void Publish<TEvent>(TEvent evt)
        where TEvent : struct, ISceneEvent;
}
```

This is the preferred minimal API for the first version. It is small, explicit, and keeps subscription ownership simple.

The returned `IDisposable` represents the subscription. Disposing it unsubscribes the handler.

---

## Subscription Ownership

Subscriptions created by a behavior component should be owned by that behavior component.

The behavior should subscribe during `OnAttach()` or `Start()` and release those subscriptions during `OnDetach()`.

The engine should not try to infer or automatically manage subscription lifetimes behind the scenes in the first version. Explicit ownership is simpler and easier to debug.

A behavior base class may later offer a small helper list for disposable subscriptions, but that is optional sugar, not core design.

---

## Sender Information

Events should carry any sender or subject information explicitly in their payload.

The `EventBus` itself does not need a special sender parameter.

For example:

```csharp
public readonly record struct BrickDestroyedEvent(GameObject Brick, int Score) : ISceneEvent;
```

This keeps the event type self-contained and avoids special dispatcher rules.

---

## Intended Use

The `EventBus` should be used when one behavior needs to express a gameplay fact that other behaviors may care about, but direct object-to-object calls would unnecessarily couple them.

Examples in the current game include a brick being destroyed, a ball leaving the arena, a round beginning, or the ball being launched.

These are scene-relevant facts that can be observed by game-state behaviors, UI behaviors, audio behaviors, or future effects behaviors without forcing the sender to know about all listeners.

---

## Cases Where Direct Calls Are Still Preferred

Events are not mandatory.

If an object already holds a natural structural reference to another object and the interaction is simple, a direct callback or method call is still acceptable.

For example, if the ball behavior already holds a known reference to the paddle and needs a simple query or direct coordination, using that reference may be clearer than publishing an event.

The event system exists to decouple where decoupling improves the design. It should not become a ritual applied everywhere.

---

# Part III: Scene and Runtime Integration

## Scene Base Shape

The scene owns object membership and the `EventBus`.

A minimal base shape could look like this:

```csharp
public abstract class Scene
{
    private readonly List<GameObject> _objects = new();

    public EventBus Events { get; } = new();

    public IReadOnlyList<GameObject> Objects => _objects;

    public virtual void Update(float dt) { }
    public virtual void PhysicsUpdate(float dt) { }

    public void AddObject(GameObject obj)
    {
        _objects.Add(obj);
        obj.AttachToScene(this);
    }
}
```

This is the missing structural assumption behind behavior code like `Scene.Events.Subscribe(...)` and `Scene.Events.Publish(...)`.

The scene is the scope that ties together object ownership, behavior execution, and event communication.

---

## Concrete Scene Shape

A concrete scene inherits from `Scene`, owns game-specific state, and builds the object composition of that game.

For example:

```csharp
public sealed class BreakoutScene : Scene
{
    public Rectangle Arena { get; }
    public int Score { get; set; }
    public int Lives { get; set; }

    public GameObject Paddle { get; private set; } = null!;
    public GameObject Ball { get; private set; } = null!;

    public BreakoutScene(int width, int height)
    {
        Arena = new Rectangle(40, 80, width - 80, height - 160);

        CreateCoreObjects();
        CreateLevel();
    }

    private void CreateCoreObjects()
    {
        var gameState = new GameObject("GameState");
        gameState.AddBehavior(new RoundBehaviorComponent());

        Paddle = new GameObject("Paddle");
        Paddle.AddBehavior(new PaddleBehaviorComponent());

        Ball = new GameObject("Ball");
        Ball.AddBehavior(new BallBehaviorComponent());

        AddObject(gameState);
        AddObject(Paddle);
        AddObject(Ball);
    }

    private void CreateLevel()
    {
        // create bricks and AddObject(...)
    }
}
```

This shows how object-owned behavior and scene-scoped events fit into a concrete game scene without moving round/game-state logic into the `Scene` class itself.

---

## Example Scene Composition

A simple scene composition for the example could look like this:

```text
Scene
├── GameObject: GameState
│   └── RoundBehaviorComponent
├── GameObject: Paddle
│   ├── PaddleBehaviorComponent
│   ├── RectangleRendererComponent
│   └── BoxColliderComponent
├── GameObject: Ball
│   ├── BallBehaviorComponent
│   ├── RectangleRendererComponent
│   └── BoxColliderComponent
├── GameObject: Brick_01
│   ├── BrickBehaviorComponent
│   ├── RectangleRendererComponent
│   └── BoxColliderComponent
├── GameObject: Brick_02
│   ├── BrickBehaviorComponent
│   ├── RectangleRendererComponent
│   └── BoxColliderComponent
└── ...
```

In this layout, `RoundBehaviorComponent` is object-owned like any other behavior, but it is attached to a dedicated scene coordinator object such as `GameState` rather than to a visible gameplay object.

---

## Runtime Responsibility

The runtime remains responsible for frame phase orchestration.

It should not interpret gameplay events. It should not know concrete behavior types. It should only:

- advance the active scene through its phases
- ensure behavior lifecycle is respected
- dispatch `Start`, `Update`, and `PhysicsUpdate` where appropriate

The runtime remains an orchestrator, not a gameplay participant.

---

## Scene Responsibility

The scene owns:

- the collection of objects
- the `EventBus`
- object membership and lifetime within the scene

A `BehaviorComponent` is valid only in the context of an attached object that belongs to a scene.

---

## GameObject Responsibility

A `GameObject` owns:

- its transform
- its active state
- its passive data components
- its behavior components

It is the local composition root for object state and object logic.

---

## Suggested Update Flow

A minimal scene update flow could be:

1. Ensure newly attached behaviors are started.
2. Iterate active objects.
3. Iterate enabled behaviors on each object.
4. Call `Update(dt)` during the normal phase.
5. Call `PhysicsUpdate(dt)` during the physics phase.

The exact implementation can be optimized later. The main requirement is deterministic phase dispatch and stable lifecycle ordering.

---

# Part IV: Example Usage

## Example Event Definitions

```csharp
public interface ISceneEvent
{
}

public readonly record struct BallLaunchedEvent(GameObject Ball) : ISceneEvent;

public readonly record struct BrickDestroyedEvent(GameObject Brick, int Score) : ISceneEvent;

public readonly record struct BallOutOfBoundsEvent(GameObject Ball) : ISceneEvent;
```

---

## Example Ball Behavior

```csharp
public sealed class BallBehaviorComponent : BehaviorComponent
{
    private BallMovementComponent _movement = default!;

    public override void Start()
    {
        _movement = Owner.Get<BallMovementComponent>();
    }

    public override void Update(float dt)
    {
        // Serving / launch logic here
    }

    public override void PhysicsUpdate(float dt)
    {
        // Motion and collision handling here

        if (HasLeftArena())
        {
            Scene.Events.Publish(new BallOutOfBoundsEvent(Owner));
        }
    }

    private bool HasLeftArena()
    {
        return Owner.Transform.Position.Y > 1000f;
    }
}
```

---

## Example Brick Behavior

```csharp
public sealed class BrickBehaviorComponent : BehaviorComponent
{
    private BrickComponent _brick = default!;

    public override void Start()
    {
        _brick = Owner.Get<BrickComponent>();
    }

    public void DestroyBrick()
    {
        Scene.Events.Publish(new BrickDestroyedEvent(Owner, _brick.Score));
        Owner.Active = false;
    }
}
```

---

## Example Round Behavior

```csharp
public sealed class RoundBehaviorComponent : BehaviorComponent
{
    private IDisposable? _brickDestroyedSubscription;
    private IDisposable? _ballOutSubscription;

    protected override void OnAttach()
    {
        _brickDestroyedSubscription =
            Scene.Events.Subscribe<BrickDestroyedEvent>(OnBrickDestroyed);

        _ballOutSubscription =
            Scene.Events.Subscribe<BallOutOfBoundsEvent>(OnBallOutOfBounds);
    }

    protected override void OnDetach()
    {
        _brickDestroyedSubscription?.Dispose();
        _ballOutSubscription?.Dispose();
    }

    private void OnBrickDestroyed(BrickDestroyedEvent evt)
    {
        // Update score and brick count
    }

    private void OnBallOutOfBounds(BallOutOfBoundsEvent evt)
    {
        // Lose life and reset round
    }
}
```

In this example, `OnBrickDestroyed(...)` and `OnBallOutOfBounds(...)` are not called by the runtime directly. They are called by the scene `EventBus` when the corresponding events are published.

The flow is:

1. A behavior subscribes a handler through `Scene.Events.Subscribe(...)`.
2. Another behavior publishes an event through `Scene.Events.Publish(...)`.
3. The `EventBus` finds all subscribers for that event type.
4. The `EventBus` invokes their handlers immediately.

---

# Part V: Migration Guidance

## Migration Strategy

The transition should be incremental.

The engine does not need a large rewrite. It can introduce `BehaviorComponent` and the scene `EventBus` first, then migrate existing gameplay logic piece by piece.

The safest path is to start with the most object-local systems.

Paddle control is a strong early candidate because it is clearly owned by the paddle object.

Ball serving and launch logic are also strong candidates because they belong naturally to the ball object.

Brick reaction logic is another good candidate because it is highly local and object-owned.

More shared responsibilities such as level building, rendering, or broader collision traversal can remain in their existing system-based form until there is a clear reason to move them.

---

## Likely First Migration Targets

A reasonable first pass would be:

- move paddle movement into `PaddleBehaviorComponent`
- move ball serve and motion logic into `BallBehaviorComponent`
- move brick hit or destruction response into `BrickBehaviorComponent`
- attach `RoundBehaviorComponent` to a dedicated `GameState` object
- keep scene reset and level construction outside behavior components
- introduce scene events only where they remove obvious coupling

This lets the engine validate the model in real gameplay without overcommitting too early.

---

# Part VI: Future Extensions

## Extensions Explicitly Deferred

The following features are intentionally deferred:

- ordered behavior priorities
- late update phases
- post-physics phases
- async or queued event delivery
- event filtering by sender
- sticky events
- global engine-wide event bus
- object-level event dispatchers
- automatic subscription tracking in the base class
- editor serialization of behaviors

These may become useful later, but they should not be part of the first implementation.

---

## Criteria for Future Reconsideration

The design should only expand if real pressure appears in the codebase.

If behavior ordering becomes a repeated source of bugs, ordering controls may be justified.

If event reentrancy or mutation during dispatch becomes problematic, queued delivery may be justified.

If scene-wide subscription patterns become too broad or too noisy, event scoping may need refinement.

If many behaviors repeatedly need the same subscription bookkeeping, small quality-of-life helpers may be justified.

Until then, the design should remain small.

---

# Final Summary

The engine will adopt a minimal object-owned behavior model through `BehaviorComponent`, paired with a minimal typed scene-scoped `EventBus`.

`BehaviorComponent` is a specialized component type for executable object logic. It is reference-based, lifecycle-aware, and owned by a `GameObject`.

The scene `EventBus` is the default decoupling mechanism for behavior communication when direct references are not the clearest tool. It is typed, synchronous, scene-scoped, and fact-oriented.

The `Scene` base class owns the `EventBus` and the object collection. Concrete scenes such as `BreakoutScene` build gameplay object composition on top of that base. Scene-wide gameplay orchestration that still belongs in the object model can live on a dedicated coordinator object such as `GameState`.

This design preserves the current engine’s clean runtime and scene structure, aligns with the desired move toward a more natural OOP gameplay model, and stays intentionally small enough to serve the current game without unnecessary framework growth.
