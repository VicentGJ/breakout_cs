# Scene Setup and Component Queries Spec

## Purpose

This document defines the next direction for scene setup and object access.

The current refactor moved Breakout object construction out of `BreakoutScene`, but the scene still owns direct references and lifecycle helpers for paddle, ball, and bricks. That keeps gameplay relationships centralized in the scene and makes the scene responsible for details that should belong to object setup, behaviors, or content builders.

The goal is to move toward this model:

`Scene -> GameObjects -> Components -> Systems/Behaviors`

The scene should own the object list and scene-level context. It should not act as a hand-written registry for specific game objects.

## Design Goals

- `Scene` remains the generic owner of game objects.
- `BreakoutScene` keeps Breakout-level context only: screen size, arena, phase, score, lives, input, draw hook.
- Game objects are created during scene setup.
- Runtime behavior finds related objects through component queries, not scene-owned direct references.
- Object recipes remain useful for construction, but not for runtime ownership.
- This stays small and linear-scan based. No ECS indexing, entity database, dependency injection, or broad scene service system yet.

## Non-Goals

- Do not introduce a full ECS.
- Do not introduce global object registries.
- Do not cache component lookups yet.
- Do not make every object relationship event-driven.
- Do not move Breakout-specific state into the engine.

## Scene Setup Lifecycle

Add a setup lifecycle hook to `Scene`.

```csharp
abstract class Scene
{
    public virtual void Setup()
    {
    }
}
```

`Runtime` calls `ActiveScene.Setup()` once when the initial scene is installed, and again when `ChangeScene` installs a replacement scene.

Setup is for initial object graph creation only. It should create objects, attach components, attach behaviors, and build initial content. It should not run per-frame logic.

`BreakoutScene.Setup()` should create:

- game state object with `RoundBehavior`
- presentation objects
- paddle object
- ball object
- initial brick wall

This lets the constructor focus on immutable scene context such as screen size and arena bounds.

## Component Queries

Add simple query helpers to `Scene`.

```csharp
public IEnumerable<GameObject> With<T>() where T : struct, IComponent
```

Returns active and inactive objects that have component `T`.

```csharp
public GameObject SingleWith<T>() where T : struct, IComponent
```

Returns the only object with component `T`, or throws if none/multiple exist.

```csharp
public bool TrySingleWith<T>(out GameObject? gameObject) where T : struct, IComponent
```

Returns `true` only when exactly one matching object exists.

Queries should be linear scans over the scene object list. That is enough for current Breakout scale and keeps the engine honest.

## Breakout Object Access

Remove these direct members from `BreakoutScene`:

- `Bricks`
- `PaddleObject`
- `BallObject`
- `ClearBricks`
- `ResetCoreObjects`
- `CreateBrickObject`

Use component queries instead:

- paddle: `Scene.SingleWith<PaddleComponent>()`
- ball: `Scene.SingleWith<BallComponent>()`
- bricks: `Scene.With<BrickComponent>()`

Behaviors may still cast `Scene` to `BreakoutScene` for Breakout context: arena bounds, phase, input, score, lives, and remaining brick count.

## Round Flow

`RoundBehavior` owns round lifecycle, not object storage.

It should:

- start a new game
- reset score/lives
- clear and rebuild bricks through `LevelBuilder`
- reset paddle and ball by component query
- update score and remaining brick count from events

Resetting paddle and ball means:

- query paddle/ball objects
- set active state
- reset transforms
- reset ball velocity and previous position

This logic belongs in round flow because it is round lifecycle behavior, not scene ownership.

## Level Building

`LevelBuilder` should create and remove brick objects through the scene object list.

Build flow:

- remove existing objects with `BrickComponent`
- calculate layout from the arena
- call `BrickObject.Create(scene, index, position, size, color, points)`
- return created brick count

`LevelBuilder` should not depend on a scene-owned `Bricks` list.

## Ball Collision

`BallBehavior` should resolve relationships by query.

- use `Scene.SingleWith<PaddleComponent>()` when sticking to paddle or resolving paddle collision
- iterate `Scene.With<BrickComponent>()` for brick collision
- ignore inactive bricks during collision

This keeps the relationship visible through components and avoids hidden object registries.

## Naming

`Game/Objects` remains the right module name.

Object recipe classes should stay grouped by object type:

- `Game/Objects/Paddle/PaddleObject.cs`
- `Game/Objects/Ball/BallObject.cs`
- `Game/Objects/Brick/BrickObject.cs`
- `Game/Objects/Presentation/PresentationObjects.cs`

Data structs keep `*Component` names because they are still components.

## Migration Plan

1. Add `Scene.Setup()` and call it from `Runtime`.
2. Add `Scene` component query helpers.
3. Move `BreakoutScene` object creation from constructor into `Setup()`.
4. Update `RoundBehavior` to reset paddle/ball via component queries.
5. Update `LevelBuilder` to clear/build bricks via component queries and return brick count.
6. Update `BallBehavior` to find paddle/bricks via component queries.
7. Remove direct object fields and helper methods from `BreakoutScene`.

## Verification

Run narrow compile check:

```bash
dotnet build breakout_cs.csproj --no-restore
```

Search for removed coupling:

```bash
rg "PaddleObject|BallObject|scene\\.Bricks|ClearBricks|ResetCoreObjects|CreateBrickObject" Game
```

Manual runtime check:

- scene starts with paddle, ball, bricks, HUD, and overlay
- ball serves from paddle
- launch works
- paddle collision works
- brick collision destroys bricks
- losing a life resets paddle and ball
- win/loss restart still works

## Open Questions

- Should `With<T>()` return inactive objects by default, or should there be `ActiveWith<T>()` later?
- Should `SingleWith<T>()` throw on multiple matches, or should game code use explicit filtering once duplicate component types become common?
- Should setup run from `Runtime` constructor, or should `Runtime.ChangeScene` own all scene installation through one private method?
