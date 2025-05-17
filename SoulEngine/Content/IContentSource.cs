namespace SoulEngine.Content;

public interface IContentSource
{
    public Stream? LoadContent(string id);
    public bool HasContent(string id);
    public IEnumerable<string> Search();
}