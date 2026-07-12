# Scene Setup and Explicit Object Wiring Spec

## Purpose

This document records the implemented scene setup and object-relationship direction.

The refactor moved object construction into focused object recipes and scene setup. Moving their relationships into a generic lookup API would only hide the coupling. The ball really does depend on the paddle. The round flow really does reset the paddle, ball, and brick wall.

The goal is to make those relationships explicit at setup time:

`Scene setup -> object recipes -> wired behavior dependencies -> runtime phases`

The scene remains the generic owner of game objects. Game-specific relationships are wired once when the scene is built.

## Design Goals

- `Scene` owns the object list, events, and lifecycle dispatch.
- `BreakoutScene` keeps Breakout-level context: screen size, arena, phase, score, lives, input, draw hook.
- Game objects are created during scene setup, not hidden inside unrelated runtime methods.
- Behaviors receive direct references to objects or small collections they actually depend on.
- Object recipes remain responsible for constructing objects and attaching components/behaviors.
- No broad scene lookup API as the main relationship mechanism.
- No object registry that simply recreates scene coupling under another name.

## Non-Goals

- Do not introduce a full ECS.
- Do not add global component queries for gameplay relationships.
- Do not add string path/name lookup until there is a scene graph or editor reason.
- Do not add entity IDs/handles when `GameObject` references already provide stable runtime handles.
- Do not add parent/child transforms yet.

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

`Application` installs scenes through `ChangeScene`, which calls `Setup()` exactly once per scene instance. The constructor uses the same path.

Setup is for initial object graph creation only. It should create objects, attach components, attach behaviors, and build initial content. It should not run per-frame logic.

`BreakoutScene.Setup()` should create the initial Breakout graph in dependency order:

1. game state object shell, so its behavior runs first
2. shared brick list
3. paddle
4. ball, wired to paddle and brick list
5. round behavior, attached to the game state object with paddle, ball, and brick list refs
6. presentation objects

The constructor should focus on immutable scene context such as screen size and arena bounds.

## Explicit References

Use direct `GameObject` references when one object has a real, stable dependency on another object.

Example: before launch, the ball needs the paddle position so it can stick to it. That is not accidental coupling; it is the rule of Breakout.

```csharp
GameObject gameState = GameStateObject.Create(scene);
List<GameObject> bricks = [];
GameObject paddle = PaddleObject.Create(scene);
GameObject ball = BallObject.Create(scene, paddle, bricks);
GameStateObject.AddRoundBehavior(gameState, paddle, ball, bricks);
```

`BallBehavior` should receive its dependencies through its constructor.

```csharp
sealed class BallBehavior : BehaviorComponent
{
    private readonly GameObject _paddle;
    private readonly IReadOnlyList<GameObject> _bricks;

    public BallBehavior(GameObject paddle, IReadOnlyList<GameObject> bricks)
    {
        _paddle = paddle;
        _bricks = bricks;
    }
}
```

This makes runtime access cheap and visible. There is no search, no name contract, and no global registry.

## Brick List

Bricks are not a single stable object. They are a dynamic group, but they do not yet need a separate domain object.

A standalone `BrickWall` class would mostly be a named list right now. That adds a concept without enough behavior to justify it. Use a setup-wired shared list instead.

```csharp
List<GameObject> bricks = [];

GameObject ball = BallObject.Create(scene, paddle, bricks);
GameObject gameState = GameStateObject.Create(scene, paddle, ball, bricks);
```

This keeps the real relationship visible:

- `RoundBehavior` mutates the list when rebuilding the level.
- `BallBehavior` reads the list for brick collision.
- `BreakoutScene` does not own the list.
- No extra wall abstraction exists until the wall earns behavior/state of its own.

`LevelBuilder` should build into the shared brick list:

- clear old bricks
- create new brick objects through `BrickObject.Create`
- add them to `bricks`
- return/count created bricks for `BricksRemaining`

## Round Flow

`RoundBehavior` owns round lifecycle.

It should receive the references it needs when the game state object is created:

```csharp
GameObject gameState = GameStateObject.Create(
    scene,
    paddle,
    ball,
    bricks);
```

`RoundBehavior` should:

- start a new game
- reset score/lives
- clear and rebuild bricks through `LevelBuilder`
- reset paddle and ball from direct references
- update score and remaining brick count from events

Resetting paddle and ball means:

- set active state
- reset transforms
- reset ball velocity and previous position

This logic belongs in round flow because it is round lifecycle behavior, not scene ownership.

## Ball Collision

`BallBehavior` should not look up the paddle or scan the scene for bricks.

It should:

- use `_paddle` while serving
- use `_paddle` for paddle collision
- iterate `_bricks` for brick collision
- ignore inactive bricks

This keeps object relationships explicit and avoids both broad scene queries and hidden registries.

## Scene Responsibilities

After this refactor, remove these direct gameplay members from `BreakoutScene`:

- `Bricks`
- `PaddleObject`
- `BallObject`
- `ClearBricks`
- `ResetCoreObjects`
- `CreateBrickObject`

`BreakoutScene` should keep:

- `ScreenWidth`
- `ScreenHeight`
- `Arena`
- `Phase`
- `Score`
- `Lives`
- `BricksRemaining`
- `Input`
- `Update`
- `Draw`

Behaviors may still cast `Scene` to `BreakoutScene` for Breakout context: arena bounds, phase, input, score, lives, and remaining brick count.

## Naming

`Game/Objects` remains the right module name.

Object recipe classes should stay grouped by object type:

- `Game/Objects/Paddle/PaddleObject.cs`
- `Game/Objects/Ball/BallObject.cs`
- `Game/Objects/Brick/BrickObject.cs`
- `Game/Objects/Presentation/PresentationObjects.cs`
- `Game/Objects/GameState/GameStateObject.cs`

Data structs keep `*Component` names because they are still components.

Use a plain `List<GameObject>` for the brick group until the wall needs real behavior or state.

## Deferred Options

Parent/child transforms are a good future fit for serving.

If the engine later supports hierarchy, the ball could become a child of the paddle while serving, then detach on launch while preserving world position. Do not add that now; it requires local/world transform rules, detach behavior, and render/physics transform decisions.

Scene path/name lookup may become useful when scenes are authored externally or in an editor. Do not add it now.

Component queries may still be useful later for systems such as rendering, physics broadphase, or editor tooling. Do not use them as the primary gameplay relationship API now.

Promoting the brick list into a `BrickWall` object may make sense later if the wall gains behavior such as patterns, animations, layout state, damage rules, or object hierarchy. Do not add it as a named list now.

## Implementation Status

This design is implemented. `BreakoutScene.Setup()` creates the game-state object, shared brick list, paddle, ball, and presentation objects. `BallBehavior` and `RoundBehavior` receive their required references during setup, and `LevelBuilder` builds into the shared list.

## Verification

Run narrow compile check:

```bash
dotnet build breakout_cs.csproj --no-restore
```

Search for removed scene coupling:

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

- Should object recipes keep calling `scene.AddObject`, or should they eventually return unattached objects so setup controls object order directly?
