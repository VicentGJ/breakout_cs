namespace Breakout;

sealed class RoundBehavior : BehaviorComponent
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

    public override void Start()
    {
        StartNewGame();
    }

    public override void Update(float dt)
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
        _levelBuilder.Build(scene);
        ResetRound();
    }

    private void ResetRound()
    {
        BreakoutScene scene = (BreakoutScene)Scene;
        scene.Phase = GamePhase.Serving;
        scene.ResetCoreObjects();
        Scene.Events.Publish(new RoundStartedEvent());
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
