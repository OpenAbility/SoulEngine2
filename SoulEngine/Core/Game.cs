using System.Diagnostics;
using Hexa.NET.ImGui;
using OpenAbility.Logging;
using SoulEngine.Content;
using SoulEngine.Core.Tools;
using SoulEngine.Data;
using SoulEngine.Events;
using SoulEngine.Input;
using SoulEngine.Localization;
using SoulEngine.PostProcessing;
using SoulEngine.Renderer;
using SoulEngine.Rendering;
using SoulEngine.Resources;
using SoulEngine.UI;
using SoulEngine.Util;
using ImGuiWindow = SoulEngine.Rendering.ImGuiWindow;
using Vector2 = System.Numerics.Vector2;
#if DEVELOPMENT
using Hexa.NET.ImGuizmo;
using NativeFileDialogSharp;
using SoulEngine.Data.NBT;
using SoulEngine.Development;
using SoulEngine.Props;
#endif

namespace SoulEngine.Core;

public abstract class Game
{


    public static Game Current { get; private set; }
    
    public readonly Logger Logger;

#if DEVELOPMENT
    public const bool Development = true;
#else
    public const bool Development = false;
#endif

    public readonly ContentContext Content;
    public readonly ResourceManager ResourceManager;
    public readonly ThreadSafety ThreadSafety;
    public readonly DataRegistry GameRegistry;
    public readonly Localizator Localizator;
    
#if DEVELOPMENT
    public readonly DataRegistry DevelopmentRegistry;
    private readonly ContentCompileContext ContentCompileContext;

    private readonly ImGuiWindow GameWindow;
    private readonly ImGuiWindow SceneWindow;

    internal Prop? CurrentProp;
    
    private SceneCamera SceneCamera;

    public bool Visible => GameWindow.Visible && GameWindow.Active;

#else
    public bool Visible => true;
#endif
    
    public readonly MenuContext MenuContext = new MenuContext();
    
    private readonly ImGuiRenderer ImGuiRenderer;


    public readonly string PersistentDataPath;
    public readonly string BinaryDataPath;
    public readonly GameData GameData;

    public readonly EventBus<GameEvent> EventBus;
    public readonly EventBus<InputEvent> InputBus;

    public readonly InputManager InputManager;
    public readonly KeyActions Keys;
    

    private SceneRenderer? sceneRenderer;
    
    public Scene? Scene { get; private set; }

    public readonly Window MainWindow;
    public readonly Thread MainThread;

    public readonly RenderContext RenderContext;
    
    public float DeltaTime { get; private set; }

    private float lastFrameDelta = -1;
    

    public GameState State;
    
    public IRenderPipeline RenderPipeline { get; private set; }

    internal readonly BuiltinActions BuiltinActions;

    private readonly List<Tool> tools = new List<Tool>();

    private readonly UIContext uiContext;



    public Game(GameData data)
    {
        MainThread = Thread.CurrentThread;
        Current = this;
        
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
        
        if (Environment.GetCommandLineArgs().Contains("-compile"))
        {
            PackContent();
            Environment.Exit(0);
        }
        
#endif
        
        Content = new ContentContext();
        
        PopulateContent();

        EngineVarContext.Global = new EngineVarContext(Content);
        ResourceManager = new ResourceManager(this);
        ResourceManager.Global = ResourceManager;

        MainWindow = new Window(this, 1280, 720, data.Name);
        
        InputManager = new InputManager(this, InputBus);

        ImGuiRenderer = new ImGuiRenderer(ResourceManager);
        InputBus.BeginListen(ImGuiRenderer.OnInputEvent);
        

        RenderContext = new RenderContext();


        BuiltinActions = new BuiltinActions(InputManager);
        Keys = new KeyActions(InputManager);
        
#if DEVELOPMENT
        GameWindow = new ImGuiWindow(this, "Game");
        SceneWindow = new ImGuiWindow(this, "Scene");

        SceneCamera = new SceneCamera(this);
#endif
        
        Localizator = new Localizator(this);

        uiContext = new UIContext(this);
        
        AddTool(new DebuggerTool());
        AddTool(new DirectorTool());
        AddTool(new Inspector());
        AddTool(new SceneView());
        AddTool(new InputViewer());
        AddTool(new EngineVarEditor());
    }
    
#if DEVELOPMENT
    protected virtual void CompileContent(ContentCompileContext compiler)
    {
        
    }

    public void RefreshContent()
    {
        CompileContent(ContentCompileContext);
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
        
        RenderPipeline = CreateDefaultRenderPipeline();
        
        EarlyLoad();
        
        LoadScene();

        State = GameState.Running;

        MainLoop();
    }

    public virtual void RenderHook(RenderContext renderContext)
    {
        
    }

    public virtual void UpdateHook()
    {
        
    }

    protected virtual void LoadScene()
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

            float currentFrameDelta = (float)elapsed.TotalSeconds;
            float balancedFrameDelta = currentFrameDelta;

            if (lastFrameDelta == -1 || !EngineVarContext.Global.GetBool("e_dt_smoothing"))
                DeltaTime = currentFrameDelta;
            else
            {
                balancedFrameDelta = (currentFrameDelta + lastFrameDelta) / 2;
                lastFrameDelta = currentFrameDelta;
                
                DeltaTime = balancedFrameDelta;
            }

            if (DeltaTime > 1.0f)
                DeltaTime = 1.0f;
            
            
            
            EngineVarContext.Global.SetFloat("dt", DeltaTime);
            EngineVarContext.Global.SetInt("frameDelta", (int)(balancedFrameDelta * 1000));

            fpsHistogram.AddLast(1 / DeltaTime);
            msHistogram.AddLast((float)elapsed.TotalMilliseconds);
            
            if(fpsHistogram.Count > 30)
                fpsHistogram.RemoveFirst();
            if(msHistogram.Count > 30)
                msHistogram.RemoveFirst();
            
            if (Keys.Enter.Pressed && Keys.LeftAlt.Down)
            {
                MainWindow.Fullscreen = !MainWindow.Fullscreen;
            }
            
            ImGuiRenderer.BeginFrame(MainWindow, DeltaTime);
            
#if DEVELOPMENT
             
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
            ImGui.SetNextWindowBgAlpha(0.5f);
            if (ImGui.Begin("Info", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.AlwaysAutoResize))
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
                //RefreshContent();
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
            
            RenderPass imguiPass = new RenderPass();
            imguiPass.Name = "imgui";
            imguiPass.Surface = MainWindow;
            imguiPass.DepthStencilSettings.LoadOp = AttachmentLoadOp.Clear;
            
            if(Development)
                RenderContext.BeginRendering(imguiPass);
            ImGuiRenderer.EndFrame(MainWindow, !Development);
            if(Development)
                RenderContext.RebuildState();
            if(Development)
                RenderContext.EndRendering();
            
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
        if (ImGui.Begin("LoadingGame",
                ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.Modal))
        {
            ImGui.Text("Loading Game...");
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
#if DEVELOPMENT

        ImGui.DockSpaceOverViewport();


        if (MenuContext.IsPressed("Scene View", "Camera", "Free"))
        {
            SceneCamera.CameraMode = CameraMode.FreeCamera;
        }
        
        if (MenuContext.IsPressed("Scene View", "Camera", "Fly"))
        {
            SceneCamera.CameraMode = CameraMode.FlyCamera;
        }
        
        if (MenuContext.IsPressed("Scene View", "Camera", "Game"))
        {
            SceneCamera.CameraMode = CameraMode.GameCamera;
        }

        if (MenuContext.IsPressed("File", "New"))
        { 
            SetScene(new Scene(this));
        }
        
        if (MenuContext.IsPressed("File", "Open"))
        {
            var result = Dialog.FileOpen("scene_s;scene", "content_src");
            if (result.IsOk)
            {
                Logger.Info("Loading from {}" , result.Path);

                CurrentProp = null;

                if (result.Path.EndsWith(".scene"))
                {
                    using FileStream stream = File.OpenRead(result.Path);
                    SetScene(Scene.Loader.Load(this, (CompoundTag)TagIO.ReadCompressed(stream)));
                }
                else
                {
                                
                    SetScene(Scene.Loader.Load(this, (CompoundTag)TagIO.ReadSNBT(File.ReadAllText(result.Path))));
                }
            }
        }

        if (MenuContext.IsPressed("File", "Save"))
        {
            var result = Dialog.FileSave("scene_s", "content_src/my_scene.scene_s");
            if (result.IsOk)
            {
                Logger.Info("Saving to {}" , result.Path);
                File.WriteAllText(result.Path, TagIO.WriteSNBT(Scene!.Write()));
            }
        }

        foreach (var tool in tools)
        {
            if (MenuContext.IsPressed(tool.GetToolPath()))
            {
                tool.Enabled = !tool.Enabled;
            }
        }
        
        
        SceneWindow.Draw(false, () =>
        {
            ImGuizmo.SetDrawlist();
            Vector2 size = ImGui.GetContentRegionAvail();
            Vector2 position = ImGui.GetCursorScreenPos();
            ImGuizmo.SetRect(position.X, position.Y, size.X, size.Y);
        }, () =>
        {
            if (Scene != null && CurrentProp != null)
            {
                CurrentProp.RenderMoveGizmo(SceneCamera.GetView(), SceneCamera.GetProjection((float)SceneWindow.FramebufferSize.X / SceneWindow.FramebufferSize.Y));
            }
            
            SceneCamera.Update(DeltaTime, ImGui.IsWindowFocused());
            
        });
        GameWindow.Draw(false, null, null);
        
        ImGuizmo.Enable(true);

        InputManager.WindowOffset = GameWindow.Position;
        InputManager.WindowSize = new Vector2(GameWindow.FramebufferSize.X, GameWindow.FramebufferSize.Y);

        foreach (var tool in tools)
        {
            if(tool.Enabled)
                tool.Perform(this);
#if DEVELOPMENT
            DevelopmentRegistry.SetInt32("_EDITOR_TOOLS." + tool.GetType() + ".enabled", tool.Enabled ? 1 : 0);
#endif
        }

        
#else
        InputManager.WindowOffset = new Vector2(0, 0);
        InputManager.WindowSize = new Vector2(MainWindow.FramebufferSize.X, MainWindow.FramebufferSize.Y);
#endif
        
        
        UpdateHook();
        Scene?.Update(DeltaTime);
            
        RenderHook(RenderContext);

#if DEVELOPMENT

        CameraSettings sceneWindowSettings = new CameraSettings();
        sceneWindowSettings.CameraMode = SceneCamera.CameraMode;
        sceneWindowSettings.ProjectionMatrix = SceneCamera.GetProjection((float)SceneWindow.FramebufferSize.X / SceneWindow.FramebufferSize.Y);
        sceneWindowSettings.ViewMatrix = SceneCamera.GetView();
        sceneWindowSettings.ShowGizmos = MenuContext.IsFlagSet("View", "Gizmos");
        sceneWindowSettings.SelectedProp = CurrentProp;
        sceneWindowSettings.ShowUI = false;
        
        sceneWindowSettings.CameraPosition = SceneCamera.Position;
        sceneWindowSettings.CameraDirection = SceneCamera.Forward;
        sceneWindowSettings.CameraRight = SceneCamera.Right;
        sceneWindowSettings.CameraUp = SceneCamera.Up;
        sceneWindowSettings.FieldOfView = SceneCamera.FOV;
        sceneWindowSettings.NearPlane = SceneCamera.Near;
        sceneWindowSettings.FarPlane = SceneCamera.Far;

        if (GameWindow.Visible)
        {
            PipelineData pipelineData = new PipelineData();
            pipelineData.Game = this;
            pipelineData.RenderContext = RenderContext;
            pipelineData.CameraSettings = sceneRenderer?.MakePipelineCamera(CameraSettings.Game, GameWindow) ??
                                          new CameraSettings();
            pipelineData.DeltaTime = DeltaTime;
            pipelineData.TargetSurface = GameWindow;
            pipelineData.UIContext = uiContext;
            
            
            sceneRenderer?.Render(RenderPipeline, GameWindow, DeltaTime, CameraSettings.Game, uiContext);
            sceneRenderer?.RenderGizmo(ref pipelineData);
            RenderPipeline.DrawFrame(pipelineData);
        }

        if (SceneWindow.Visible)
        {
            PipelineData pipelineData = new PipelineData();
            pipelineData.Game = this;
            pipelineData.RenderContext = RenderContext;
            pipelineData.CameraSettings = sceneRenderer?.MakePipelineCamera(sceneWindowSettings, SceneWindow) ??
                                          new CameraSettings();
            pipelineData.DeltaTime = DeltaTime;
            pipelineData.TargetSurface = SceneWindow;

            
            sceneRenderer?.Render(RenderPipeline, SceneWindow, DeltaTime, sceneWindowSettings, uiContext);
            sceneRenderer?.RenderGizmo(ref pipelineData);
            RenderPipeline.DrawFrame(pipelineData);
        }
#else
        PipelineData pipelineData = new PipelineData();
        pipelineData.Game = this;
        pipelineData.RenderContext = RenderContext;
        pipelineData.CameraSettings = sceneRenderer?.MakePipelineCamera(CameraSettings.Game, MainWindow) ??
                                      new CameraSettings();
        pipelineData.DeltaTime = DeltaTime;
        pipelineData.TargetSurface = MainWindow;
        pipelineData.UIContext = uiContext;
        
        sceneRenderer?.Render(RenderPipeline, MainWindow, DeltaTime, CameraSettings.Game, uiContext);
        sceneRenderer?.RenderGizmo(ref pipelineData);
        
        RenderContext.RebuildState();
        RenderPipeline.DrawFrame(pipelineData);
        RenderContext.RebuildState();
#endif
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

    protected abstract IRenderPipeline CreateDefaultRenderPipeline();

    public void MessageBox(MessageBoxType type, string title, string description)
    {
#if !SDL
        SDL.ShowSimpleMessageBox(type switch
        {
            MessageBoxType.Error => SDL.MessageBoxFlags.Error,
            MessageBoxType.Info => SDL.MessageBoxFlags.Information,
            MessageBoxType.Warning => SDL.MessageBoxFlags.Warning,
            _ => SDL.MessageBoxFlags.Information
        }, title, description, MainWindow.Handle);
#else
        Logger.Error("Message box not supported outside SDL3!");
        Logger.Info("[{}]({}): {}", title, type, description);
#endif
    }
    
    public abstract class Tool
    {
        public bool Enabled;
        
        public abstract void Perform(Game game);

        public abstract string[] GetToolPath();
    }

    protected void AddTool(Tool tool)
    {
#if DEVELOPMENT
        if (DevelopmentRegistry.Exists("_EDITOR_TOOLS." + tool.GetType() + ".enabled"))
            tool.Enabled = DevelopmentRegistry.GetInt32("_EDITOR_TOOLS." + tool.GetType() + ".enabled") > 0;
#endif
        
        tools.Add(tool);
    }
    
}

public struct GameData
{
    public string Name;
    public string Developer;
}