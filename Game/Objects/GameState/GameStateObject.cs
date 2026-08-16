namespace Breakout;

static class GameStateObject
{
    public static GameObject Create()
    {
        return new GameObject("GameState");
    }

    public static void AddRoundBehavior(GameObject gameObject, GameObject paddle, GameObject ball, List<GameObject> bricks)
    {
        gameObject.Add(new RoundBehavior(paddle, ball, bricks));
    }
}
