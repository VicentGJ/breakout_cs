namespace Breakout;

sealed class Brick : GameObject
{
    public int Points { get; set; }

    public Sprite Sprite { get; }

    public BoxCollider Collider { get; }

    public Brick()
    {
        Sprite = AddChild(new Sprite { Name = "BrickSprite", AssetPath = "Game/Assets/Textures/brick.png", ZIndex = 10 });
        Collider = AddChild(new BoxCollider { Name = "BrickCollider" });
    }

    public void Destroy()
    {
        if (!Active)
        {
            return;
        }

        World.Events.Publish(new BrickDestroyedEvent(this, Points));
        Active = false;
    }
}
