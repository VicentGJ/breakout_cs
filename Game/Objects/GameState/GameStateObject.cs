namespace Breakout;

static class GameStateObject
{
    public static GameObject Create(Scene scene)
    {
        GameObject gameObject = new("GameState");
        scene.AddObject(gameObject);
        return gameObject;
    }

    public static void AddRoundBehavior(GameObject gameObject, GameObject paddle, GameObject ball, List<GameObject> bricks)
    {
        gameObject.AddBehavior(new RoundBehavior(paddle, ball, bricks));
    }
}
