namespace Breakout;

// A loadable set of root objects. Until scene files exist, the content comes from `populate`,
// which adds objects with World.AddObject. Roots added while a scene is current belong to it.
sealed class Scene(string path, Action populate)
{
    public string Path { get; } = path;

    internal List<GameObject> Roots { get; } = [];

    internal void Load()
    {
        populate();
    }

    internal void Unload(World world)
    {
        foreach (GameObject root in Roots.ToList())
        {
            world.RemoveObject(root);
        }
    }
}

readonly record struct SceneUnloadingEvent(string Path) : ISceneEvent;

readonly record struct SceneLoadedEvent(string Path, string? PreviousPath) : ISceneEvent
{
    // Loading the scene that is already current is a restart.
    public bool IsRestart => Path == PreviousPath;
}
