using System.Numerics;

namespace Breakout;

sealed class SpriteRendererComponent : Component
{
    public string AssetPath { get; init; } = null!;
    public Vector2 Size = Vector2.Zero;
    public Vector2 Offset = Vector2.Zero;
    public Vector2 Origin = Vector2.Zero;
}
