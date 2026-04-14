namespace Breakout;

internal interface IGameScene
{
    void Update(FrameContext context);
    void Draw(FrameContext context);
}

internal sealed class SceneManager
{
    private IGameScene _scene;

    public SceneManager(IGameScene scene)
    {
        _scene = scene;
    }

    public void Update(FrameContext context) => _scene.Update(context);

    public void Draw(FrameContext context) => _scene.Draw(context);
}
