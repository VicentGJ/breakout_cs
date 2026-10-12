namespace Breakout;

sealed class Paddle : GameObject
{
    private GameState _game = null!;

    public float Speed { get; set; } = BreakoutRules.PaddleSpeed;

    public RectangleShape Visual { get; }

    public BoxCollider Collider { get; }

    public Paddle()
    {
        Visual = AddChild(new RectangleShape
        {
            Name = "PaddleVisual",
            ZIndex = 20,
            Size = BreakoutRules.PaddleSize,
            Origin = BreakoutRules.PaddleSize * 0.5f,
            FillColor = BreakoutTheme.PaddleColor
        });
        Collider = AddChild(new BoxCollider { Name = "PaddleCollider", Size = BreakoutRules.PaddleSize });
    }

    public override void Start()
    {
        _game = World.Find<GameState>()!;
    }

    public override void Update(float dt)
    {
        if (_game.Phase is not (GamePhase.Serving or GamePhase.Playing))
        {
            return;
        }

        BreakoutWorld world = (BreakoutWorld)World;
        Transform2D transform = Transform2D;
        float halfWidth = MathF.Abs(Collider.Size.X * transform.Scale.X) * 0.5f;

        transform.Position.X += world.Input.HorizontalAxis * Speed * dt;
        transform.Position.X = Math.Clamp(transform.Position.X, world.Arena.X + halfWidth, world.ArenaRight - halfWidth);

        Transform2D = transform;
    }
}
