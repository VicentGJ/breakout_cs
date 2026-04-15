using System.Numerics;

namespace Breakout;

struct BoxCollider
{
    public Vector2 Size = Vector2.Zero;

    public BoxCollider()
    {
        Size = Vector2.Zero;
    }

    public Collision GetCollision(in BoxCollider other, in Transform2D selfTransform, in Transform2D otherTransform)
    {
        return BoxCollision.GetCollision(in this, in other, in selfTransform, in otherTransform);
    }
}
