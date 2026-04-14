# Breakout CS - Agent Notes

## Project
Breakout game being used as development vehicle for a 2D game engine foundation.
NOT a shipped engine - expect iteration on architecture.

## Build
```bash
dotnet build
dotnet run
```

## Communication
- Respond **terse** like smart caveman
- All fluff removed, technical substance stays
- Active by default until user says `stop caveman` or `normal mode`

## Naming Conventions (Locked)
- `GameApp` for main runtime wrapper
- Scene owns game state; no intermediate "World" object
- Renderer API: typed components, not primitive enum blob
- Folder split: `Engine/` (generic) vs `Game/` (Breakout-specific)

## Folder Structure (Stable)
```
Engine/
  Runtime/       # GameApp, FrameContext, InputState, SceneManager
  Scene/         # GameObject, components
  Rendering/     # RenderSystem, handlers
Game/
  Scenes/        # BreakoutScene
  Systems/       # Paddle, Ball, Collision, Round
  Content/       # Rules, Theme, LevelBuilder
  Rendering/     # BreakoutRenderer
```

## What May Change
- Component model details
- Systems design
- Renderer component types
- Object/component organization

## What's Stable
- `GameApp -> Scene -> Update/Draw` loop
- Scene owns objects
- Systems operate on scene
- Generic render system + game-specific overlay pattern
