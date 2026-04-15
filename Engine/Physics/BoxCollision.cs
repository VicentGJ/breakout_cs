using System.Numerics;

namespace Breakout;

static class BoxCollision
{
    public static Collision GetCollision(in BoxCollider self, in BoxCollider other, in Transform2D selfTransform, in Transform2D otherTransform)
    {
        Bounds selfBounds = GetBounds(in self, in selfTransform);
        Bounds otherBounds = GetBounds(in other, in otherTransform);

        float overlapX = MathF.Min(selfBounds.Right, otherBounds.Right) - MathF.Max(selfBounds.Left, otherBounds.Left);
        float overlapY = MathF.Min(selfBounds.Bottom, otherBounds.Bottom) - MathF.Max(selfBounds.Top, otherBounds.Top);

        if (overlapX < 0f || overlapY < 0f)
        {
            return GetSeparation(selfBounds, otherBounds);
        }

        if (overlapX < overlapY)
        {
            float centerDeltaX = otherBounds.Center.X - selfBounds.Center.X;
            float distance = -overlapX;
            Vector2 normal = centerDeltaX >= 0f ? Vector2.UnitX : -Vector2.UnitX;

            return new Collision
            {
                Normal = normal,
                Distance = distance,
                Point = GetContactPoint(selfBounds, otherBounds, normal)
            };
        }

        float centerDeltaY = otherBounds.Center.Y - selfBounds.Center.Y;
        float distanceY = -overlapY;
        Vector2 normalY = centerDeltaY >= 0f ? Vector2.UnitY : -Vector2.UnitY;

        return new Collision
        {
            Normal = normalY,
            Distance = distanceY,
            Point = GetContactPoint(selfBounds, otherBounds, normalY)
        };
    }

    private static Collision GetSeparation(in Bounds selfBounds, in Bounds otherBounds)
    {
        Vector2 delta = otherBounds.Center - selfBounds.Center;
        float gapX = Math.Max(0f, Math.Max(selfBounds.Left, otherBounds.Left) - Math.Min(selfBounds.Right, otherBounds.Right));
        float gapY = Math.Max(0f, Math.Max(selfBounds.Top, otherBounds.Top) - Math.Min(selfBounds.Bottom, otherBounds.Bottom));

        if (gapX < gapY)
        {
            Vector2 normal = delta.X >= 0f ? Vector2.UnitX : -Vector2.UnitX;
            return new Collision { Normal = normal, Distance = gapX, Point = ClosestPointOnPerimeter(selfBounds, otherBounds.Center) };
        }

        Vector2 normalY = delta.Y >= 0f ? Vector2.UnitY : -Vector2.UnitY;
        return new Collision { Normal = normalY, Distance = gapY, Point = ClosestPointOnPerimeter(selfBounds, otherBounds.Center) };
    }

    private static Bounds GetBounds(in BoxCollider collider, in Transform2D transform)
    {
        Vector2 size = collider.Size * transform.Scale;
        Vector2 half = Vector2.Abs(size) * 0.5f;

        return new Bounds
        {
            Center = transform.Position,
            HalfSize = half
        };
    }

    private static Vector2 GetContactPoint(in Bounds a, in Bounds b, Vector2 normal)
    {
        Vector2 point = (a.Center + b.Center) * 0.5f;

        if (normal.X != 0f)
        {
            point.X = normal.X > 0f ? a.Right : a.Left;
            point.Y = Math.Clamp(point.Y, Math.Max(a.Top, b.Top), Math.Min(a.Bottom, b.Bottom));
        }
        else
        {
            point.Y = normal.Y > 0f ? a.Bottom : a.Top;
            point.X = Math.Clamp(point.X, Math.Max(a.Left, b.Left), Math.Min(a.Right, b.Right));
        }

        return point;
    }

    private static Vector2 ClosestPointOnPerimeter(in Bounds bounds, Vector2 point)
    {
        float x = Math.Clamp(point.X, bounds.Left, bounds.Right);
        float y = Math.Clamp(point.Y, bounds.Top, bounds.Bottom);

        if (point.X < bounds.Left)
        {
            x = bounds.Left;
        }
        else if (point.X > bounds.Right)
        {
            x = bounds.Right;
        }
        else if (point.Y < bounds.Top)
        {
            y = bounds.Top;
        }
        else if (point.Y > bounds.Bottom)
        {
            y = bounds.Bottom;
        }

        return new Vector2(x, y);
    }

    private readonly struct Bounds
    {
        public Vector2 Center { get; init; }
        public Vector2 HalfSize { get; init; }

        public float Left => Center.X - HalfSize.X;
        public float Right => Center.X + HalfSize.X;
        public float Top => Center.Y - HalfSize.Y;
        public float Bottom => Center.Y + HalfSize.Y;
    }
}
