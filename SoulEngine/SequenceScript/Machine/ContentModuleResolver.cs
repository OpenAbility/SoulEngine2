using SoulEngine.Content;

namespace SoulEngine.SequenceScript.Machine;

public class ContentModuleResolver : IModuleResolver
{
    private readonly ContentContext context;
    private readonly string directory;
    
    public ContentModuleResolver(ContentContext context, string directory)
    {
        this.context = context;
        this.directory = directory;
    }
    
    public Stream LoadModule(string resolvePath)
    {
        return context.Load(Path.Join(directory, resolvePath)) ?? Stream.Null;
    }
}