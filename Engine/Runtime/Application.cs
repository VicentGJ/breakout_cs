using Raylib_cs;

namespace Breakout;

sealed class Application(int windowWidth, int windowHeight, string title, World initialWorld)
{
    private readonly RenderSystem _renderSystem = new();
    private AudioSystem? _audioSystem;

    public World ActiveWorld { get; private set; } = initialWorld;

    public RuntimeTime Time { get; } = new();

    public void Run()
    {
        Raylib.InitWindow(windowWidth, windowHeight, title);
        Raylib.SetTargetFPS(120);

        try
        {
            _audioSystem = new AudioSystem();
            ChangeWorld(ActiveWorld);

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

    public void ChangeWorld(World world)
    {
        if (_audioSystem is null)
        {
            ActiveWorld = world;
            return;
        }

        _audioSystem.StopWorld(ActiveWorld);
        ActiveWorld.UnbindAudio(_audioSystem);
        ActiveWorld = world;
        ActiveWorld.BindAudio(_audioSystem);
        ActiveWorld.SetupOnce();
    }

    private void BeginFrame(float unscaledDeltaTime)
    {
        Time.UnscaledDeltaTime = unscaledDeltaTime;
        Time.DeltaTime = unscaledDeltaTime * Time.TimeScale;
    }

    private void Update()
    {
        ActiveWorld.Update(Time.DeltaTime);
        ActiveWorld.UpdateObjects(Time.DeltaTime);
    }

    private void PhysicsUpdate()
    {
        ActiveWorld.PhysicsUpdate(Time.DeltaTime);
        ActiveWorld.PhysicsUpdateObjects(Time.DeltaTime);
    }

    private void Draw()
    {
        Raylib.ClearBackground(ActiveWorld.BackgroundColor);
        _renderSystem.Draw(ActiveWorld.Objects);
    }
}

sealed class RuntimeTime
{
    public float UnscaledDeltaTime { get; internal set; }

    public float DeltaTime { get; internal set; }

    public float TimeScale { get; set; } = 1f;
}
