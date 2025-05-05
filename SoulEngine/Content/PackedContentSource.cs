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
        return content.LoadIntoStream(id);
    }

    public bool HasContent(string id)
    {
        return content.Has(id);
    }

    public IEnumerable<string> Search(string prefix, string suffix)
    {
        yield break;
    }
}