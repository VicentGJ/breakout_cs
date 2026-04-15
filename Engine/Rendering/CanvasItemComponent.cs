using Raylib_cs;

namespace Breakout;

internal struct CanvasItemComponent : IComponent
{
    public bool Visible;
    public int ZIndex;
    public Color Modulate;
}
