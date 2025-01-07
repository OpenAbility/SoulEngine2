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
    public readonly MenuContext MenuContext = new MenuContext();
    private readonly ContentCompileContext ContentCompileContext;
#endif
    
    private readonly ImGuiRenderer ImGuiRenderer;

    public readonly string PersistentDataPath;
    public readonly string BinaryDataPath;
    public readonly GameData GameData;

    public readonly EventBus<GameEvent> EventBus;
    public readonly EventBus<InputEvent> InputBus;

    private SceneRenderer? sceneRenderer;
    public Scene? Scene { get; private set; }

    public readonly Window MainWindow;
    public readonly Thread MainThread;

    private Task sceneLoadTask;
    
    public float DeltaTime { get; private set; }

    public GameState State;



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
        
        ContentCompileContext = new ContentCompileContext(this);
        CompileContent(ContentCompileContext);
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

    public void RefreshContent()
    {
        CompileContent(ContentCompileContext);
        ResourceManager.ReloadAll();
    }
    
#endif

    protected abstract void PopulateContent();

    protected virtual void EarlyLoad()
    {
        return;
    }
    
    public void Run()
    {
        State = GameState.Loading;
        
        EarlyLoad();
        
        sceneLoadTask = Task.Run(LoadScene);

        MainLoop();
    }

    public virtual void RenderHook()
    {
        
    }

    public virtual void UpdateHook()
    {
        
    }

    protected virtual Task LoadScene()
    {
        return Task.CompletedTask;
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

            DeltaTime = (float)elapsed.TotalSeconds;
            EngineVar.SetFloat("dt", DeltaTime);
            EngineVar.SetInt("frameDelta", (int)elapsed.TotalMilliseconds);

            fpsHistogram.AddLast(1 / DeltaTime);
            msHistogram.AddLast((float)elapsed.TotalMilliseconds);
            
            if(fpsHistogram.Count > 30)
                fpsHistogram.RemoveFirst();
            if(msHistogram.Count > 30)
                msHistogram.RemoveFirst();
            
#if DEVELOPMENT
            ImGuiRenderer.BeginFrame(MainWindow, DeltaTime);
            
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
            

            ImGui.SetNextWindowPos(new Vector2(ImGui.GetIO().DisplaySize.X - 5, ImGui.GetIO().DisplaySize.Y - 5), ImGuiCond.Always, new Vector2(1, 1));
            ImGui.SetNextWindowSize(new Vector2(100, 49), ImGuiCond.Always);
            ImGui.SetNextWindowBgAlpha(0.5f);
            if (ImGui.Begin("Info", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoInputs))
            {
                ImGui.Text(" FPS: " + avgFPS.ToString("F0"));
                ImGui.Text(" MS: " + avgMS.ToString("F2"));
            }
            ImGui.End();
            
            ImGui.PopStyleVar();

            if (ImGui.BeginMainMenuBar())
            {
                MenuContext.Draw();
                ImGui.EndMainMenuBar();
            }

            if (MenuContext.IsPressed("Content", "Refresh All"))
            {
                RefreshContent();
            }

            if (MenuContext.IsPressed("Content", "Pack"))
            {
                PackContent();
            }
            
            
#endif
            if(State == GameState.Running)
                RunningFrame();
            else if (State == GameState.Loading)
               LoadingFrame();
            else if(State == GameState.ReloadingAssets)
                ReloadingFrame();
            
#if DEVELOPMENT
            ImGuiRenderer.EndFrame(MainWindow);
#endif
            
            MainWindow.Swap();
            Window.Poll();
            ThreadSafety.RunTasks();
            InputBus.Dispatch();
        }
    }

    protected virtual void PackContent()
    {
        
    }

    private void LoadingFrame()
    {
        
        ImGui.SetNextWindowPos(ImGui.GetIO().DisplaySize / 2, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(700, 300));
        if (ImGui.Begin("LoadingGame", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.Modal))
        {
            if (sceneLoadTask.Status is TaskStatus.Running or TaskStatus.WaitingForActivation)
            {
                ImGui.Text("Loading Game...");
            } else if (sceneLoadTask.Status == TaskStatus.Canceled)
            {
                ImGui.Text("Game load canceled(???)");
            } else if (sceneLoadTask.Status == TaskStatus.Faulted)
            {
                ImGui.Text("Game load failed!");

                string input = sceneLoadTask.Exception?.ToString() ?? "NO ERROR";
                ImGui.InputTextMultiline("##", ref input, (uint)input.Length, ImGui.GetContentRegionAvail(),
                    ImGuiInputTextFlags.ReadOnly);
            } else if (sceneLoadTask.Status == TaskStatus.RanToCompletion)
            {
                ImGui.Text("Starting Game...");
                State = GameState.Running;
            }
            
        }
        ImGui.End();
        
    }

    private void ReloadingFrame()
    {
        Vector2 windowSize = new Vector2(200, 100);
        
        ImGui.SetNextWindowPos(ImGui.GetIO().DisplaySize / 2, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(windowSize);
        if (ImGui.Begin("ReloadingAssets", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.Modal))
        {
            ImGui.Text("Reloading assets!");
        }
        ImGui.End();
        
    }

    private void RunningFrame()
    {
        UpdateHook();
        Scene?.Update(DeltaTime);
            
            
        MainWindow.BindFramebuffer();
        RenderUtility.Clear(Colour.Black, 0, 0);
            
        RenderHook();

        sceneRenderer?.Render(MainWindow, DeltaTime);
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
        this.Scene = scene;
        this.sceneRenderer = new SceneRenderer(scene);
    }
}

public struct GameData
{
    public string Name;
    public string Developer;
}