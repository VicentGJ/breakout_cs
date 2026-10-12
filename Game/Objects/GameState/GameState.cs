using System.Numerics;

namespace Breakout;

sealed class GameState : GameObject
{
    private IDisposable? _sceneLoadedSubscription;
    private IDisposable? _ballOutOfBoundsSubscription;
    private IDisposable? _brickDestroyedSubscription;

    public GamePhase Phase { get; set; }

    public int Score { get; set; }

    public int Lives { get; set; } = BreakoutRules.StartingLives;

    public int BricksRemaining { get; set; }

    protected override void OnAttach()
    {
        _sceneLoadedSubscription = World.Events.Subscribe<SceneLoadedEvent>(OnSceneLoaded);
        _ballOutOfBoundsSubscription = World.Events.Subscribe<BallOutOfBoundsEvent>(OnBallOutOfBounds);
        _brickDestroyedSubscription = World.Events.Subscribe<BrickDestroyedEvent>(OnBrickDestroyed);
    }

    protected override void OnDetach()
    {
        _sceneLoadedSubscription?.Dispose();
        _ballOutOfBoundsSubscription?.Dispose();
        _brickDestroyedSubscription?.Dispose();
    }

    public override void Update(float dt)
    {
        BreakoutWorld world = (BreakoutWorld)World;

        if (Phase is (GamePhase.Won or GamePhase.Lost) && world.Input.ConfirmPressed)
        {
            World.LoadScene(world.CreateScene());
        }
    }

    // Runs when the scene's objects are in place. A restart resets the run; a new scene keeps it.
    private void OnSceneLoaded(SceneLoadedEvent evt)
    {
        if (evt.IsRestart)
        {
            Score = 0;
            Lives = BreakoutRules.StartingLives;
        }

        BricksRemaining = World.FindAll<Brick>().Count();
        ResetRound();
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
        Paddle paddle = World.Find<Paddle>()!;
        Ball ball = World.Find<Ball>()!;
        paddle.Active = true;
        ball.Active = true;

        Transform2D paddleTransform = paddle.Transform2D;
        paddleTransform.Position = new Vector2(
            world.Arena.X + world.Arena.Width * 0.5f,
            world.ArenaBottom - BreakoutRules.PaddleFloorOffset + BreakoutRules.PaddleSize.Y * 0.5f);
        paddleTransform.Rotation = 0f;
        paddleTransform.Scale = Vector2.One;
        paddle.Transform2D = paddleTransform;

        Transform2D ballTransform = ball.Transform2D;
        ballTransform.Position = Vector2.Zero;
        ballTransform.Rotation = 0f;
        ballTransform.Scale = Vector2.One;
        ball.PreviousPosition = Vector2.Zero;
        ball.Velocity = Vector2.Zero;
        ball.Transform2D = ballTransform;
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
