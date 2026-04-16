# Physics Update Research

## Goal

Understand how object-oriented, scene-based engines place physics updates in the runtime loop, with source-level evidence where possible.

Engines under review:

- Godot
- Wicked Engine
- jMonkeyEngine
- Irrlicht

## Questions

- Is physics stepped at the app loop level, scene/world level, or object/script level?
- Is there a distinct physics phase separate from the regular update?
- Is the physics phase fixed-step or variable-step?
- How much of the ordering is owned by the engine core vs the active scene/world?
- What lessons fit this codebase?

## Notes

This document is intentionally iterative while research is in progress.

## What the engines say

The engines reviewed converge on the same broad shape:

- outer loop belongs to the engine/app/window layer
- physics is a distinct phase
- the phase is owned by scene/world/runtime state, not by a top-level app wrapper directly
- fixed-step physics is common

Examples:

- Godot: engine schedules fixed ticks, `SceneTree` dispatches physics callbacks
- Wicked Engine: app loop exists, but physics runs as a world/scene system phase
- jMonkeyEngine: physics is commonly an attachable app-state subsystem
- Irrlicht: no built-in world physics phase; physics is explicit user/runtime code

## What this means for us

Putting physics on `BreakoutScene` bakes behavior into the scene object and conflicts with a future serialized-scene model.

The better long-term split is:

- `SceneData` or `SceneDefinition`: serializable content only
- `Runtime`: owns update ordering and phase execution
- `Systems`: operate on loaded scene state

Physics belongs in `Runtime`, not in serialized scene data.

## Recommended shape

```csharp
GameApp
  -> Runtime
      -> SceneData / loaded world state
      -> Update systems
      -> Physics systems
      -> Render systems
```

At the code level, this means:

- keep `GameApp` as the outer window loop
- add a `Runtime` layer that owns phase ordering
- keep scenes as data/loaded state, not as the place where all behavior lives
- treat physics as one runtime phase among others

## Impact on physics

The physics module itself should stay scene-agnostic.

It should operate on engine objects/components/transforms, so it still works if the scene comes from:

- code
- file data
- editor export
- tests

This keeps the runtime model stable if scene content later comes from serialized data.

## Fixed-Step Upgrade Path

If later you want a stronger physics model, evolve `Runtime` to accumulate real time and dispatch physics multiple times per render frame.

Example shape:

```csharp
accumulator += Time.UnscaledDeltaTime * Time.TimeScale;

while (accumulator >= fixedDelta)
{
    Time.DeltaTime = fixedDelta;
    _runtime.PhysicsUpdate();
    accumulator -= fixedDelta;
}

Time.DeltaTime = Time.UnscaledDeltaTime * Time.TimeScale;
    _runtime.Update();
    _runtime.Draw();
```

That would move this engine closer to Godot on fixed-step ownership while keeping the physics phase owned by runtime.

## Recommendation

Best next step for this project:

- introduce a `Runtime` layer that owns update, physics, and draw phases
- keep scenes as data / loaded state, not behavior owners
- move collision resolution into runtime-owned physics execution
- add fixed-step accumulation in `Runtime` only when physics needs determinism

This keeps the current engine small, matches the direction of Godot/Wicked/jME more than Irrlicht, and avoids baking behavior into scene objects.

## Refactor Scope

If the target shape is:

- `GameApp` becomes `Application`
- `GameScene.cs` is removed
- a new `Scene.cs` defines the abstract data-oriented scene shape
- `BreakoutScene` becomes one concrete scene definition / loaded scene state

then the refactor is not only a rename. The surrounding runtime ownership must change too.

### 1. Application layer

Current:

- `Engine/Runtime/GameApp.cs`
- `Program.cs` constructs `GameApp` directly with `BreakoutScene`

Target:

- `Engine/Runtime/Application.cs`
- owns window creation, app lifetime, and outer frame loop only
- delegates execution to `Runtime`

This means `Application` should no longer own a scene manager directly.

### 2. Runtime layer

This is the missing piece that becomes necessary once `GameApp` is reduced to basic app setup.

New responsibility:

- active scene ownership
- phase ordering
- update / physics / draw dispatch
- time-step policy later if fixed-step is introduced

Likely new file:

- `Engine/Runtime/Runtime.cs`

Without this layer, behavior would fall back into `Scene`, which is exactly what this refactor is trying to avoid.

### 3. Scene abstraction

Current:

- `Engine/Runtime/GameScene.cs` defines `IGameScene` with `Update()` and `Draw()`

Target:

- remove `GameScene.cs`
- add `Engine/Scene/Scene.cs`

The new `Scene` should describe scene structure and loaded state, not execution ownership.

At minimum it should own:

- object collection
- object creation / IDs
- scene-level data containers
- lookup helpers needed by systems

It should not own:

- input handling
- gameplay execution order
- physics stepping
- render phase orchestration

### 4. BreakoutScene evolution

Current `Game/Scenes/BreakoutScene.cs` is doing three jobs at once:

- scene data container
- scene bootstrap / object creation
- behavior orchestration

The first two can stay. The third should move out.

What should remain in `BreakoutScene`:

- Breakout-specific scene data
- arena dimensions
- score / lives / phase state if they are considered world state
- object references such as paddle, ball, bricks
- object factory helpers like `CreateGameObject()`
- setup/reset helpers like `ResetCoreObjects()` and `CreateBrickObject()`

What should move out of `BreakoutScene`:

- `Update()`
- `Draw()`
- direct calls to `_paddleSystem`, `_ballSystem`, `_collisionSystem`, `_roundSystem`, `_renderer`
- ownership of execution order

This is the biggest concrete change required to make the refactor coherent.

### 5. System ownership

Right now `BreakoutScene` constructs and owns:

- `BreakoutRenderer`
- `PaddleSystem`
- `BallSystem`
- `CollisionSystem`
- `RoundSystem`

That is behavior ownership, so it should move to runtime or a game-specific runtime composition root.

Possible target shape:

- `Runtime` owns generic engine phases
- a Breakout-specific runtime setup owns the concrete system instances

Examples:

- `Game/Runtime/BreakoutRuntime.cs`
- or `Runtime` plus a smaller `BreakoutMode` / `BreakoutSession` object

The important part is that systems stop being owned by the scene object.

### Alternative: scene update hook

There is a valid middle ground:

- `Runtime` still owns phase ordering
- the active scene can expose an `Update()` hook
- `Runtime` calls that hook at the appropriate point in the frame

That can work well for scene-specific gameplay logic, as long as the responsibility split stays clear.

Good use of `Scene.Update()`:

- scene-specific rules
- mission/level scripting
- one-off gameplay coordination that does not belong in reusable engine systems

Bad use of `Scene.Update()`:

- owning the full frame order
- directly acting as the physics phase owner
- constructing and owning all systems
- becoming the place where all gameplay logic accumulates again

So the distinction is:

- `Runtime` decides when update, physics, and draw happen
- `Scene.Update()` can provide scene-specific behavior inside that runtime-owned sequence

This keeps the architecture closer to established engines:

- runtime owns orchestration
- world/scene can still inject scene-specific logic

If this route is taken, then `Scene.Update()` should be treated as a hook inside runtime, not as a replacement for runtime.

### 6. Rendering side

Current draw flow is scene-owned:

- `BreakoutScene.Draw()`
- `BreakoutRenderer.Draw()`

Target:

- runtime owns draw phase
- renderer consumes scene state

This means `BreakoutRenderer` should accept a scene/world reference but not be called by the scene itself.

### 7. Physics side

Physics integration should follow the same rule:

- the physics module stays engine-side
- collision execution belongs to runtime-owned system dispatch
- scene only exposes data that systems operate on

This is what keeps the design aligned with future serialized scenes.

### 8. Program entry point

`Program.cs` will also need to change.

Current:

- create `GameApp`
- pass `BreakoutScene` directly

Target:

- create `Application`
- create `Runtime`
- create/load `BreakoutScene`
- bind them together explicitly

That makes the final ownership graph clearer.

## Minimal coherent end state

```csharp
Program
  -> Application
      -> Runtime
          -> active Scene
          -> systems
          -> renderer
```

Where:

- `Application` owns OS/window/frame lifetime
- `Runtime` owns execution
- `Scene` owns data/state
- systems and renderer operate on the scene

## Practical implication

If you make only the renames:

- `GameApp` -> `Application`
- `GameScene.cs` -> `Scene.cs`

but leave update/draw/system ownership inside `BreakoutScene`, the architecture will still fundamentally be scene-driven behavior.

To make the refactor comprehensive, the real step is:

- extract behavior ownership out of `BreakoutScene`
- introduce `Runtime`
- let `Scene` become the data/world container
