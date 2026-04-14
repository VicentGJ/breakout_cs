namespace Breakout;

internal interface IGameScene
{
    void Update();
    void Draw();
}

internal sealed class SceneManager
{
    private IGameScene _scene;

    public SceneManager(IGameScene scene)
    {
        _scene = scene;
    }

    public void Update() => _scene.Update();

    public void Draw() => _scene.Draw();
}
