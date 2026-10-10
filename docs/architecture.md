# Architecture

## Overview

This codebase is split into a small engine layer and a Breakout-specific game layer. The engine layer lives under `Engine/` and provides the application loop, the scene and object tree, a scene-scoped event bus, drawable objects, a box collider, and an audio player. The game layer lives under `Game/` and contains Breakout rules, content tuning, gameplay object classes, and scene bootstrap.

The core execution shape is:

`Program -> Application -> active Scene -> GameObject tree`

`Application` owns the Raylib window loop, active scene, runtime time state, frame phase ordering, and generic render execution. It does not know Breakout types. `Scene` owns objects, background configuration, and events. Every object inherits `GameObject`; game classes own their logic and communicate scene-level facts through typed events.

## Process Model

Execution starts in `Program.cs`, which creates a `BreakoutScene` with the window dimensions and passes it into `Application`. `Application` opens the window, sets the target FPS, owns `RuntimeTime` and the active scene, and runs a four-stage frame cycle: `BeginFrame`, `Update`, `PhysicsUpdate`, and `Draw`.

Each frame `Application` updates time, calls the scene hook, runs `Start`/`Update` on scene objects, calls the scene physics hook, runs `PhysicsUpdate` on scene objects, clears the scene background, and calls `RenderSystem.Draw(ActiveScene.Objects)`. It does not poll Breakout input, reset rounds, handle collisions, or inspect game phases.

`BreakoutScene` sets its background color, polls input in its scene update hook, and exposes Breakout context (screen size, arena, input) used by game objects.

## Object Model

The model is Godot-style: typed subclasses of one base class, with reuse coming from children in a tree instead of attached components.

`GameObject` is the base class. It stores a name, ID, `Active`, `Visible`, `ZIndex`, `Modulate`, a local `Transform2D`, a `Parent`, and a list of `Children`. It has virtual hooks: `OnAttach`, `OnDetach`, `Start`, `Update`, `PhysicsUpdate`, and an internal `Draw`.

Engine object types:

- `Sprite`, `Label`, `RectangleShape`, `CircleShape` draw themselves.
- `BoxCollider` is a collision box.
- `AudioPlayer` plays a sound.

Game object types combine them as children. `Paddle` is a `GameObject` with a `RectangleShape` child and a `BoxCollider` child. `Ball` has a `CircleShape`, a `BoxCollider`, and an `AudioPlayer`. `Brick` has a `Sprite` and a `BoxCollider`. An empty `GameObject` with children is a group.

### Hierarchy

- `Transform2D` is local, relative to the parent. For root objects local equals world. `GetWorldTransform()` composes position, rotation, and scale through the ancestors. Rendering and collision use world transforms.
- `Active` and `Visible` are inherited: `ActiveInHierarchy` and `VisibleInHierarchy` are the object's own flag AND every ancestor's.
- `Scene.AddObject` adds a root with its whole subtree; `AddChild` on an attached parent registers the child too. `Scene.RemoveObject` removes the object with its whole subtree and runs `OnDetach` for each. The scene keeps one flat `Objects` list with parents before children.
- Not implemented yet: reparenting, preserving world position on reparent.

### Lifecycle

1. `OnAttach` when the object joins a scene. It touches only the object itself and scene events.
2. `Start` once, at the beginning of the next update, after every object that was loaded is in the scene. It may look at the rest of the world with `Scene.Find<T>()`. Order between objects is undefined.
3. `Update` and `PhysicsUpdate` each frame, for objects that are active in the hierarchy.
4. `OnDetach` when the object leaves the scene.

### Reaching other objects

An object stores only its own children. Everything else is found by type when needed (`Scene.Find<T>()`, `Scene.FindAll<T>()`, linear scan, call from `Start`), or arrives through events. Saving a reference to another object in a field is avoided on purpose, so scene data never needs object-to-object links.

## Event Bus

Each scene owns one typed synchronous `EventBus`. Events are plain structs implementing `ISceneEvent`. Subscriptions are created with `Subscribe<TEvent>(Action<TEvent>)` and released by disposing the returned `IDisposable`.

Events represent facts that already happened, not commands. Breakout currently publishes `BallLaunchedEvent`, `BallOutOfBoundsEvent`, `BrickDestroyedEvent`, and `RoundStartedEvent`.

## Rendering

`Application` clears `ActiveScene.BackgroundColor` and owns one `RenderSystem`. It gathers objects that are active and visible in the hierarchy, pairs each with its world transform, sorts by `ZIndex`, and calls each object's `Draw`. Drawable object types implement `Draw` themselves; objects that draw nothing inherit the empty default.

`Modulate` is per object and is not inherited. `RenderSystem` owns the texture cache and the color modulation helper.

## Physics And Collision

The collision math stays generic. `BoxCollider` stores a local box size. `BoxCollision.GetCollision` combines it with world transforms to produce a signed `Collision` result. `CollisionHelper.GetCorrection` turns negative-distance collisions into separation vectors.

Breakout collision meaning lives in game classes. `Ball` moves, resolves arena walls, the paddle, and bricks, and publishes `BallOutOfBoundsEvent`. `Brick` owns its destruction and publishes `BrickDestroyedEvent`.

The split remains: engine physics answers geometric questions; game code decides gameplay meaning.

## Breakout Game Flow

Game classes live under `Game/Objects/<Name>/`. Event definitions live under `Game/Runtime`.

`BreakoutScene` stores screen dimensions, the arena rectangle, and the input snapshot. Its `Setup` creates the `GameState`, `Paddle`, `Ball`, arena rectangle, and `Hud`, and adds them to the scene.

`GameState` holds phase, score, lives, and remaining bricks, and runs round flow. In `Start` it finds the paddle and ball, builds the brick wall with `BrickWallObjects`, and enters serving phase. It subscribes to brick and ball events. Other objects read it with `Scene.Find<GameState>()`.

`Paddle` moves during serving and playing phases. `Ball` handles serving attachment, launch, movement, collision response, and ball-out event publishing. `Brick` handles its own destruction. `Hud` updates score, lives, controls text, and the phase overlay.

## Data And Tuning

Game tuning lives in `BreakoutRules`. Visual tuning lives in `BreakoutTheme`.

## Current Boundaries

There is no scene serialization, reparenting, broadphase physics, rigid-body simulation, animation system, event queueing, or editor-facing model. Scene switching (`Application.ChangeScene`) is only half built: it does not detach the outgoing scene's objects, and a scene cannot request a change.

The most important boundary remains engine versus game code. `Engine/` provides the generic object tree, lifecycle, events, drawable objects, collider, and audio. `Game/` owns Breakout rules, object behavior, scene state, level content, and presentation.
