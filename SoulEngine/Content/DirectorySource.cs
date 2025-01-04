namespace SoulEngine.Content;

public class DirectorySource : IContentSource
{
    public readonly string Directory;
    
    public DirectorySource(string path)
    {
        Directory = path;
    }

    private string Resolve(string id)
    {
        if (Path.IsPathRooted(id))
            id = id[1..];
        return Path.Join(Directory, id);
    }
    
    public Stream? LoadContent(string id)
    {
        string path = Resolve(id);
        if(File.Exists(path))
            return File.OpenRead(path);
        return null;
    }

    public bool HasContent(string id)
    {
        return File.Exists(Resolve(id));
    }
}