namespace Breakout;

interface IGameScene
{
    void Update();
    void Draw();
}

sealed class SceneManager
{
    private IGameScene _scene;

    public SceneManager(IGameScene scene)
    {
        _scene = scene;
    }

    public void Update() => _scene.Update();

    public void Draw() => _scene.Draw();
}
