using System.Numerics;

namespace Breakout;

sealed class BoxCollider : GameObject
{
    public Vector2 Size;

    public Collision GetCollision(BoxCollider other)
    {
        return BoxCollision.GetCollision(this, other, GetWorldTransform(), other.GetWorldTransform());
    }
}
