namespace Breakout;

sealed class RoundSystem
{
    private readonly LevelBuilder _levelBuilder;
    private readonly BallSystem _ballSystem;

    public RoundSystem(LevelBuilder levelBuilder, BallSystem ballSystem)
    {
        _levelBuilder = levelBuilder;
        _ballSystem = ballSystem;
    }

    public void StartNewGame(BreakoutScene scene)
    {
        scene.Score = 0;
        scene.Lives = BreakoutRules.StartingLives;
        _levelBuilder.Build(scene);
        ResetRound(scene);
    }

    public void ResetRound(BreakoutScene scene)
    {
        scene.Phase = GamePhase.Serving;
        scene.ResetCoreObjects();
        _ballSystem.StickToPaddle(scene);
    }

    public void HandleBallOutOfBounds(BreakoutScene scene)
    {
        Transform2D ballTransform = scene.BallObject.Transform2D;
        BoxColliderComponent ballCollider = scene.BallObject.Get<BoxColliderComponent>();
        float ballHalfHeight = MathF.Abs(ballCollider.Size.Y * ballTransform.Scale.Y) * 0.5f;

        if (ballTransform.Position.Y - ballHalfHeight <= scene.ArenaBottom)
        {
            return;
        }

        scene.Lives--;

        if (scene.Lives <= 0)
        {
            scene.Phase = GamePhase.Lost;
            return;
        }

        ResetRound(scene);
    }
}
