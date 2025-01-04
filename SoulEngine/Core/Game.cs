using OpenAbility.Logging;
using SoulEngine.Content;
using SoulEngine.Data;
using SoulEngine.Events;
using SoulEngine.Resources;
#if DEVELOPMENT
using SoulEngine.Development;
#endif

namespace SoulEngine.Core;

public class Game
{
    public readonly Logger Logger;

#if DEVELOPMENT
    public const bool Development = true;
#else
    public const bool Development = false;
#endif

    public readonly ContentContext Content;
    public readonly EngineVarContext EngineVar;
    public readonly ResourceManager ResourceManager;

    public readonly DataRegistry GameRegistry;
    
#if DEVELOPMENT
    public readonly DataRegistry DevelopmentRegistry;
#endif

    public readonly string PersistentDataPath;
    public readonly string BinaryDataPath;
    public readonly GameData GameData;

    public readonly EventBus<GameEvent> EventBus;

    public Game(GameData data)
    {
        GameData = data;
        EventBus = new EventBus<GameEvent>();

        PersistentDataPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            data.Developer, data.Name);

        BinaryDataPath = AppDomain.CurrentDomain.BaseDirectory;

        if (!Directory.Exists(PersistentDataPath))
            Directory.CreateDirectory(PersistentDataPath);
        
        if (!Directory.Exists(PersistentPath("logs")))
            Directory.CreateDirectory(PersistentPath("logs"));
        
        Logger.RegisterLogFile(PersistentPath("logs/latest.log"));
        Logger.RegisterLogFile(PersistentPath($"logs/{DateTime.Now:yyyy-M dd}.log"));
        
        Logger = Logger.Get("Game", data.Name);
        
        Logger.Debug("Persistent data at '{}', binary data at '{}'", PersistentDataPath, BinaryDataPath);
        
        GameRegistry = DataRegistry.CreateData(EventBus, PersistentPath("game.reg"));
        

#if DEVELOPMENT
        
        DevelopmentRegistry = DataRegistry.CreateData(EventBus, PersistentPath("development.reg"));
        
        ContentCompileContext contentCompiler = new ContentCompileContext(this);
        CompileContent(contentCompiler);
#endif
        
        Content = new ContentContext();

        EngineVar = new EngineVarContext(Content);
        ResourceManager = new ResourceManager();
    }
    
#if DEVELOPMENT
    protected virtual void CompileContent(ContentCompileContext compiler)
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

    public void FinalizeEngine()
    {
        EventBus.EventNow(new GameEvent(GameEvent.Finalizing));
    }

    public string PersistentPath(string path)
    {
        return Path.Join(PersistentDataPath, path);
    }
    
    public string BinaryPath(string path)
    {
        return Path.Join(BinaryDataPath, path);
    }
}

public struct GameData
{
    public string Name;
    public string Developer;
}