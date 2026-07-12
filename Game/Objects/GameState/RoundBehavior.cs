using System.Numerics;

namespace Breakout;

sealed class RoundBehavior(GameObject paddle, GameObject ballObject, List<GameObject> bricks) : Component, IStartable, IUpdatable
{
    private readonly LevelBuilder _levelBuilder = new();
    private IDisposable? _ballOutOfBoundsSubscription;
    private IDisposable? _brickDestroyedSubscription;

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

    public void Start()
    {
        StartNewGame();
    }

    public void Update(float dt)
    {
        BreakoutScene scene = (BreakoutScene)Scene;

        if (scene.Phase is (GamePhase.Won or GamePhase.Lost) && scene.Input.ConfirmPressed)
        {
            StartNewGame();
        }
    }

    private void StartNewGame()
    {
        BreakoutScene scene = (BreakoutScene)Scene;
        scene.Score = 0;
        scene.Lives = BreakoutRules.StartingLives;
        scene.BricksRemaining = _levelBuilder.Build(scene, bricks);
        ResetRound();
    }

    private void ResetRound()
    {
        BreakoutScene scene = (BreakoutScene)Scene;
        scene.Phase = GamePhase.Serving;
        ResetRoundObjects(scene);
        Scene.Events.Publish(new RoundStartedEvent());
    }

    private void ResetRoundObjects(BreakoutScene scene)
    {
        paddle.Active = true;
        ballObject.Active = true;

        Transform2D paddleTransform = paddle.Transform2D;
        paddleTransform.Position = new Vector2(
            scene.Arena.X + scene.Arena.Width * 0.5f,
            scene.ArenaBottom - BreakoutRules.PaddleFloorOffset + BreakoutRules.PaddleSize.Y * 0.5f);
        paddleTransform.Rotation = 0f;
        paddleTransform.Scale = Vector2.One;
        paddle.Transform2D = paddleTransform;

        Transform2D ballTransform = ballObject.Transform2D;
        BallComponent ball = ballObject.Get<BallComponent>();
        ballTransform.Position = Vector2.Zero;
        ballTransform.Rotation = 0f;
        ballTransform.Scale = Vector2.One;
        ball.PreviousPosition = Vector2.Zero;
        ball.Velocity = Vector2.Zero;
        ballObject.Transform2D = ballTransform;
    }

    private void OnBallOutOfBounds(BallOutOfBoundsEvent evt)
    {
        BreakoutScene scene = (BreakoutScene)Scene;

        if (scene.Phase != GamePhase.Playing)
        {
            return;
        }

        scene.Lives--;

        if (scene.Lives <= 0)
        {
            scene.Phase = GamePhase.Lost;
            return;
        }

        ResetRound();
    }

    private void OnBrickDestroyed(BrickDestroyedEvent evt)
    {
        BreakoutScene scene = (BreakoutScene)Scene;
        scene.BricksRemaining--;
        scene.Score += evt.Score;

        if (scene.BricksRemaining == 0)
        {
            scene.Phase = GamePhase.Won;
        }
    }
}
