using Raylib_cs;

namespace Breakout;

sealed class AudioSystem : IDisposable
{
    private readonly Dictionary<string, Sound> _sounds = [];
    private readonly Dictionary<AudioPlayer, Sound> _voices = [];

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

    public void Play(AudioPlayer player)
    {
        if (!_voices.TryGetValue(player, out Sound voice))
        {
            voice = Raylib.LoadSoundAlias(GetSound(player.AssetPath));
            _voices.Add(player, voice);
        }

        Raylib.StopSound(voice);
        Raylib.SetSoundVolume(voice, player.Volume);
        Raylib.SetSoundPitch(voice, player.Pitch);
        Raylib.PlaySound(voice);
    }

    public void Stop(AudioPlayer player)
    {
        if (_voices.TryGetValue(player, out Sound voice))
        {
            Raylib.StopSound(voice);
        }
    }

    public bool IsPlaying(AudioPlayer player)
    {
        return _voices.TryGetValue(player, out Sound voice) && Raylib.IsSoundPlaying(voice);
    }

    public void StopScene(Scene scene)
    {
        List<AudioPlayer> players = [];

        foreach (AudioPlayer player in _voices.Keys)
        {
            if (player.Scene == scene)
            {
                players.Add(player);
            }
        }

        foreach (AudioPlayer player in players)
        {
            Release(player);
        }
    }

    internal void Release(AudioPlayer player)
    {
        if (_voices.Remove(player, out Sound voice))
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
