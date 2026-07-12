# Audio System Spec

## Purpose

Add a small, non-spatial audio system that fits the current component model.

`AudioComponent` gives an object local playback control. `AudioSystem` owns the Raylib audio device, loaded sounds, active voices, and volume. Game components decide when a sound happens; the audio engine decides how to play it.

## Version 1

Version 1 supports one-shots, stopping, playback-state queries, per-component volume and pitch, and global master volume.

It does not include looping or streaming music, spatial audio, listeners, mixer graphs, effects, scheduling, editor tooling, or multiple concurrent voices from one `AudioComponent`.

## Ownership and Lifetime

`Application` owns one `AudioSystem`, just as it owns the render system. It creates the system after creating the window; the system initializes Raylib's audio device and closes it before the window closes.

Before a scene runs `Setup()`, `Application` binds its `AudioSystem` to that scene. Components added during setup can therefore access audio from their first `OnAttach()` callback.

```text
Program
  -> Application
      -> AudioSystem
      -> active Scene
          -> GameObject
              -> AudioComponent
```

On `ChangeScene`, `Application` stops voices owned by the outgoing scene and unbinds audio from it before binding the incoming scene. Sounds do not persist across scene changes.

`Scene` exposes the bound system internally. This is an engine service reference, not a gameplay dependency:

```csharp
internal AudioSystem? Audio { get; private set; }
```

## AudioComponent

`AudioComponent` is an ordinary `Component`. It has owner and scene context through the common component base, but does not need a frame-update interface.

```csharp
sealed class AudioComponent : Component
{
    public string AssetPath { get; init; } = null!;
    public float Volume { get; set; } = 1f;
    public float Pitch { get; set; } = 1f;

    public void Play()
    {
        GetAudio().Play(this);
    }

    public void Stop()
    {
        GetAudio().Stop(this);
    }

    public bool IsPlaying => GetAudio().IsPlaying(this);

    protected override void OnDetach()
    {
        Scene.Audio?.Release(this);
    }

    private AudioSystem GetAudio()
    {
        return Scene.Audio
            ?? throw new InvalidOperationException("Audio is unavailable for this scene.");
    }
}
```

The component does not keep its own `AudioSystem` reference. Scene binding is enough, and unbinding a scene makes stale objects unable to issue playback commands.

## AudioSystem

`AudioSystem` owns backend state. Its public surface stays small:

```csharp
sealed class AudioSystem
{
    public static float MasterVolume { get; set; }

    public void Play(AudioComponent component);
    public void Stop(AudioComponent component);
    public bool IsPlaying(AudioComponent component);
    public void StopScene(Scene scene);
}
```

The effective volume is the component volume multiplied by `MasterVolume`. Master volume is global Raylib device state; component volume is applied when that component plays.

## Voice Policy

Each `AudioComponent` owns one active voice. On first play, the audio system loads and caches the component's asset path, then creates a Raylib sound alias for that component. Calling `Play` stops that component's current voice, if any, before starting it again. `IsPlaying` reports only that voice.

This makes the first implementation predictable: no voice handles, no pool, and no ambiguous playback state. Add overlapping voices only when the game demonstrates a need.

## Audio Assets

An `AudioComponent` names its local asset path. `AudioSystem` loads and caches its Raylib sound on first play, then unloads all cached sounds when the application closes.

The object recipe that owns a sound only declares its asset:

```csharp
gameObject.Add(new AudioComponent
{
    AssetPath = "Game/Assets/Audio/SFX/ball_bounce.wav"
});
```

The project already copies `Game/Assets/` beside the executable. Asset paths remain local game-content configuration, not engine cue identifiers.

## Usage

Object-local sounds are direct component calls:

```csharp
sealed class BallBehavior : Component, IPhysicsUpdatable
{
    private AudioComponent _audio = null!;

    protected override void OnAttach()
    {
        _audio = Owner.Get<AudioComponent>();
    }

    private void OnBounce()
    {
        _audio.Play();
    }

    public void PhysicsUpdate(float dt)
    {
        // movement and collision
    }
}
```

The event bus is unnecessary when a behavior plays a sound from its own object. Use it only when a game fact should independently trigger audio from another object or service.

Streaming and looping music require Raylib's separate music-stream API, so they are deliberately deferred from version 1.

## Implementation Boundary

1. Add `Engine/Audio/AudioSystem.cs` and `AudioComponent.cs`.
2. Add internal audio bind/unbind support to `Scene`.
3. Have `Application` initialize and close the Raylib audio device, bind scenes before setup, and stop/release outgoing-scene voices.
4. Add an `AudioComponent` with `ball_bounce.wav` to the ball.
5. Play the component after a successful bounce.

Do not add music content, loops, overlapping voices, or a custom mixer in this pass.
