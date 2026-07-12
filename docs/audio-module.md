# Audio System Spec

## Purpose

Add a small, non-spatial audio system that fits the current component model.

`AudioComponent` gives an object local playback control. `AudioSystem` owns the Raylib audio device, loaded clips, active voices, and volume. Game components decide when a sound happens; the audio engine decides how to play it.

## Version 1

Version 1 supports one-shots, loops, stopping, playback-state queries, per-component volume and pitch, music/SFX categories, and category volume.

It does not include spatial audio, listeners, mixer graphs, effects, scheduling, editor tooling, or multiple concurrent voices from one `AudioComponent`.

## Ownership and Lifetime

`Application` owns one `AudioSystem`, just as it owns the render system. It initializes Raylib's audio device after creating the window and closes it before closing the window.

Before a scene runs `Setup()`, `Application` binds its `AudioSystem` to that scene. Components added during setup can therefore access audio from their first `OnAttach()` callback.

```text
Program
  -> Application
      -> AudioSystem
      -> active Scene
          -> GameObject
              -> AudioComponent
```

On `ChangeScene`, `Application` stops voices owned by the outgoing scene and unbinds audio from it before binding the incoming scene. Version 1 does not preserve music across scene changes.

`Scene` exposes the bound system internally. This is an engine service reference, not a gameplay dependency:

```csharp
internal AudioSystem? Audio { get; private set; }
```

## AudioComponent

`AudioComponent` is an ordinary `Component`. It has owner and scene context through the common component base, but does not need a frame-update interface.

```csharp
sealed class AudioComponent : Component
{
    public AudioClip Clip { get; init; } = null!;
    public AudioCategory Category { get; set; } = AudioCategory.Sfx;
    public float Volume { get; set; } = 1f;
    public float Pitch { get; set; } = 1f;
    public bool Loop { get; set; }

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
        Scene.Audio?.Stop(this);
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
    public float MasterVolume { get; set; } = 1f;

    public AudioClip LoadClip(string assetPath);
    public void Play(AudioComponent component);
    public void Stop(AudioComponent component);
    public bool IsPlaying(AudioComponent component);
    public void StopScene(Scene scene);

    public void SetCategoryVolume(AudioCategory category, float volume);
    public float GetCategoryVolume(AudioCategory category);
}
```

The effective volume is the component volume multiplied by its category volume and `MasterVolume`.

```csharp
enum AudioCategory
{
    Music,
    Sfx
}
```

`Master` is not a category. It is global mixer state, so it cannot accidentally be assigned to a component.

## Voice Policy

Each `AudioComponent` owns one active voice. Calling `Play` stops that component's current voice, if any, before starting its assigned clip. `IsPlaying` reports only that voice.

This makes the first implementation predictable: no voice handles, no pool, and no ambiguous playback state. Add overlapping voices only when the game demonstrates a need.

## Audio Clips

`AudioClip` is a small engine wrapper around a loaded Raylib sound. `AudioSystem.LoadClip` caches clips by source path and unloads them when the application closes.

The object recipe that owns a sound chooses its clip directly:

```csharp
gameObject.Add(new AudioComponent
{
    Clip = scene.Audio!.LoadClip(
        "Game/Assets/Audio/SFX/ball_bounce.wav")
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

Music uses the same model: a dedicated object has an `AudioComponent` with `Category = Music` and `Loop = true`.

## Implementation Boundary

1. Add `Engine/Audio/AudioSystem.cs`, `AudioComponent.cs`, and `AudioCategory.cs`.
2. Add internal audio bind/unbind support to `Scene`.
3. Have `Application` initialize and close the Raylib audio device, bind scenes, and stop outgoing-scene voices.
4. Add an `AudioComponent` with `ball_bounce.wav` to the ball.
5. Play the component after a successful bounce.

Do not add music content, overlapping voices, or a custom mixer in this pass.
