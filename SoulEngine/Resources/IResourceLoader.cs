using SoulEngine.Content;

namespace SoulEngine.Resources;

public interface IResourceLoader<T>
{
    public T LoadResource(ResourceData data);
}