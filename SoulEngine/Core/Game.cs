using System.Diagnostics;
using System.Numerics;
using OpenAbility.Logging;
using SoulEngine.Content;
using SoulEngine.Data;
using SoulEngine.Events;
using SoulEngine.Rendering;
using SoulEngine.Resources;
using SoulEngine.Util;
#if DEVELOPMENT
using ImGuiNET;
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
    private readonly ImGuiRenderer ImGuiRenderer;
#endif

    public readonly string PersistentDataPath;
    public readonly string BinaryDataPath;
    public readonly GameData GameData;

    public readonly EventBus<GameEvent> EventBus;
    public readonly EventBus<InputEvent> InputBus;

    private SceneRenderer? sceneRenderer;
    private Scene? scene;

    public readonly Window MainWindow;
    public readonly Thread MainThread;



    public Game(GameData data)
    {
        MainThread = Thread.CurrentThread;
        
        GameData = data;
        EventBus = new EventBus<GameEvent>();
        InputBus = new EventBus<InputEvent>();

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
        
#if DEVELOPMENT
        ImGuiRenderer = new ImGuiRenderer(ResourceManager);
        InputBus.BeginListen(ImGuiRenderer.OnInputEvent);
#endif
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

        MainLoop();
    }

    public virtual void RenderHook()
    {
        
    }

    public virtual void UpdateHook()
    {
        
    }

    private LinkedList<float> fpsHistogram = new LinkedList<float>();
    private LinkedList<float> msHistogram = new LinkedList<float>();

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

            fpsHistogram.AddLast(1 / deltaTime);
            msHistogram.AddLast((float)elapsed.TotalMilliseconds);
            
            if(fpsHistogram.Count > 30)
                fpsHistogram.RemoveFirst();
            if(msHistogram.Count > 30)
                msHistogram.RemoveFirst();
            
#if DEVELOPMENT
            ImGuiRenderer.BeginFrame(MainWindow, deltaTime);
            
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 10);

            float avgFPS = 0;
            foreach (var sample in fpsHistogram)
            {
                avgFPS += sample;
            }

            avgFPS /= fpsHistogram.Count;
            
            float avgMS = 0;
            foreach (var sample in msHistogram)
            {
                avgMS += sample;
            }

            avgMS /= msHistogram.Count;
            
            ImGui.SetNextWindowPos(new Vector2(5, 5), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(100, 49), ImGuiCond.Always);
            if (ImGui.Begin("Info", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar))
            {
                ImGui.Text(" FPS: " + avgFPS.ToString("F0"));
                ImGui.Text(" MS: " + avgMS.ToString("F2"));
            }
            ImGui.End();
            
            ImGui.PopStyleVar();
            
            
#endif
            
            UpdateHook();
            scene?.Update(deltaTime);
            
            
            MainWindow.BindFramebuffer();
            RenderUtility.Clear(Colour.Black, 0, 0);
            
            RenderHook();

            sceneRenderer?.Render(MainWindow);
            
#if DEVELOPMENT
            ImGuiRenderer.EndFrame(MainWindow);
#endif
            
            MainWindow.Swap();
            Window.Poll();
            ThreadSafety.RunTasks();
            InputBus.Dispatch();
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