using Raylib_cs;

namespace Breakout;

sealed class GameApp
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
            Time.UnscaledDeltaTime = Raylib.GetFrameTime();
            Time.DeltaTime = Time.UnscaledDeltaTime * Time.TimeScale;

            _sceneManager.Update();

            Raylib.BeginDrawing();
            _sceneManager.Draw();
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
