using System.Numerics;

namespace Breakout;

static class CollisionHelper
{
    public static Vector2 GetCorrection(in Collision collision)
    {
        if (collision.Distance >= 0f)
        {
            return Vector2.Zero;
        }

        return collision.Normal * collision.Distance;
    }
}
