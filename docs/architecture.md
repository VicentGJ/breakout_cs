# Architecture

## Overview

This codebase is split into a small engine layer and a Breakout-specific game layer. The engine layer lives under `Engine/` and provides the runtime loop, scene and object model, generic component storage, object-owned behavior lifecycle, a scene-scoped event bus, rendering components, and box-collision utilities. The game layer lives under `Game/` and contains Breakout rules, content tuning, gameplay object modules, scene bootstrap, and final renderer composition.

The core execution shape is now:

`Program -> Application -> Runtime -> Scene -> GameObjects -> BehaviorComponents`

`Runtime` owns frame phase ordering only. It does not know Breakout systems or concrete gameplay behavior types. `Scene` owns objects and events. `GameObject` owns passive data components and active behavior components. Behavior components own object-local gameplay logic and communicate scene-level facts through typed events.

## Process Model

Execution starts in `Program.cs`, which creates a `BreakoutScene` with the window dimensions and passes it into `Runtime`. `Application` is the outer loop wrapper around Raylib. It opens the window, sets the target FPS, and runs a four-stage frame cycle: `BeginFrame`, `Update`, `PhysicsUpdate`, and `Draw`.

`Runtime` owns `RuntimeTime` and the active scene. Each frame it updates time, calls the scene hook, dispatches behavior `Update`, calls the scene physics hook, dispatches behavior `PhysicsUpdate`, and then asks the scene to draw. Runtime does not poll Breakout input, reset rounds, handle collisions, score bricks, or inspect game phases. Runtime-adjacent infrastructure such as `EventBus` also lives under `Engine/Runtime`.

`BreakoutScene` polls input in its scene update hook, clears the frame background, invokes the generic `RenderSystem`, and exposes Breakout state used by behaviors. Gameplay phase gates now live inside behavior components.

## Scene, Object, And Behavior Model

`Scene` is the base container for world objects. It owns object membership, assigns IDs, exposes a scene-scoped `EventBus`, and dispatches behavior lifecycle hooks. Objects can be created through the protected scene helper or added with `AddObject`.

`GameObject` is the unit of composition. It stores a name, ID, active flag, `Transform2D`, passive data components, and behavior components. Passive components are structs keyed by component type and accessed with `Add`, `Set`, `Has`, `Get`, and `TryGet`.

`BehaviorComponent` is the executable component base. It is a class with reference semantics, an `Enabled` flag, and owner/scene references assigned on attachment. The lifecycle is deterministic:

1. `OnAttach` when the behavior joins a scene-owned object.
2. `Start` once before the first phase callback.
3. `Update(float dt)` during normal frame logic.
4. `PhysicsUpdate(float dt)` during physics-phase logic.
5. `OnDetach` when the object leaves the scene.

Objects keep passive data and behaviors in separate storage. Inactive objects do not run behaviors. Disabled behaviors do not run even when the owner object is active.

## Event Bus

Each scene owns one typed synchronous `EventBus`. Events are plain structs implementing `ISceneEvent`. Subscriptions are created with `Subscribe<TEvent>(Action<TEvent>)` and released by disposing the returned `IDisposable`.

Events represent facts that already happened, not commands. Breakout currently publishes:

- `BallLaunchedEvent`
- `BallOutOfBoundsEvent`
- `BrickDestroyedEvent`
- `RoundStartedEvent`

This keeps communication decoupled where useful. For structural relationships, direct references remain valid. For example, ball behavior reads the paddle and brick list from `BreakoutScene`, while score/life state reacts to published facts.

## Rendering

Rendering remains component-driven. `Engine/Rendering/RenderSystem.cs` gathers visible active objects, picks the first renderer component it understands, sorts entries by `CanvasItemComponent.ZIndex`, and draws them in order.

`CanvasItemComponent` gates rendering and carries modulation color. Shape-specific renderer components describe visual data. `RectangleRendererComponent`, `CircleRendererComponent`, and `TextRendererComponent` are handled by corresponding renderer handlers.

Breakout presentation is modeled as scene objects. The arena, HUD text, and overlay panel/text use regular renderer components, while `PresentationBehavior` updates score, lives, text alignment, and overlay visibility from scene state.

## Physics And Collision

The collision math stays generic. `BoxColliderComponent` stores local box size, and `BoxCollision.GetCollision` combines that size with transforms to produce a signed `Collision` result. `CollisionHelper.GetCorrection` turns negative-distance collisions into separation vectors.

Breakout collision meaning now lives in object behavior. `BallBehavior` moves the ball, resolves arena wall collision, resolves paddle collision, checks brick collision, and publishes `BallOutOfBoundsEvent` when the ball leaves the arena. `BrickBehavior` owns brick destruction and publishes `BrickDestroyedEvent`.

The split remains: engine physics answers geometric questions; game behavior decides gameplay meaning.

## Breakout Game Flow

Game object modules live under `Game/Components/<ObjectName>/`. Each module keeps the object's data component and behavior together when both exist, such as `Game/Components/Ball/BallComponent.cs` and `Game/Components/Ball/BallBehavior.cs`.

Game event definitions live under `Game/Runtime`, beside other game flow primitives rather than in a standalone events module.

`BreakoutScene` stores screen dimensions, arena rectangle, input snapshot, phase, score, lives, remaining brick count, and direct references to the paddle, ball, and bricks. It also creates a dedicated `GameState` object with `RoundBehavior`.

`RoundBehavior` owns round and game-state flow. On start it creates a new game through `LevelBuilder`, resets core objects through the scene, and enters serving phase. It subscribes to brick and ball events. Brick destruction updates score and win state. Ball out-of-bounds reduces lives, resets the round, or enters lost state. In won/lost phases, confirm input starts a fresh game.

`PaddleBehavior` owns paddle movement during serving and playing phases.

`BallBehavior` owns serving attachment, launch, movement, collision response, and ball-out event publishing.

`BrickBehavior` owns local brick destruction and render visibility.

`PresentationBehavior` owns HUD text updates, controls text alignment, and phase overlay visibility.

`LevelBuilder` remains a content service. It clears old bricks, lays out the brick wall, creates brick objects, and records the remaining brick count.

## Data And Tuning

Game tuning lives in `BreakoutRules`. Visual tuning lives in `BreakoutTheme`. Behavior components use these constants directly instead of scattering magic numbers through gameplay code.

## Current Boundaries

The engine is still intentionally small. There is no scene serialization, object hierarchy, broadphase physics, rigid-body simulation, animation system, behavior priority ordering, queued event delivery, global event bus, or editor-facing scripting model.

The most important boundary remains engine versus game code. `Engine/` provides generic lifecycle, events, rendering, scene/object storage, and box-collision helpers. `Game/` owns Breakout rules, object behavior, scene state, level content, and presentation.
