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
        _ballOutOfBoundsSubscription = Scene.Events.Subscribe<BallOutOfBoundsEvent>(OnBallOutOfBounds);
        _brickDestroyedSubscription = Scene.Events.Subscribe<BrickDestroyedEvent>(OnBrickDestroyed);
    }

    protected override void OnDetach()
    {
        _ballOutOfBoundsSubscription?.Dispose();
        _brickDestroyedSubscription?.Dispose();
    }

    public override void Start()
    {
        _paddle = Scene.Find<Paddle>()!;
        _ball = Scene.Find<Ball>()!;
        StartNewGame();
    }

    public override void Update(float dt)
    {
        BreakoutScene scene = (BreakoutScene)Scene;

        if (Phase is (GamePhase.Won or GamePhase.Lost) && scene.Input.ConfirmPressed)
        {
            StartNewGame();
        }
    }

    private void StartNewGame()
    {
        BreakoutScene scene = (BreakoutScene)Scene;
        Score = 0;
        Lives = BreakoutRules.StartingLives;
        RebuildBrickWall(scene);
        ResetRound();
    }

    private void RebuildBrickWall(BreakoutScene scene)
    {
        foreach (Brick brick in Scene.FindAll<Brick>().ToList())
        {
            Scene.RemoveObject(brick);
        }

        IReadOnlyList<Brick> bricks = BrickWallObjects.Create(scene.Arena);

        foreach (Brick brick in bricks)
        {
            Scene.AddObject(brick);
        }

        BricksRemaining = bricks.Count;
    }

    private void ResetRound()
    {
        BreakoutScene scene = (BreakoutScene)Scene;
        Phase = GamePhase.Serving;
        ResetRoundObjects(scene);
        Scene.Events.Publish(new RoundStartedEvent());
    }

    private void ResetRoundObjects(BreakoutScene scene)
    {
        _paddle.Active = true;
        _ball.Active = true;

        Transform2D paddleTransform = _paddle.Transform2D;
        paddleTransform.Position = new Vector2(
            scene.Arena.X + scene.Arena.Width * 0.5f,
            scene.ArenaBottom - BreakoutRules.PaddleFloorOffset + BreakoutRules.PaddleSize.Y * 0.5f);
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
