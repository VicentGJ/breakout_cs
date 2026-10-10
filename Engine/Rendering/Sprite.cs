using System.Numerics;
using Raylib_cs;

namespace Breakout;

sealed class Sprite : GameObject
{
    public string AssetPath { get; init; } = null!;
    public Vector2 Size = Vector2.Zero;
    public Vector2 Offset = Vector2.Zero;
    public Vector2 Origin = Vector2.Zero;

    internal override void Draw(RenderSystem renderer, in Transform2D world)
    {
        Texture2D texture = renderer.GetTexture(AssetPath);
        Vector2 size = Vector2.Multiply(Size, world.Scale);
        Rectangle source = new(0f, 0f, texture.Width, texture.Height);
        Rectangle destination = new(world.Position.X + Offset.X, world.Position.Y + Offset.Y, size.X, size.Y);
        Vector2 origin = Vector2.Multiply(Origin, world.Scale);
        float rotationDegrees = world.Rotation * (180f / MathF.PI);

        Raylib.DrawTexturePro(texture, source, destination, origin, rotationDegrees, Modulate);
    }
}
