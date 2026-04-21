namespace Breakout;

sealed class BrickBehavior : BehaviorComponent
{
    public void DestroyBrick()
    {
        if (!Owner.Active)
        {
            return;
        }

        BrickComponent brick = Owner.Get<BrickComponent>();
        CanvasItemComponent canvasItem = Owner.Get<CanvasItemComponent>();

        Scene.Events.Publish(new BrickDestroyedEvent(Owner, brick.Points));
        Owner.Active = false;
        canvasItem.Visible = false;
        Owner.Set(canvasItem);
    }
}
