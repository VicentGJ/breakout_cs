# Component Lifecycle and Scene EventBus

## Purpose

This document defines the engine's component model and its scene-scoped event bus.

Every object attachment inherits `Component`. A component may hold only data or opt into frame callbacks through interfaces. This is a capability choice, not a separate component category.

## Component Model

`GameObject` stores one component of each type and exposes one API:

```csharp
gameObject.Add(new RectangleRendererComponent { /* data */ });
gameObject.Add(new BallBehavior(paddle, bricks));

RectangleRendererComponent renderer = gameObject.Get<RectangleRendererComponent>();
BallBehavior ball = gameObject.Get<BallBehavior>();
```

All components are reference objects derived from `Component`. They receive owner and scene context on attachment, and changes to their fields apply directly without a `Get`/`Set` round trip.

## Lifecycle and Frame Phases

`Component` provides:

```csharp
protected virtual void OnAttach();
protected virtual void OnDetach();
```

It also provides `Owner`, `Scene`, and `Enabled`.

Frame callbacks are explicit capabilities:

```csharp
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
```

A component implements only the interfaces it needs. For example, `RoundBehavior` implements `IStartable` and `IUpdatable`, `BallBehavior` implements both frame interfaces, and a renderer or collider implements neither.

Lifecycle order is deterministic:

1. `OnAttach` runs when the component is added to an object that belongs to a scene, or when its object joins a scene.
2. `IStartable.Start` runs once during the next normal update phase, when implemented.
3. `IUpdatable.Update` runs during the normal frame phase, when implemented.
4. `IPhysicsUpdatable.PhysicsUpdate` runs during the physics phase, when implemented.
5. `OnDetach` runs when the object leaves the scene.

Inactive objects do not dispatch frame callbacks. Disabled components remain attached but do not receive frame callbacks.

`Application` owns phase ordering. `Scene` owns objects and dispatches components implementing the relevant phase interface. Neither knows concrete game component types.

## Mutation Semantics

Each frame phase snapshots its object/component pairs before invoking callbacks. A component added during `Start`, `Update`, or `PhysicsUpdate` attaches immediately, but first receives a frame callback in the next phase.

Object attachment and detachment also iterate snapshots. A component added from `OnAttach` attaches immediately. A component added from `OnDetach` remains unattached because its object has already left the scene.

## Composition

Game components remain close to the objects they affect:

- `PaddleBehavior` controls paddle movement.
- `BallBehavior` handles serving, movement, and collision response.
- `BrickBehavior` owns brick destruction.
- `RoundBehavior` coordinates round flow from a dedicated game-state object.
- `PresentationBehavior` updates HUD and overlay objects.

Direct object references are appropriate for stable structural relationships. For example, `BallBehavior` receives its paddle and brick list during scene setup.

## Scene EventBus

Each scene owns one typed synchronous `EventBus`. Events are structs implementing `ISceneEvent`.

```csharp
Scene.Events.Publish(new BrickDestroyedEvent(Owner, points));

_subscription = Scene.Events.Subscribe<BrickDestroyedEvent>(OnBrickDestroyed);
```

Subscriptions return `IDisposable` values. Components dispose them in `OnDetach`.

Events represent facts that already happened, not commands. Breakout currently uses them for ball launch, ball out-of-bounds, brick destruction, and round start. Direct calls remain clearer when the sender has one known structural collaborator.

## Deferred Features

The engine intentionally does not provide component priorities, late-update phases, queued events, global events, object hierarchies, scene serialization, or generic component queries. Add one only after a concrete game need appears.
