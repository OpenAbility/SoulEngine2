namespace SoulEngine.SequenceScript.Machine;

public class DirectoryModuleResolver(string directory) : IModuleResolver
{

    public readonly string Directory = directory;

    public Stream LoadModule(string resolvePath)
    {
        return File.OpenRead(Path.Join(Directory, resolvePath));
    }
}