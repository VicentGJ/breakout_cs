using Raylib_cs;

namespace Breakout;

sealed class Application(int windowWidth, int windowHeight, string title, World initialWorld)
{
    private readonly RenderSystem _renderSystem = new();
    private AudioSystem? _audioSystem;

    public World World { get; } = initialWorld;

    public RuntimeTime Time { get; } = new();

    public void Run()
    {
        Raylib.InitWindow(windowWidth, windowHeight, title);
        Raylib.SetTargetFPS(120);

        try
        {
            _audioSystem = new AudioSystem();
            World.BindAudio(_audioSystem);
            World.SetupOnce();
            World.FlushSceneChange();

            while (!Raylib.WindowShouldClose())
            {
                BeginFrame(Raylib.GetFrameTime());
                Update();
                PhysicsUpdate();

                Raylib.BeginDrawing();
                Draw();
                Raylib.EndDrawing();
                World.FlushSceneChange();
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

    private void BeginFrame(float unscaledDeltaTime)
    {
        Time.UnscaledDeltaTime = unscaledDeltaTime;
        Time.DeltaTime = unscaledDeltaTime * Time.TimeScale;
    }

    private void Update()
    {
        World.Update(Time.DeltaTime);
        World.UpdateObjects(Time.DeltaTime);
    }

    private void PhysicsUpdate()
    {
        World.PhysicsUpdate(Time.DeltaTime);
        World.PhysicsUpdateObjects(Time.DeltaTime);
    }

    private void Draw()
    {
        Raylib.ClearBackground(World.BackgroundColor);
        _renderSystem.Draw(World.Objects);
    }
}

sealed class RuntimeTime
{
    public float UnscaledDeltaTime { get; internal set; }

    public float DeltaTime { get; internal set; }

    public float TimeScale { get; set; } = 1f;
}
