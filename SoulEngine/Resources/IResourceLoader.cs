using SoulEngine.Content;

namespace SoulEngine.Resources;

public interface IResourceLoader<T>
{
    public T LoadResource(ResourceManager resourceManager, string id, ContentContext content);
}