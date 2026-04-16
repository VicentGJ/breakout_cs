namespace Breakout;

static class Program
{
    private const int WindowWidth = 960;
    private const int WindowHeight = 720;

    // STAThread is required if you deploy using NativeAOT on Windows.
    [System.STAThread]
    public static void Main()
    {
        var runtime = new Runtime(new BreakoutScene(WindowWidth, WindowHeight));
        var app = new Application(WindowWidth, WindowHeight, "Breakout", runtime);
        app.Run();
    }
}
