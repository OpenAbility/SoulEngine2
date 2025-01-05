using System.Diagnostics;
using OpenAbility.Logging;
using SoulEngine.Content;
using SoulEngine.Data;
using SoulEngine.Events;
using SoulEngine.Rendering;
using SoulEngine.Resources;
using SoulEngine.Util;
#if DEVELOPMENT
using SoulEngine.Development;
#endif

namespace SoulEngine.Core;

public abstract class Game
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
    public readonly ThreadSafety ThreadSafety;
    public readonly DataRegistry GameRegistry;
    
#if DEVELOPMENT
    public readonly DataRegistry DevelopmentRegistry;
#endif

    public readonly string PersistentDataPath;
    public readonly string BinaryDataPath;
    public readonly GameData GameData;

    public readonly EventBus<GameEvent> EventBus;

    private SceneRenderer? sceneRenderer;
    private Scene? scene;

    public readonly Window MainWindow;
    public readonly Thread MainThread;

    public Game(GameData data)
    {
        MainThread = Thread.CurrentThread;
        
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

        ThreadSafety = new ThreadSafety(this);
        

#if DEVELOPMENT
        
        DevelopmentRegistry = DataRegistry.CreateData(EventBus, PersistentPath("development.reg"));
        
        ContentCompileContext contentCompiler = new ContentCompileContext(this);
        CompileContent(contentCompiler);
#endif
        
        Content = new ContentContext();
        
        PopulateContent();

        EngineVar = new EngineVarContext(Content);
        ResourceManager = new ResourceManager(this);

        MainWindow = new Window(this, 1280, 720, data.Name, null);
    }
    
#if DEVELOPMENT
    protected virtual void CompileContent(ContentCompileContext compiler)
    {
        
    }
#endif

    protected abstract void PopulateContent();

    protected virtual void EarlyLoad()
    {
        return;
    }
    
    public void Run()
    {
        EarlyLoad();
        
        SetScene(new Scene());
        
        MainLoop();
    }

    public virtual void RenderHook()
    {
        
    }

    public virtual void UpdateHook()
    {
        
    }

    private void MainLoop()
    {
        MainWindow.Show();
        
        Stopwatch stopwatch = Stopwatch.StartNew();

        while (!MainWindow.ShouldClose)
        {
            TimeSpan elapsed = stopwatch.Elapsed;
            stopwatch.Restart();

            float deltaTime = (float)elapsed.TotalSeconds;
            EngineVar.SetFloat("dt", deltaTime);
            EngineVar.SetInt("frameDelta", (int)elapsed.TotalMilliseconds);
            
            UpdateHook();
            scene?.Update(deltaTime);
            
            
            MainWindow.BindFramebuffer();
            RenderUtility.Clear(Colour.Black, 0, 0);
            
            RenderHook();

            sceneRenderer?.Render(MainWindow);
            
            MainWindow.Swap();
            Window.Poll();
            ThreadSafety.RunTasks();
        }
    }

    public void FinalizeEngine()
    {
        EventBus.EventNow(new GameEvent(GameEvent.Finalizing));
        
        ThreadSafety.RunTasks();
    }

    public string PersistentPath(string path)
    {
        return Path.Join(PersistentDataPath, path);
    }
    
    public string BinaryPath(string path)
    {
        return Path.Join(BinaryDataPath, path);
    }

    public void SetScene(Scene scene)
    {
        this.scene = scene;
        this.sceneRenderer = new SceneRenderer(scene);
    }
}

public struct GameData
{
    public string Name;
    public string Developer;
}