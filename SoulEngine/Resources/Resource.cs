using SoulEngine.Content;

namespace SoulEngine.Resources;

/// <summary>
/// Base resource class
/// </summary>
public abstract class Resource
{
    public abstract Task Load(ResourceManager resourceManager, string id, ContentContext content);
}