namespace Breakout;

sealed class Runtime
{
    private readonly BreakoutRenderer _breakoutRenderer;
    private readonly PaddleSystem _paddleSystem;
    private readonly BallSystem _ballSystem;
    private readonly CollisionSystem _collisionSystem;
    private readonly RoundSystem _roundSystem;

    public Runtime(Scene initialScene)
    {
        _breakoutRenderer = new BreakoutRenderer();
        _paddleSystem = new PaddleSystem();
        _ballSystem = new BallSystem();
        _collisionSystem = new CollisionSystem();
        _roundSystem = new RoundSystem(new LevelBuilder(), _ballSystem);
        Time = new RuntimeTime();
        ActiveScene = initialScene;

        InitializeScene(initialScene);
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
        InputState input = InputState.Poll();
        ActiveScene.Update(this, input);

        if (ActiveScene is not BreakoutScene scene)
        {
            return;
        }

        if (scene.Phase is GamePhase.Serving or GamePhase.Playing)
        {
            _paddleSystem.Update(scene, input, Time.DeltaTime);
        }

        switch (scene.Phase)
        {
            case GamePhase.Serving:
                _ballSystem.StickToPaddle(scene);

                if (input.LaunchPressed)
                {
                    _ballSystem.Launch(scene);
                }

                break;

            case GamePhase.Won:
            case GamePhase.Lost:
                if (input.ConfirmPressed)
                {
                    _roundSystem.StartNewGame(scene);
                }

                break;
        }
    }

    public void PhysicsUpdate()
    {
        if (ActiveScene is not BreakoutScene scene || scene.Phase != GamePhase.Playing)
        {
            return;
        }

        _ballSystem.Update(scene, Time.DeltaTime);
        _collisionSystem.Resolve(scene);
        _roundSystem.HandleBallOutOfBounds(scene);
    }

    public void Draw()
    {
        if (ActiveScene is BreakoutScene scene)
        {
            _breakoutRenderer.Draw(scene);
        }
    }

    public void ChangeScene(Scene scene)
    {
        ActiveScene = scene;
        InitializeScene(scene);
    }

    private void InitializeScene(Scene scene)
    {
        if (scene is BreakoutScene breakoutScene)
        {
            _roundSystem.StartNewGame(breakoutScene);
        }
    }
}

sealed class RuntimeTime
{
    public float UnscaledDeltaTime { get; internal set; }

    public float DeltaTime { get; internal set; }

    public float TimeScale { get; set; } = 1f;
}
