# Physics Module Spec

## Why This Exists

The engine needs a small, reliable physics layer that can answer one question well: when two objects overlap, what happened, and how should they be separated? Breakout does not need a full rigid-body simulation. It needs collision math, a clean contact result, and a place to keep that logic separate from gameplay rules.

The design goal is to keep the engine responsible for geometry and correction, while keeping game-specific behavior such as score, brick destruction, and bounce behavior in the game layer.

## What V1 Covers

Version 1 is box-only. That keeps the first physics pass small and focused. Every collidable object is represented by a `BoxCollider` with a local size. The collider is centered on the object origin, and the object’s `Transform2D` provides the world-space position.

This first version must support three things:

- The engine must be able to test whether two boxes overlap.

- The engine must be able to compute a contact description for an overlap, including the direction to push the objects apart and how deep the overlap is.

- The engine must be able to produce a correction vector so the caller can separate the objects and continue the simulation.

That is enough for the current Breakout setup: walls, bricks, paddle, and a box-shaped ball.

## What V1 Does Not Cover

This module is intentionally not a full physics engine. It should not try to simulate mass, force, gravity, restitution, friction, angular velocity, joints, or sleeping bodies. It should also not introduce broadphase acceleration structures yet. The current game has a very small number of colliders, so brute-force pair checking is acceptable for now.

Circle colliders are also out of scope for the first pass. They may be added later if the gameplay needs a round ball collider or if we want the engine to support more common 2D shape combinations.

## Public Shape Model

The public collision shape for v1 is `BoxCollider`.

It should hold local-space shape data only. The collider should not store world-space state or attempt to own the object’s transform. Instead, it should be a component-style data structure attached to an object, with the world position coming from `Transform2D` when collision queries run.

The shape data itself is simple:

- `size`: the width and height of the box

That is enough to describe the paddle, the bricks, the walls, and any simple rectangular gameplay object as long as the collider stays centered on the object origin.

## How Transform Fits In

`Transform2D` is core state on every `GameObject`. Physics does not replace it. Physics reads it.

When the engine asks whether two objects overlap, it combines the object transform with the collider size to produce world-space box bounds. In v1 the collider stays centered on the object origin, so the transform describes where the object is and the collider describes how large the box is.

This separation matters because it keeps rendering, collision, and gameplay data independent. A visual rectangle should not be the only source of collision geometry, and a collider should not need to know how it is drawn.

## Core API Shape

The module should feel closer to Unity than to a separate query service. The collider is the main thing the game works with, and the heavy math stays behind it.

The v1 surface should look like this:

```csharp
struct BoxCollider
{
    Vector2 Size;

    Rectangle GetWorldBounds(in Transform2D transform);
    bool Intersects(in BoxCollider other, in Transform2D selfTransform, in Transform2D otherTransform);
    Collision GetCollision(in BoxCollider other, in Transform2D selfTransform, in Transform2D otherTransform);
}

struct Collision
{
    Vector2 Normal;
    float Depth;
    Vector2 Point;
}

static class Physics2D
{
    Vector2 GetCorrection(in Collision collision);
}
```

`GetWorldBounds` takes the collider plus the object transform and returns the collider in world space.

`Intersects` takes two colliders and their transforms and returns a boolean overlap answer.

`GetCollision` takes the same inputs and returns a `Collision` only when the boxes overlap. If the boxes do not overlap, it should return `default` and the caller should treat that as “no collision”.

`Physics2D.GetCorrection` turns a valid `Collision` into the correction vector the caller applies to separate the objects. For a default or empty collision, it should return `Vector2.Zero`.

This keeps the public API small and readable. Game code talks to colliders directly instead of building an intermediate query object first.

Example flow:

```csharp
var playerBox = player.GetComponent<BoxCollider>();
var wallBox = wall.GetComponent<BoxCollider>();

if (playerBox.Intersects(wallBox, player.Transform2D, wall.Transform2D))
{
    var collision = playerBox.GetCollision(wallBox, player.Transform2D, wall.Transform2D);
    var correction = Physics2D.GetCorrection(collision);

    player.Transform2D.Position -= correction;
}
```

## Collision Data

The `Collision` result should carry the minimum information needed to separate objects and make gameplay decisions. It should not own behavior or physics helpers.

- `Normal`: the direction from the first collider toward the second.
- `Depth`: how deeply the boxes overlap.
- `Point`: an approximate contact point, if useful.

For v1, `Normal` and `Depth` are the important fields. `Point` can be approximate or omitted from gameplay use if it turns out not to be needed immediately.

## Engine Responsibilities

The physics module decides geometry. It should answer whether objects overlap, which side the contact came from, how far the overlap goes, and how to move the objects apart.

The physics module should not decide game meaning. It should not destroy bricks, update score, choose a special bounce angle, or determine win and lose state. Those are gameplay rules and belong in `CollisionSystem` or another game-side system.

This split keeps the engine reusable. The same collision code can later support a different game with different rules.

## Suggested File Layout

The module should live under `Engine/Physics/` and stay small enough to reason about easily.

- `BoxCollider.cs`: the component type and the public collider-facing methods
- `Collision.cs`: collision result data
- `Physics2D.cs`: helper math such as correction vectors
- `BoxCollision.cs`: internal box-box intersection and contact math

That layout keeps shape data, math, and query code separated without over-engineering the first version.

## Migration Plan

The migration should be mechanical and low-risk.

First, create the new physics folder and move the box-collider data into it.

Next, implement box-box overlap testing and contact generation.

Then replace the current collision math usage with the new physics module.

After that, keep `CollisionSystem` focused on gameplay responses such as brick destruction, score changes, and paddle bounce behavior.

Once all callers use the new module, retire `GameObjectGeometry.cs`.

## Done When

This first pass is complete when the engine can correctly handle rectangular collision cases for the current Breakout game and the game layer no longer contains geometry math.

At that point, the engine should have:

- geometry in `Engine/Physics/`

- game rules in the game systems

- a single box collider shape for v1

- tests for overlap and edge cases such as corners and grazing contacts

## Later Extensions

Future passes can add circle colliders, sweep tests for faster objects, collider offsets, multiple colliders per object, and broadphase acceleration if the scene grows large enough to need it.
