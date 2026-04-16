using System.Numerics;

namespace Breakout;

struct BoxColliderComponent : IComponent
{
    public Vector2 Size = Vector2.Zero;

    public BoxColliderComponent()
    {
        Size = Vector2.Zero;
    }

    public Collision GetCollision(in BoxColliderComponent other, in Transform2D selfTransform, in Transform2D otherTransform)
    {
        return BoxCollision.GetCollision(in this, in other, in selfTransform, in otherTransform);
    }
}
