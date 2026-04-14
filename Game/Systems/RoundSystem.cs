namespace Breakout;

internal sealed class RoundSystem
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
        Transform2DComponent ballTransform = scene.BallObject.Get<Transform2DComponent>();
        float ballRadius = GameObjectGeometry.GetCircleRadius(scene.BallObject);

        if (ballTransform.Position.Y - ballRadius <= scene.ArenaBottom)
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
