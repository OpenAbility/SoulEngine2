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

    private IEnumerable<string> RecursiveSearch(DirectoryInfo directory, string currentPath)
    {
        currentPath += directory.Name + "/";
        foreach (var file in directory.GetFiles())
        {
            yield return currentPath + file.Name;
        }
        
        foreach (var dir in directory.GetDirectories())
        {
            foreach (var result in RecursiveSearch(dir, currentPath))
            {
                yield return result;
            }
        }
    }

    public IEnumerable<string> Search()
    {
        DirectoryInfo directory = new DirectoryInfo(Directory);

        // Avoid including the root dir
        
        foreach (var file in directory.GetFiles())
        {
            yield return file.Name;
        }
        
        foreach (var dir in directory.GetDirectories())
        {
            foreach (var result in RecursiveSearch(dir, ""))
            {
                yield return result;
            }
        }
    }
}