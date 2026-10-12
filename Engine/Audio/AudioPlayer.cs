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
        World.Audio?.Release(this);
    }

    private AudioSystem GetAudio()
    {
        return World.Audio
            ?? throw new InvalidOperationException("Audio is unavailable for this world.");
    }
}
