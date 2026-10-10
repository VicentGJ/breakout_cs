using System.Numerics;
using Raylib_cs;

namespace Breakout;

sealed class Label : GameObject
{
    public string Text = string.Empty;
    public int FontSize = 16;
    public Vector2 Offset = Vector2.Zero;
    public Color Color = Color.White;

    internal override void Draw(RenderSystem renderer, in Transform2D world)
    {
        Vector2 position = world.Position + Offset;
        Raylib.DrawText(Text, (int)position.X, (int)position.Y, FontSize, RenderSystem.Modulate(Color, Modulate));
    }
}
