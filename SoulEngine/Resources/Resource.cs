namespace SoulEngine.Resources;

/// <summary>
/// Base resource class
/// </summary>
public abstract class Resource
{
    public abstract void Load(ResourceManager resourceManager, string id);
}