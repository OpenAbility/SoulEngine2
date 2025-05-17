namespace SoulEngine.Content;

public class TemporaryResourceSource : IContentSource
{
    private readonly Dictionary<string, Stream> content = new Dictionary<string, Stream>();
    
    public Stream? LoadContent(string id)
    {
        if (content.TryGetValue(id, out var stream))
            return stream;
        return null;
    }

    public bool HasContent(string id)
    {
        return content.ContainsKey(id);
    }

    public IEnumerable<string> Search()
    {
        yield break;
    }

    private static ulong ResourceID = 1000;
    
    public string Register(Stream stream)
    {
        string name = "__TEMPORARY_RESOURCE_ID_" + (ResourceID++);
        content[name] = stream;
        return name;
    }

    public void Free(string id)
    {
        content.Remove(id);
    }
}