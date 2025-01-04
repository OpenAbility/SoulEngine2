using SoulEngine.Content;
using SoulEngine.Data;
using SoulEngine.Resources;
#if DEVELOPMENT
using Soul.Development;
#endif

namespace SoulEngine.Core;

public class Game
{

    public readonly ContentContext Content;
    public readonly EngineVarContext EngineVar;
    public readonly ResourceManager ResourceManager;

    public Game()
    {

#if DEVELOPMENT
        ContentCompiler contentCompiler = new ContentCompiler();
        CompileContent(contentCompiler);
#endif
        
        Content = new ContentContext();

        EngineVar = new EngineVarContext(Content);
        ResourceManager = new ResourceManager();
    }
    
#if DEVELOPMENT
    protected virtual void CompileContent(ContentCompiler compiler)
    {
        
    }
#endif

    protected virtual void PopulateContent()
    {
        Content.Mount(new DirectorySource("data_builtin"));
    }
    
    public void Run()
    {
        
    }
}