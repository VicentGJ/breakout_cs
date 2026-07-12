using Raylib_cs;

namespace Breakout;

sealed class Application
{
    private readonly int _windowWidth;
    private readonly int _windowHeight;
    private readonly string _title;

    public Application(int windowWidth, int windowHeight, string title, Scene initialScene)
    {
        _windowWidth = windowWidth;
        _windowHeight = windowHeight;
        _title = title;
        Time = new RuntimeTime();
        ActiveScene = null!;
        ChangeScene(initialScene);
    }

    public Scene ActiveScene { get; private set; }

    public RuntimeTime Time { get; }

    public void Run()
    {
        Raylib.InitWindow(_windowWidth, _windowHeight, _title);
        Raylib.SetTargetFPS(120);

        while (!Raylib.WindowShouldClose())
        {
            BeginFrame(Raylib.GetFrameTime());
            Update();
            PhysicsUpdate();

            Raylib.BeginDrawing();
            Draw();
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    public void ChangeScene(Scene scene)
    {
        ActiveScene = scene;
        ActiveScene.SetupOnce();
    }

    private void BeginFrame(float unscaledDeltaTime)
    {
        Time.UnscaledDeltaTime = unscaledDeltaTime;
        Time.DeltaTime = unscaledDeltaTime * Time.TimeScale;
    }

    private void Update()
    {
        ActiveScene.Update(Time.DeltaTime);
        ActiveScene.UpdateComponents(Time.DeltaTime);
    }

    private void PhysicsUpdate()
    {
        ActiveScene.PhysicsUpdate(Time.DeltaTime);
        ActiveScene.PhysicsUpdateComponents(Time.DeltaTime);
    }

    private void Draw()
    {
        ActiveScene.Draw();
    }
}

sealed class RuntimeTime
{
    public float UnscaledDeltaTime { get; internal set; }

    public float DeltaTime { get; internal set; }

    public float TimeScale { get; set; } = 1f;
}
