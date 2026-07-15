using Raylib_cs;

namespace Breakout;

sealed class Application(int windowWidth, int windowHeight, string title, Scene initialScene)
{
    private readonly RenderSystem _renderSystem = new();
    private AudioSystem? _audioSystem;

    public Scene ActiveScene { get; private set; } = initialScene;

    public RuntimeTime Time { get; } = new();

    public void Run()
    {
        Raylib.InitWindow(windowWidth, windowHeight, title);
        Raylib.SetTargetFPS(120);

        try
        {
            _audioSystem = new AudioSystem();
            ChangeScene(ActiveScene);

            while (!Raylib.WindowShouldClose())
            {
                BeginFrame(Raylib.GetFrameTime());
                Update();
                PhysicsUpdate();

                Raylib.BeginDrawing();
                Draw();
                Raylib.EndDrawing();
            }
        }
        finally
        {
            _renderSystem.Dispose();
            _audioSystem?.Dispose();
            _audioSystem = null;
            Raylib.CloseWindow();
        }
    }

    public void ChangeScene(Scene scene)
    {
        if (_audioSystem is null)
        {
            ActiveScene = scene;
            return;
        }

        _audioSystem.StopScene(ActiveScene);
        ActiveScene.UnbindAudio(_audioSystem);
        ActiveScene = scene;
        ActiveScene.BindAudio(_audioSystem);
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
        Raylib.ClearBackground(ActiveScene.BackgroundColor);
        _renderSystem.Draw(ActiveScene.Objects);
    }
}

sealed class RuntimeTime
{
    public float UnscaledDeltaTime { get; internal set; }

    public float DeltaTime { get; internal set; }

    public float TimeScale { get; set; } = 1f;
}
