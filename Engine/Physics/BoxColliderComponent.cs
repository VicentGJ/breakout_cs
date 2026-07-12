using System.Numerics;

namespace Breakout;

sealed class BoxColliderComponent : Component
{
    public Vector2 Size;

    public Collision GetCollision(BoxColliderComponent other, in Transform2D selfTransform, in Transform2D otherTransform)
    {
        return BoxCollision.GetCollision(this, other, in selfTransform, in otherTransform);
    }
}
