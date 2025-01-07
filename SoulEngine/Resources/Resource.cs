using SoulEngine.Content;
using SoulEngine.Core;

namespace SoulEngine.Resources;

/// <summary>
/// Base resource class
/// </summary>
public abstract class Resource : EngineObject
{
    public abstract Task Load(ResourceManager resourceManager, string id, ContentContext content);
}