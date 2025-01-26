namespace SoulEngine.Content;

public class PackedContentSource : IContentSource
{

    private readonly PackedContent content;
    public PackedContentSource(PackedContent packedContent)
    {
        content = packedContent;
    }
    
    private string Resolve(string id)
    {
        if (Path.IsPathRooted(id))
            id = id[1..];
        return id;
    }
    
    public Stream? LoadContent(string id)
    {
        if (!HasContent(id))
            return null;
        return new MemoryStream(content.LoadIntoMemory(id));
    }

    public bool HasContent(string id)
    {
        return content.Has(id);
    }
}