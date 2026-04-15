using System.Numerics;

namespace Breakout;

internal struct BallComponent : IComponent
{
    public Vector2 PreviousPosition;
    public Vector2 Velocity;
}
