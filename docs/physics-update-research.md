# Physics Update Research and Current Direction

## Goal

Understand where a scene-based engine should run physics and choose a small shape that fits Breakout.

Engines reviewed: Godot, Wicked Engine, jMonkeyEngine, and Irrlicht.

## Research Outcome

The engines converge on a few useful ideas:

- the window loop belongs to the application layer;
- physics is a distinct phase from normal update;
- fixed-step physics is common when deterministic simulation matters;
- scene or world state supplies data, while an engine-owned loop decides phase order.

Breakout adopts the first three ideas in a deliberately small form. It has distinct update and physics phases, but physics currently uses the render-frame delta time. That is sufficient for the present box-collision gameplay.

## Current Architecture

The project briefly used a separate `Runtime` object, then removed it. `Application` now owns the active scene, `RuntimeTime`, scene changes, and frame ordering. A separate wrapper would only forward those calls today.

```text
Program
  -> Application
      -> active Scene
          -> scene hooks
          -> GameObjects
              -> Components
```

For each frame, `Application` updates time, calls the scene update hook, dispatches lifecycle component `Update`, calls the scene physics hook, dispatches lifecycle component `PhysicsUpdate`, and draws the scene. It does not know Breakout gameplay types or interpret gameplay events.

`Scene` owns object membership and scene-scoped events. `BreakoutScene` supplies Breakout context, input polling, and rendering. Lifecycle components own paddle movement, ball motion, collision response, and round flow.

## Physics Boundary

Collision geometry stays in `Engine/Physics`. Game behavior decides what a collision means: wall bounce, paddle bounce, brick destruction, scoring, and losing a ball. This keeps generic math reusable without introducing a general physics-system abstraction before the game needs one.

## Fixed-Step Upgrade Path

If variable-step physics causes a real gameplay or determinism problem, add an accumulator to `Application` and run `PhysicsUpdate` zero or more times per render frame.

```csharp
accumulator += Time.UnscaledDeltaTime * Time.TimeScale;

while (accumulator >= fixedDelta)
{
    PhysicsUpdate(fixedDelta);
    accumulator -= fixedDelta;
}

Update(Time.UnscaledDeltaTime * Time.TimeScale);
Draw();
```

That change belongs in `Application`; it does not require reintroducing `Runtime`.

## Decision

Do not add a separate `Runtime` layer without a concrete responsibility that cannot live clearly in `Application`. The next justified runtime change is fixed-step accumulation, and only when the game demonstrates the need.
