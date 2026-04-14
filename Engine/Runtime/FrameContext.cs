namespace Breakout;

internal readonly record struct FrameContext(float DeltaTime, int ScreenWidth, int ScreenHeight, InputState Input);
