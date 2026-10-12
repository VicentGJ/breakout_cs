using System.Numerics;

namespace Breakout;

sealed class GameState : GameObject
{
    private Paddle _paddle = null!;
    private Ball _ball = null!;
    private IDisposable? _ballOutOfBoundsSubscription;
    private IDisposable? _brickDestroyedSubscription;

    public GamePhase Phase { get; set; }

    public int Score { get; set; }

    public int Lives { get; set; }

    public int BricksRemaining { get; set; }

    protected override void OnAttach()
    {
        _ballOutOfBoundsSubscription = World.Events.Subscribe<BallOutOfBoundsEvent>(OnBallOutOfBounds);
        _brickDestroyedSubscription = World.Events.Subscribe<BrickDestroyedEvent>(OnBrickDestroyed);
    }

    protected override void OnDetach()
    {
        _ballOutOfBoundsSubscription?.Dispose();
        _brickDestroyedSubscription?.Dispose();
    }

    public override void Start()
    {
        _paddle = World.Find<Paddle>()!;
        _ball = World.Find<Ball>()!;
        StartNewGame();
    }

    public override void Update(float dt)
    {
        BreakoutWorld world = (BreakoutWorld)World;

        if (Phase is (GamePhase.Won or GamePhase.Lost) && world.Input.ConfirmPressed)
        {
            StartNewGame();
        }
    }

    private void StartNewGame()
    {
        BreakoutWorld world = (BreakoutWorld)World;
        Score = 0;
        Lives = BreakoutRules.StartingLives;
        RebuildBrickWall(world);
        ResetRound();
    }

    private void RebuildBrickWall(BreakoutWorld world)
    {
        foreach (Brick brick in World.FindAll<Brick>().ToList())
        {
            World.RemoveObject(brick);
        }

        IReadOnlyList<Brick> bricks = BrickWallObjects.Create(world.Arena);

        foreach (Brick brick in bricks)
        {
            World.AddObject(brick);
        }

        BricksRemaining = bricks.Count;
    }

    private void ResetRound()
    {
        BreakoutWorld world = (BreakoutWorld)World;
        Phase = GamePhase.Serving;
        ResetRoundObjects(world);
        World.Events.Publish(new RoundStartedEvent());
    }

    private void ResetRoundObjects(BreakoutWorld world)
    {
        _paddle.Active = true;
        _ball.Active = true;

        Transform2D paddleTransform = _paddle.Transform2D;
        paddleTransform.Position = new Vector2(
            world.Arena.X + world.Arena.Width * 0.5f,
            world.ArenaBottom - BreakoutRules.PaddleFloorOffset + BreakoutRules.PaddleSize.Y * 0.5f);
        paddleTransform.Rotation = 0f;
        paddleTransform.Scale = Vector2.One;
        _paddle.Transform2D = paddleTransform;

        Transform2D ballTransform = _ball.Transform2D;
        ballTransform.Position = Vector2.Zero;
        ballTransform.Rotation = 0f;
        ballTransform.Scale = Vector2.One;
        _ball.PreviousPosition = Vector2.Zero;
        _ball.Velocity = Vector2.Zero;
        _ball.Transform2D = ballTransform;
    }

    private void OnBallOutOfBounds(BallOutOfBoundsEvent evt)
    {
        if (Phase != GamePhase.Playing)
        {
            return;
        }

        Lives--;

        if (Lives <= 0)
        {
            Phase = GamePhase.Lost;
            return;
        }

        ResetRound();
    }

    private void OnBrickDestroyed(BrickDestroyedEvent evt)
    {
        BricksRemaining--;
        Score += evt.Score;

        if (BricksRemaining == 0)
        {
            Phase = GamePhase.Won;
        }
    }
}
