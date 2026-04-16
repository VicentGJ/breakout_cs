namespace Breakout;

abstract class Scene
{
    private int _nextGameObjectId = 1;

    public List<GameObject> Objects { get; } = [];

    public virtual void Update(Runtime runtime, InputState input)
    {
    }

    protected GameObject CreateGameObject(string name)
    {
        GameObject gameObject = new(_nextGameObjectId++, name);
        Objects.Add(gameObject);
        return gameObject;
    }
}
