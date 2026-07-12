using Raylib_cs;

namespace Breakout;

sealed class AudioSystem : IDisposable
{
    private readonly Dictionary<string, Sound> _sounds = [];
    private readonly Dictionary<AudioComponent, Sound> _voices = [];

    public AudioSystem()
    {
        Raylib.InitAudioDevice();

        if (!Raylib.IsAudioDeviceReady())
        {
            throw new InvalidOperationException("Could not initialize the audio device.");
        }
    }

    static public float MasterVolume
    {
        get => Raylib.GetMasterVolume();
        set => Raylib.SetMasterVolume(value);
    }

    private Sound GetSound(string assetPath)
    {
        if (_sounds.TryGetValue(assetPath, out Sound sound))
        {
            return sound;
        }

        sound = Raylib.LoadSound(assetPath);

        if (!Raylib.IsSoundValid(sound))
        {
            throw new InvalidOperationException($"Could not load audio file '{assetPath}'.");
        }

        _sounds.Add(assetPath, sound);
        return sound;
    }

    public void Play(AudioComponent component)
    {
        if (!_voices.TryGetValue(component, out Sound voice))
        {
            voice = Raylib.LoadSoundAlias(GetSound(component.AssetPath));
            _voices.Add(component, voice);
        }

        Raylib.StopSound(voice);
        Raylib.SetSoundVolume(voice, component.Volume);
        Raylib.SetSoundPitch(voice, component.Pitch);
        Raylib.PlaySound(voice);
    }

    public void Stop(AudioComponent component)
    {
        if (_voices.TryGetValue(component, out Sound voice))
        {
            Raylib.StopSound(voice);
        }
    }

    public bool IsPlaying(AudioComponent component)
    {
        return _voices.TryGetValue(component, out Sound voice) && Raylib.IsSoundPlaying(voice);
    }

    public void StopScene(Scene scene)
    {
        List<AudioComponent> components = [];

        foreach (AudioComponent component in _voices.Keys)
        {
            if (component.Scene == scene)
            {
                components.Add(component);
            }
        }

        foreach (AudioComponent component in components)
        {
            Release(component);
        }
    }

    internal void Release(AudioComponent component)
    {
        if (_voices.Remove(component, out Sound voice))
        {
            Raylib.StopSound(voice);
            Raylib.UnloadSoundAlias(voice);
        }
    }

    public void Dispose()
    {
        foreach (Sound voice in _voices.Values)
        {
            Raylib.StopSound(voice);
            Raylib.UnloadSoundAlias(voice);
        }

        foreach (Sound sound in _sounds.Values)
        {
            Raylib.UnloadSound(sound);
        }

        Raylib.CloseAudioDevice();
    }
}
