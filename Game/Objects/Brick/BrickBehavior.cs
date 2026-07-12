namespace Breakout;

sealed class BrickBehavior : Component
{
    public void DestroyBrick()
    {
        if (!Owner.Active)
        {
            return;
        }

        BrickComponent brick = Owner.Get<BrickComponent>();

        Scene.Events.Publish(new BrickDestroyedEvent(Owner, brick.Points));
        Owner.Active = false;
    }
}
