using System.Numerics;

namespace Breakout;

struct BallComponent : IComponent
{
    public Vector2 PreviousPosition;
    public Vector2 Velocity;
}
