using Raylib_cs;

namespace Breakout;

sealed class RenderSystem : IDisposable
{
    private readonly List<RenderEntry> _entries = [];
    private readonly Dictionary<string, Texture2D> _textures = [];

    public void Draw(IEnumerable<GameObject> objects)
    {
        _entries.Clear();

        foreach (GameObject gameObject in objects)
        {
            if (gameObject.ActiveInHierarchy && gameObject.VisibleInHierarchy)
            {
                _entries.Add(new RenderEntry(gameObject, gameObject.GetWorldTransform()));
            }
        }

        _entries.Sort(static (left, right) => left.GameObject.ZIndex.CompareTo(right.GameObject.ZIndex));

        foreach (RenderEntry entry in _entries)
        {
            entry.GameObject.Draw(this, entry.Transform);
        }
    }

    public void Dispose()
    {
        foreach (Texture2D texture in _textures.Values)
        {
            Raylib.UnloadTexture(texture);
        }

        _textures.Clear();
    }

    internal Texture2D GetTexture(string assetPath)
    {
        if (_textures.TryGetValue(assetPath, out Texture2D texture))
        {
            return texture;
        }

        texture = Raylib.LoadTexture(assetPath);

        if (!Raylib.IsTextureValid(texture))
        {
            throw new InvalidOperationException($"Could not load texture '{assetPath}'.");
        }

        Raylib.SetTextureFilter(texture, TextureFilter.Point);
        _textures.Add(assetPath, texture);
        return texture;
    }

    internal static Color Modulate(Color color, Color modulate)
    {
        return new Color(
            (byte)(color.R * modulate.R / 255),
            (byte)(color.G * modulate.G / 255),
            (byte)(color.B * modulate.B / 255),
            (byte)(color.A * modulate.A / 255));
    }

    private readonly record struct RenderEntry(GameObject GameObject, Transform2D Transform);
}
