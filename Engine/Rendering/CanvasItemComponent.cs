using Raylib_cs;

namespace Breakout;

struct CanvasItemComponent : IComponent
{
    public bool Visible;
    public int ZIndex;
    public Color Modulate;
}
