using Raylib_cs;

namespace Breakout;

readonly record struct InputState(bool MoveLeft, bool MoveRight, bool LaunchPressed, bool ConfirmPressed)
{
    public float HorizontalAxis
    {
        get
        {
            float axis = 0f;

            if (MoveLeft)
            {
                axis -= 1f;
            }

            if (MoveRight)
            {
                axis += 1f;
            }

            return axis;
        }
    }

    public static InputState Poll()
    {
        return new InputState(
            Raylib.IsKeyDown(KeyboardKey.Left) || Raylib.IsKeyDown(KeyboardKey.A),
            Raylib.IsKeyDown(KeyboardKey.Right) || Raylib.IsKeyDown(KeyboardKey.D),
            Raylib.IsKeyPressed(KeyboardKey.Space),
            Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsKeyPressed(KeyboardKey.Enter));
    }
}
