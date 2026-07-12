# Architecture

## Overview

This codebase is split into a small engine layer and a Breakout-specific game layer. The engine layer lives under `Engine/` and provides the application loop, scene and object model, generic component storage, object-owned behavior lifecycle, a scene-scoped event bus, rendering components, and box-collision utilities. The game layer lives under `Game/` and contains Breakout rules, content tuning, gameplay object modules, scene bootstrap, and final renderer composition.

The core execution shape is now:

`Program -> Application -> Scene -> GameObjects -> Components`

`Application` owns the Raylib window loop, active scene, runtime time state, and frame phase ordering. It does not know Breakout systems or concrete gameplay component types. `Scene` owns objects and events. `GameObject` owns components. Every component inherits `Component`; game components own object-local logic and communicate scene-level facts through typed events.

## Process Model

Execution starts in `Program.cs`, which creates a `BreakoutScene` with the window dimensions and passes it into `Application`. `Application` is the outer loop wrapper around Raylib. It opens the window, sets the target FPS, owns `RuntimeTime` and the active scene, and runs a four-stage frame cycle: `BeginFrame`, `Update`, `PhysicsUpdate`, and `Draw`.

Each frame `Application` updates time, calls the scene hook, dispatches `IUpdatable` components, calls the scene physics hook, dispatches `IPhysicsUpdatable` components, and then asks the scene to draw. It does not poll Breakout input, reset rounds, handle collisions, score bricks, or inspect game phases. Runtime-adjacent infrastructure such as `EventBus` still lives under `Engine/Runtime`.

`BreakoutScene` polls input in its scene update hook, clears the frame background, invokes the generic `RenderSystem`, and exposes Breakout state used by game components. Gameplay phase gates live inside those components.

## Scene, Object, And Behavior Model

`Scene` is the base container for world objects. It owns object membership, assigns IDs, exposes a scene-scoped `EventBus`, and dispatches component lifecycle hooks. Objects are added with `AddObject`.

`GameObject` is the unit of composition. It stores a name, ID, active flag, `Transform2D`, and one component of each type. All components inherit `Component` and use the same `Add`, `Has`, `Get`, and `TryGet` API. Components are reference objects, so changes to retrieved component data apply directly.

`Component` is the common component base. It is a class with an `Enabled` flag and owner/scene references assigned on attachment. The lifecycle is deterministic:

1. `OnAttach` when the component joins a scene-owned object.
2. `IStartable.Start()` once during the next normal update phase, when implemented.
3. `IUpdatable.Update(float dt)` during normal frame logic, when implemented.
4. `IPhysicsUpdatable.PhysicsUpdate(float dt)` during physics-phase logic, when implemented.
5. `OnDetach` when the object leaves the scene.

`IStartable`, `IUpdatable`, and `IPhysicsUpdatable` opt a component into the corresponding lifecycle or frame phase. Inactive objects do not run these interfaces. Disabled components do not run even when the owner object is active.

## Event Bus

Each scene owns one typed synchronous `EventBus`. Events are plain structs implementing `ISceneEvent`. Subscriptions are created with `Subscribe<TEvent>(Action<TEvent>)` and released by disposing the returned `IDisposable`.

Events represent facts that already happened, not commands. Breakout currently publishes:

- `BallLaunchedEvent`
- `BallOutOfBoundsEvent`
- `BrickDestroyedEvent`
- `RoundStartedEvent`

This keeps communication decoupled where useful. For structural relationships, direct references remain valid. For example, ball behavior receives the paddle and brick list during scene setup, while score/life state reacts to published facts.

## Rendering

Rendering remains component-driven. `Engine/Rendering/RenderSystem.cs` gathers visible active objects, picks the first renderer component it understands, sorts entries by `GameObject.ZIndex`, and draws them in order.

`GameObject` carries shared render state: visibility, draw order, and modulation color. Shape-specific renderer components describe visual data. `RectangleRendererComponent`, `CircleRendererComponent`, and `TextRendererComponent` are handled by corresponding renderer handlers.

Breakout presentation is modeled as scene objects. The arena, HUD text, and overlay panel/text use regular renderer components, while `PresentationBehavior` updates score, lives, text alignment, and overlay visibility from scene state.

## Physics And Collision

The collision math stays generic. `BoxColliderComponent` stores local box size, and `BoxCollision.GetCollision` combines that size with transforms to produce a signed `Collision` result. `CollisionHelper.GetCorrection` turns negative-distance collisions into separation vectors.

Breakout collision meaning now lives in object behavior. `BallBehavior` moves the ball, resolves arena wall collision, resolves paddle collision, checks brick collision, and publishes `BallOutOfBoundsEvent` when the ball leaves the arena. `BrickBehavior` owns brick destruction and publishes `BrickDestroyedEvent`.

The split remains: engine physics answers geometric questions; game behavior decides gameplay meaning.

## Breakout Game Flow

Game object modules live under `Game/Objects/<ObjectName>/`. Each module keeps the object's data component, behavior, and construction recipe together when applicable, such as `Game/Objects/Ball/BallComponent.cs`, `BallBehavior.cs`, and `BallObject.cs`.

Game event definitions live under `Game/Runtime`, beside other game flow primitives rather than in a standalone events module.

`BreakoutScene` stores screen dimensions, arena rectangle, input snapshot, phase, score, lives, and remaining brick count. Its setup method creates a dedicated `GameState` object with `RoundBehavior`, then wires paddle, ball, and the shared brick list directly into the behaviors that need them.

`RoundBehavior` owns round and game-state flow. On start it creates a new game through `LevelBuilder`, resets its wired paddle and ball references, and enters serving phase. It subscribes to brick and ball events. Brick destruction updates score and win state. Ball out-of-bounds reduces lives, resets the round, or enters lost state. In won/lost phases, confirm input starts a fresh game.

`PaddleBehavior` owns paddle movement during serving and playing phases.

`BallBehavior` owns serving attachment, launch, movement, collision response, and ball-out event publishing.

`BrickBehavior` owns local brick destruction and render visibility.

`PresentationBehavior` owns HUD text updates, controls text alignment, and phase overlay visibility.

`LevelBuilder` remains a content service. It clears old bricks, lays out the brick wall, creates brick objects, and records the remaining brick count.

## Data And Tuning

Game tuning lives in `BreakoutRules`. Visual tuning lives in `BreakoutTheme`. Game components use these constants directly instead of scattering magic numbers through gameplay code.

## Current Boundaries

The engine is still intentionally small. There is no scene serialization, object hierarchy, broadphase physics, rigid-body simulation, animation system, behavior priority ordering, queued event delivery, global event bus, or editor-facing scripting model.

The most important boundary remains engine versus game code. `Engine/` provides generic lifecycle, events, rendering, scene/object storage, and box-collision helpers. `Game/` owns Breakout rules, object behavior, scene state, level content, and presentation.
