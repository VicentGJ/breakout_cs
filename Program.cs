namespace Breakout;

static class Program
{
    private const int WindowWidth = 960;
    private const int WindowHeight = 720;

    // STAThread is required if you deploy using NativeAOT on Windows.
    [System.STAThread]
    public static void Main()
    {
        var app = new GameApp(WindowWidth, WindowHeight, "Breakout", new BreakoutScene(WindowWidth, WindowHeight));
        app.Run();
    }
}
