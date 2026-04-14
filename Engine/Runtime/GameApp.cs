using Raylib_cs;

namespace Breakout;

internal sealed class GameApp
{
    private readonly int _windowWidth;
    private readonly int _windowHeight;
    private readonly string _title;
    private readonly SceneManager _sceneManager;

    public GameApp(int windowWidth, int windowHeight, string title, IGameScene initialScene)
    {
        _windowWidth = windowWidth;
        _windowHeight = windowHeight;
        _title = title;
        _sceneManager = new SceneManager(initialScene);
    }

    public void Run()
    {
        Raylib.InitWindow(_windowWidth, _windowHeight, _title);
        Raylib.SetTargetFPS(120);

        while (!Raylib.WindowShouldClose())
        {
            FrameContext frame = new(Raylib.GetFrameTime(), _windowWidth, _windowHeight, InputState.Poll());

            _sceneManager.Update(frame);

            Raylib.BeginDrawing();
            _sceneManager.Draw(frame);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
