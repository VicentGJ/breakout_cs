# Object Lifecycle and Scene EventBus

## Purpose

This document defines how objects behave over time and how they talk to each other: the lifecycle hooks on `GameObject`, how objects find each other, and the scene-scoped event bus.

## Object Model

Every object inherits `GameObject`. Game and engine types are subclasses (`Ball`, `Brick`, `Sprite`, `BoxCollider`). Reuse comes from children in the tree, not from attached components.

```csharp
sealed class Paddle : GameObject
{
    public Paddle()
    {
        Visual = AddChild(new RectangleShape { /* data */ });
        Collider = AddChild(new BoxCollider { /* data */ });
    }

    public RectangleShape Visual { get; }
    public BoxCollider Collider { get; }
}
```

An object stores references only to its own children. Objects are reference types, so changes apply directly.

## Lifecycle Hooks

```csharp
protected virtual void OnAttach();
protected virtual void OnDetach();
public virtual void Start();
public virtual void Update(float dt);
public virtual void PhysicsUpdate(float dt);
```

A class overrides only what it needs. A pure data object such as `BoxCollider` overrides nothing.

Order is deterministic:

1. `OnAttach` runs when the object joins a scene. Touch only the object itself and scene events here. Other objects may not be in the scene yet.
2. `Start` runs once, at the beginning of the next update phase, after the whole loaded graph is attached. All `Start` calls of a frame run before any `Update`. It may look at the rest of the world with `Scene.Find`. Order between objects is undefined, so do not depend on another object's own `Start` having run.
3. `Update` runs during the normal frame phase.
4. `PhysicsUpdate` runs during the physics phase.
5. `OnDetach` runs when the object leaves the scene.

Objects that are not `ActiveInHierarchy` receive no callbacks.

`Application` owns phase ordering. `Scene` dispatches to objects. Neither knows concrete game types.

## Mutation Semantics

Each frame phase snapshots the active objects before invoking callbacks. An object added during a callback attaches immediately but first receives frame callbacks in the next phase. An object removed during a callback is skipped if it has not run yet.

`Scene.FindAll<T>()` enumerates the live object list: do not add or remove objects while enumerating it. Copy with `ToList()` first.

## Finding Other Objects

```csharp
GameState game = Scene.Find<GameState>()!;     // first object of that type
foreach (Brick brick in Scene.FindAll<Brick>()) // all of that type
```

Lookup is by type and is a linear scan. Do it once in `Start` for objects that live for the whole scene. Objects that come and go (bricks) are looked up when needed. Do not store references to objects outside your own subtree in serialized fields.

## Scene EventBus

Each scene owns one typed synchronous `EventBus`. Events are structs implementing `ISceneEvent`.

```csharp
Scene.Events.Publish(new BrickDestroyedEvent(this, Points));

_subscription = Scene.Events.Subscribe<BrickDestroyedEvent>(OnBrickDestroyed);
```

Subscriptions return `IDisposable` values. Objects dispose them in `OnDetach`.

Events represent facts that already happened, not commands. Breakout uses them for ball launch, ball out-of-bounds, brick destruction, and round start. Use them when one fact has several independent reactions. For one known collaborator, a direct call is clearer.

## Deferred Features

The engine intentionally does not provide priorities between objects, late-update phases, queued events, global events, reparenting, or scene serialization. Add one only after a concrete game need appears.
