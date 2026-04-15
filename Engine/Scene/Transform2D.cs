using System.Numerics;

namespace Breakout;

struct Transform2D
{
    public Vector2 Position;
    public float Rotation;
    public Vector2 Scale;

    public static Transform2D Default => new()
    {
        Scale = Vector2.One
    };
}
