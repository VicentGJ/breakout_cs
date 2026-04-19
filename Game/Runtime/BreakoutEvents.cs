namespace Breakout;

readonly record struct BallLaunchedEvent(GameObject Ball) : ISceneEvent;

readonly record struct BallOutOfBoundsEvent(GameObject Ball) : ISceneEvent;

readonly record struct BrickDestroyedEvent(GameObject Brick, int Score) : ISceneEvent;

readonly struct RoundStartedEvent : ISceneEvent
{
}
