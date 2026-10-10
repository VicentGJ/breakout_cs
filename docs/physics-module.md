# Physics Module Spec

## Why This Exists

The engine needs a small, reliable physics layer that can answer one question well: when two objects overlap, what happened, and how should they be separated? Breakout does not need a full rigid-body simulation. It needs collision math, a clean contact result, and a place to keep that logic separate from gameplay rules.

The design goal is to keep the engine responsible for geometry and correction, while keeping game-specific behavior such as score, brick destruction, and bounce behavior in the game layer.

## Implemented V1

Version 1 is box-only. Every collidable object is represented by a `BoxCollider` object with a local size. The collider is centered on its own origin, and its world transform (its `Transform2D` combined with its parent chain) provides the world-space position. A collidable thing is a parent object with a `BoxCollider` child.

This first version must support three things:

- The engine must be able to compute a signed contact result for two boxes, where positive distance means separated, zero means touching, and negative distance means overlap.

- The engine must be able to produce a correction vector so the caller can separate the objects and continue the simulation.

That is enough for the current Breakout setup: walls, bricks, paddle, and a box-shaped ball.

## What V1 Does Not Cover

This module is intentionally not a full physics engine. It should not try to simulate mass, force, gravity, restitution, friction, angular velocity, joints, or sleeping bodies. It should also not introduce broadphase acceleration structures yet. The current game has a very small number of colliders, so brute-force pair checking is acceptable for now.

Circle colliders are also out of scope for the first pass. They may be added later if the gameplay needs a round ball collider or if we want the engine to support more common 2D shape combinations.

## Public Shape Model

The public collision shape for v1 is `BoxCollider`.

It should hold local-space shape data only. The collider should not store world-space state or attempt to own the object’s transform. Instead, it should be a child object, with the world position coming from `GetWorldTransform()` when collision queries run.

The shape data itself is simple:

- `size`: the width and height of the box

That is enough to describe the paddle, the bricks, the walls, and any simple rectangular gameplay object as long as the collider stays centered on the object origin.

## How Transform Fits In

`Transform2D` is core state on every `GameObject`. Physics does not replace it. Physics reads it.

When the engine asks for the signed distance between two objects, it combines the object transform with the collider size to produce world-space box bounds. In v1 the collider stays centered on the object origin, so the transform describes where the object is and the collider describes how large the box is.

This separation matters because it keeps rendering, collision, and gameplay data independent. A visual rectangle should not be the only source of collision geometry, and a collider should not need to know how it is drawn.

## Core API Shape

The module should feel closer to Unity than to a separate query service. The collider is the main thing the game works with, and the heavy math stays behind it.

The v1 surface should look like this:

```csharp
sealed class BoxCollider : GameObject
{
    Vector2 Size;
    Collision GetCollision(BoxCollider other);
}

struct Collision
{
    Vector2 Normal;
    float Distance;
    Vector2 Point;
}

static class CollisionHelper
{
    Vector2 GetCorrection(in Collision collision);
}
```

`BoxCollision.GetCollision` takes two colliders and their transforms and returns a signed `Collision` result. `Distance > 0` means the boxes are separated, `Distance == 0` means they are touching, and `Distance < 0` means they overlap.

`CollisionHelper.GetCorrection` turns a signed `Collision` into the correction vector the caller applies to separate the objects. For a zero or positive distance result, it should return `Vector2.Zero`.

This keeps the public API small and readable. Game code talks to colliders directly instead of building an intermediate query object first.

Example flow:

```csharp
var playerBox = player.Collider;
var wallBox = wall.Collider;

var collision = BoxCollision.GetCollision(playerBox, wallBox, player.Transform2D, wall.Transform2D);
var correction = CollisionHelper.GetCorrection(collision);

player.Transform2D.Position -= correction;
```

Example flow with the signed result:

```csharp
var collision = BoxCollision.GetCollision(playerBox, wallBox, player.Transform2D, wall.Transform2D);

if (collision.Distance < 0)
{
    player.Transform2D.Position -= CollisionHelper.GetCorrection(collision);
}
```

## Collision Data

The `Collision` result should carry the minimum information needed to separate objects and make gameplay decisions. It should not own behavior or physics helpers.

- `Normal`: the direction from the first collider toward the second.
- `Distance`: the signed separation distance. Positive means separated, zero means touching, negative means overlapping.
- `Point`: an approximate contact point, if useful.

For v1, all three fields are part of the public result. `Point` is included for completeness even if the current gameplay code does not use it yet.

## Engine Responsibilities

The physics module decides geometry. It should answer whether objects overlap, which side the contact came from, how far the overlap goes, and how to move the objects apart.

The physics module should not decide game meaning. It should not destroy bricks, update score, choose a special bounce angle, or determine win and lose state. Those are gameplay rules and belong in game-side object classes.

This split keeps the engine reusable. The same collision code can later support a different game with different rules.

## Suggested File Layout

The module should live under `Engine/Physics/` and stay small enough to reason about easily.

- `BoxCollider.cs`: the collider object type
- `Collision.cs`: collision result data
- `CollisionHelper.cs`: helper math such as correction vectors
- `BoxCollision.cs`: internal box-box intersection and contact math

That layout keeps shape data, math, and query code separated without over-engineering the first version.

## Current Boundary

This first pass is complete. The engine provides box collision geometry and correction. `Ball` applies Breakout-specific movement, bounce, brick destruction, and round consequences.

The engine has:

- geometry in `Engine/Physics/`

- game rules in game object classes

- a single box collider shape for v1

- tests for overlap and edge cases such as corners and grazing contacts

## Later Extensions

Future passes can add circle colliders, sweep tests for faster objects, collider offsets, multiple colliders per object, and broadphase acceleration if the scene grows large enough to need it.
