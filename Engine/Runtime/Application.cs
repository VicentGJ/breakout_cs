using Raylib_cs;

namespace Breakout;

sealed class Application
{
    private readonly int _windowWidth;
    private readonly int _windowHeight;
    private readonly string _title;
    private readonly Runtime _runtime;

    public Application(int windowWidth, int windowHeight, string title, Runtime runtime)
    {
        _windowWidth = windowWidth;
        _windowHeight = windowHeight;
        _title = title;
        _runtime = runtime;
    }

    public void Run()
    {
        Raylib.InitWindow(_windowWidth, _windowHeight, _title);
        Raylib.SetTargetFPS(120);

        while (!Raylib.WindowShouldClose())
        {
            _runtime.BeginFrame(Raylib.GetFrameTime());
            _runtime.Update();
            _runtime.PhysicsUpdate();

            Raylib.BeginDrawing();
            _runtime.Draw();
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
