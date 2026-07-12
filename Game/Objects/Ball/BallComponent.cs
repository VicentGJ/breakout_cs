using System.Numerics;

namespace Breakout;

sealed class BallComponent : Component
{
    public Vector2 PreviousPosition;
    public Vector2 Velocity;
}
