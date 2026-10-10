namespace Breakout;

sealed class AudioPlayer : GameObject
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
