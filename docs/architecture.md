# Architecture

## Overview

This codebase is split into a small engine layer and a Breakout-specific game layer. The engine layer lives under `Engine/` and provides the runtime loop, the scene and object model, the generic component store, the rendering pipeline, and the box-collision utilities. The game layer lives under `Game/` and contains Breakout rules, content tuning, gameplay components, systems, the scene bootstrap, and the final renderer composition.

The shape is intentionally simple. `Program` creates a `Runtime`, `Runtime` owns the active scene and frame-phase ordering, `Scene` owns objects, and systems operate on scene state. Rendering is also split in two layers: a generic `RenderSystem` knows how to draw components, while `BreakoutRenderer` decides what the frame should contain and how the HUD and overlays should look.

## Process Model

Execution starts in `Program.cs`, which creates a `BreakoutScene` with the window dimensions and passes it into `Runtime`. `Application` is the outer loop wrapper around Raylib. It opens the window, sets the target FPS, and then runs a four-stage frame cycle: `BeginFrame`, `Update`, `PhysicsUpdate`, and `Draw`.

`Runtime` is the orchestration point. It owns `RuntimeTime`, the active scene, and the game systems used by Breakout. Each frame it polls input once, updates the active scene, runs gameplay input and movement logic, runs the physics phase when the game is in the playing state, and then hands drawing to the renderer. The runtime is also the place that resets a Breakout scene into a fresh game state.

The current update order matters. Input is sampled before game logic. Paddle motion happens during the regular update phase. Ball movement, collision resolution, and out-of-bounds handling happen during the physics phase, but only when the scene is actively playing. Serving and end-of-round states are handled in the normal update pass so the ball can stay attached to the paddle or the round can restart on confirmation.

## Scene and Object Model

`Scene` is the base container for world objects. It owns the list of `GameObject` instances and assigns incremental IDs when new objects are created. It does not own rendering or physics behavior. Its `Update` hook exists, but in the current codebase it is not used for gameplay orchestration.

`GameObject` is the unit of composition. It stores a name, an active flag, a `Transform2D`, and a dictionary of components keyed by component type. Components are plain structs that implement `IComponent`, which keeps state lightweight and easy to pass around by value. The object API supports `Add`, `Set`, `Has`, `Get`, and `TryGet`, so systems can read and replace component state without needing inheritance or a large entity framework.

`Transform2D` is the shared spatial model. It carries position, rotation, and scale, and defaults to unit scale. The engine treats transform as object state, not as a property of any one component. Rendering and collision both read from it directly.

The scene model is flat. There is no hierarchy, parenting, or nested scene graph. Objects live in one list, and systems filter them by component presence and active state. That keeps the current game easy to reason about and fits the small number of objects Breakout needs.

## Rendering

Rendering is component-driven. `Engine/Rendering/RenderSystem.cs` gathers visible objects, picks the first renderer component it understands, sorts the draw entries by `CanvasItemComponent.ZIndex`, and then draws them in order. `CanvasItemComponent` is the gate that decides whether an object participates in rendering at all. It also carries a modulation color so the object can be tinted at draw time.

The renderer handlers are shape-specific. `RectangleRendererComponent`, `CircleRendererComponent`, and `TextRendererComponent` describe the visual data, while the corresponding handlers convert that data into Raylib draw calls. The rectangle handler respects transform scale and rotation, the circle handler uses the largest scale axis as its radius scale, and the text handler draws at the transformed offset with the configured font size.

`BreakoutRenderer` composes the frame. It clears the background, draws the arena, invokes the generic render system for scene objects, draws the HUD, and then shows an overlay whenever the game is not actively playing. The overlay is state-driven: serving shows the title and launch prompt, won and lost show end-of-round messages, and playing leaves the playfield unobstructed.

This split keeps the generic renderer reusable. The engine layer does not know anything about Breakout score, lives, or game phases. It only knows how to turn object components into pixels.

## Physics And Collision

The collision code is also deliberately small. `BoxColliderComponent` stores local box size, and `BoxCollision.GetCollision` combines that size with the object transforms to produce a signed `Collision` result. `Collision` contains a normal, a signed distance, and an approximate contact point. `CollisionHelper.GetCorrection` turns a negative-distance collision into a separation vector.

The current collision model is box-only. That is enough for the paddle, bricks, and the box-shaped ball. `CollisionSystem` applies the physics response, but the geometry math stays inside the engine layer. The system decides what the collision means: it clamps the ball against the arena walls, reflects the ball off the paddle with a hit-angle adjustment, destroys bricks, updates score, and transitions to the won state when no bricks remain.

The separation of responsibilities is important. `BoxCollision` answers geometric questions. `CollisionSystem` answers gameplay questions. That split keeps the geometry reusable and keeps Breakout-specific behavior out of the physics helper.

## Breakout Game Flow

`BreakoutScene` is the game-specific scene state. It stores the screen dimensions, arena rectangle, phase, score, lives, remaining brick count, and direct references to the paddle and ball objects. It also owns the brick list. The scene does not drive the frame loop; it only contains the state that systems work against.

The scene also provides creation helpers. `ResetCoreObjects` creates the paddle and ball once, attaches the reusable components they need, and resets their transforms and movement state at the start of a round. `CreateBrickObject` builds a brick with render, collision, and score components, then places it in the scene. `ClearBricks` removes old bricks from the scene object list before a new level is built.

`LevelBuilder` lays out the brick wall. It clears the scene, computes the brick width from the arena width, fixed column count, and gap spacing, and then fills the board row by row. Higher rows are worth more points, so the score value decreases as the bricks get closer to the paddle. At the end of the build pass it records how many bricks remain.

The gameplay systems each own one narrow part of the loop. `PaddleSystem` reads horizontal input and moves the paddle within the arena bounds. `BallSystem` keeps the ball attached to the paddle during serving, launches it with a randomized initial horizontal bias, and advances it by velocity during play. `CollisionSystem` resolves walls, paddle, and bricks. `RoundSystem` starts new games, resets rounds, and handles life loss when the ball falls below the arena. This keeps each system small and easy to change without disturbing the others.

## Data And Tuning

Game tuning lives in `BreakoutRules`. That file centralizes brick layout, ball and paddle sizes, speeds, starting lives, and HUD spacing. Visual tuning lives in `BreakoutTheme`, which defines the palette used by the arena, HUD, overlay, paddle, ball, and brick rows.

The code uses these constants directly instead of scattering magic numbers through systems and renderers. That makes the game easier to tune and keeps the architecture readable: rules describe behavior, theme describes appearance, and systems describe how the rules are applied.

## Current Boundaries

The current architecture is intentionally not a full engine. There is no scene serialization, no object hierarchy, no broadphase physics, no rigid-body simulation, and no animation system. The object model is enough for a small 2D game and leaves room for future growth without forcing a larger design too early.

The most important boundary is the one between engine and game code. `Engine/` should stay generic where possible, while `Game/` should hold Breakout-specific rules, layouts, and presentation. The current structure already follows that split closely, which makes the codebase a good base for further iteration.
