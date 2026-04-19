namespace Breakout;

sealed class Runtime
{
    public Runtime(Scene initialScene)
    {
        Time = new RuntimeTime();
        ActiveScene = initialScene;
    }

    public Scene ActiveScene { get; private set; }

    public RuntimeTime Time { get; }

    public void BeginFrame(float unscaledDeltaTime)
    {
        Time.UnscaledDeltaTime = unscaledDeltaTime;
        Time.DeltaTime = unscaledDeltaTime * Time.TimeScale;
    }

    public void Update()
    {
        ActiveScene.Update(Time.DeltaTime);
        ActiveScene.UpdateBehaviors(Time.DeltaTime);
    }

    public void PhysicsUpdate()
    {
        ActiveScene.PhysicsUpdate(Time.DeltaTime);
        ActiveScene.PhysicsUpdateBehaviors(Time.DeltaTime);
    }

    public void Draw()
    {
        ActiveScene.Draw();
    }

    public void ChangeScene(Scene scene)
    {
        ActiveScene = scene;
    }
}

sealed class RuntimeTime
{
    public float UnscaledDeltaTime { get; internal set; }

    public float DeltaTime { get; internal set; }

    public float TimeScale { get; set; } = 1f;
}
