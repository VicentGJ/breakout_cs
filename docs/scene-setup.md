# Scene Setup

## Purpose

This document records how a scene builds its initial object graph and how objects relate to each other at runtime.

## Setup Lifecycle

`Application` installs scenes through `ChangeScene`, which binds audio and then calls `Scene.Setup()` exactly once per scene instance. Setup creates the initial graph. It does not run per-frame logic.

```csharp
public override void Setup()
{
    AddObject(new GameState());
    AddObject(new Paddle());
    AddObject(new Ball());
    AddObject(new RectangleShape { Name = "Arena", /* ... */ });
    AddObject(new Hud());
}
```

Each game class has a parameterless constructor and builds its own children. Layout that depends on scene context (arena, screen size) is read from `BreakoutScene` in `Start` or set by the code that creates the object. Objects are inserted in the order that determines their update order.

## Relationships

- An object stores only its own children (`Paddle.Collider`, `Ball.Visual`).
- Anything else is found by type with `Scene.Find<T>()` / `FindAll<T>()` in `Start`, or arrives through events. `Ball` finds the `Paddle` and `GameState` in `Start` and the bricks each physics frame.
- `GameState` holds phase, score, lives, and remaining bricks. It is a regular object found by type.
- `BreakoutScene` keeps scene context that is not an object: `ScreenWidth`, `ScreenHeight`, `Arena`, and the per-frame `Input`.

## Brick Wall

`BrickWallObjects.Create(arena)` returns unattached `Brick` objects. `GameState` removes the old bricks (`Scene.FindAll<Brick>().ToList()` then `RemoveObject`) and adds the new wall when a game starts. Each brick is a root object with a `Sprite` child and a `BoxCollider` child.

A parent-group object for the wall (so bricks share a transform and can be shown or hidden together) is possible now that hierarchy exists, but is not used yet.

## Application Boundary

`Application` owns generic draw execution: it clears `Scene.BackgroundColor` and renders `Scene.Objects` with its shared `RenderSystem`. `BreakoutScene` has no game-specific draw hook.

## Deferred

- Reparenting at runtime (for example the ball becoming a child of the paddle while serving), with an option to keep its world position.
- Scene serialization and loading the graph from a file. The object model was chosen so a saved object is a type name plus its public members.
- Replacing the procedural `BrickWallObjects` with scene data.
- Restart by reloading the scene instead of rebuilding the wall.

## Verification

Narrow compile check:

```bash
dotnet build breakout_cs.csproj --no-restore
```

Manual runtime check:

- scene starts with paddle, ball, bricks, HUD, and overlay
- ball serves from paddle
- launch works
- paddle collision works
- brick collision destroys bricks
- losing a life resets paddle and ball
- win/loss restart still works
