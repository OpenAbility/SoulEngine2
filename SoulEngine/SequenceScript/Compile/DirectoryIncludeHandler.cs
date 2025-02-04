namespace SoulEngine.SequenceScript.Compile;

public class DirectoryIncludeHandler : ICompileIncludeHandler
{
    private readonly string directory;

    public DirectoryIncludeHandler(string directory)
    {
        this.directory = directory;
    }
    
    public FileInfo FindFile(string from, string path)
    {
        return new FileInfo(Path.Join(directory, path));
    }
}