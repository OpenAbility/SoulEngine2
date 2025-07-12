using System.Diagnostics;
using Hexa.NET.ImGui;
using OpenAbility.Logging;
using OpenTK.Graphics.OpenGL;
using SoulEngine.Content;
using SoulEngine.Core.Tools;
using SoulEngine.Data;
using SoulEngine.Entities;
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

    public ImGuiWindow? WorkspaceGameWindow => Workspace.Current?.GameWindow;
    public ImGuiWindow? WorkspaceSceneWindow => Workspace.Current?.SceneWindow;
    
    public SceneCamera? WorkspaceSceneCamera => Workspace.Current?.SceneCamera;
    

    public bool Visible => (WorkspaceGameWindow?.Visible ?? false) && WorkspaceGameWindow.Active;

    public float AspectRatio => (WorkspaceGameWindow?.FramebufferSize.X ?? 0.0f) / (WorkspaceGameWindow?.FramebufferSize.Y ?? 1.0f);

#else
    public bool Visible => true;
    
    public float AspectRatio => (float)MainWindow.FramebufferSize.X / MainWindow.FramebufferSize.Y;
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
    

    private SceneRenderer2 sceneRenderer;
    
    public Scene? Scene { get; private set; }

    public readonly Window MainWindow;
    public readonly Thread MainThread;

    public readonly RenderContext RenderContext;
    
    public float DeltaTime { get; private set; }

    private float lastFrameDelta = -1;
    
    public IRenderPipeline RenderPipeline { get; private set; }

    internal readonly BuiltinActions BuiltinActions;

    private readonly UIContext uiContext;

    public readonly List<Action> UpdateHooks = new List<Action>();



    public Game(GameData data)
    {
        MainThread = Thread.CurrentThread;
        Current = this;
        
        GameData = data;
        EventBus = new EventBus<GameEvent>(true);
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
        RenderContext = new RenderContext(this, MainWindow);
        
        InputManager = new InputManager(this, InputBus);

        ImGuiRenderer = new ImGuiRenderer(ResourceManager);
        InputBus.BeginListen(ImGuiRenderer.OnInputEvent);
        




        BuiltinActions = new BuiltinActions(InputManager);
        Keys = new KeyActions(InputManager);
        
        
        Localizator = new Localizator(this);

        uiContext = new UIContext(this);
        
        sceneRenderer = new SceneRenderer2(this);
        
#if DEVELOPMENT

        RegisterTools();

        Workspace.Load(this);
#endif

    }
    
#if DEVELOPMENT
    protected virtual void CompileContent(ContentCompileContext compiler)
    {
        
    }

    protected virtual void RegisterTools()
    {
        EditorTool.Register("se.debugger", (game, workspace) => new DebuggerTool(game, workspace), "Tools", "Debugger");
        EditorTool.Register("se.edit.director", (game, workspace) => new DirectorTool(game, workspace), "Tools", "Game", "Director");
        EditorTool.Register("se.edit.inspector", (game, workspace) => new Inspector(game, workspace), "Tools", "Game", "Inspector");
        EditorTool.Register("se.edit.scene", (game, workspace) => new SceneView(game, workspace), "Tools", "Game", "Scene View");
        EditorTool.Register("se.viewer.input", (game, workspace) => new InputViewer(game, workspace), "Tools", "Input Viewer");
        EditorTool.Register("se.edit.engine_var", (game, workspace) => new EngineVarEditor(game, workspace), "Tools", "Engine Vars");
        
        EditorTool.Register("se.edit.event_sequence", (game, workspace) => new EventSequenceEditor(game, workspace), "Tools", "Event Sequence Editor");
        
        EditorTool.Register("se.viewer.game", (game, workspace) => new GameViewTool(game, workspace), "Tools", "Game Window");
        EditorTool.Register("se.viewer.scene", (game, workspace) => new SceneWindowTool(game, workspace), "Tools", "Scene Window");
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
        RenderPipeline = CreateDefaultRenderPipeline();
        
        EarlyLoad();
        
        LoadScene();
        
        GC.Collect();

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
            RunFrame(elapsed);
        }
    }

    private void RunFrame(TimeSpan delta)
    {
        Profiler.Instance.Reset();
        ProfilerSegment frameSegment = Profiler.Instance.Segment("frame");

        RenderContext.UseWindow(MainWindow);

        #region DELTA
        
        TimeSpan elapsed = delta;
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

        DeltaTime *= EngineVarContext.Global.GetFloat("e_timescale", 1.0f);

        EngineVarContext.Global.SetFloat("dt", DeltaTime);
        EngineVarContext.Global.SetInt("frameDelta", (int)(balancedFrameDelta * 1000));

        fpsHistogram.AddLast(1 / DeltaTime);
        msHistogram.AddLast((float)elapsed.TotalMilliseconds);

        if (fpsHistogram.Count > 30)
            fpsHistogram.RemoveFirst();
        if (msHistogram.Count > 30)
            msHistogram.RemoveFirst();

        #endregion
        
        // Fullscreen toggle
        if (Keys.Enter.Pressed && Keys.LeftAlt.Down)
        {
            MainWindow.Fullscreen = !MainWindow.Fullscreen;
        }

        ImGuiRenderer.BeginFrame(MainWindow, DeltaTime);

        ProfilerSegment updateSegment = Profiler.Instance.Segment("frame.update");
        Update();
        updateSegment.Dispose();
        
        ProfilerSegment renderSegment = Profiler.Instance.Segment("frame.render");
        Render(MainWindow);
        renderSegment.Dispose();

        // ImGui
        if (Development)
        {
            RenderPass imguiPass = new RenderPass();
            imguiPass.Name = "imgui";
            imguiPass.Surface = MainWindow;
            imguiPass.DepthStencilSettings.LoadOp = AttachmentLoadOp.Clear;

            RenderContext.BeginRendering(imguiPass);

            ImGuiRenderer.EndFrame(MainWindow, false);

            RenderContext.RebuildState();
            RenderContext.EndRendering();
        }
        else
        {
            ImGuiRenderer.EndFrame(MainWindow, true);
        }
       
        // End-of-frame stuff
        ProfilerSegment swapSegment = Profiler.Instance.Segment("frame.swap");
        MainWindow.Swap();
        swapSegment.Dispose();
        
        ProfilerSegment busSegment = Profiler.Instance.Segment("frame.bus");
        Window.Poll();
        InputBus.Dispatch();
        busSegment.Dispose();
        
        ProfilerSegment threadSegment = Profiler.Instance.Segment("frame.threading");
        ThreadSafety.RunTasks();
        threadSegment.Dispose();
        
#if DEVELOPMENT
        Workspace.Save(this);
#endif

        ProfilerSegment finishSegment = Profiler.Instance.Segment("frame.gpu_idle");
        GL.Finish();
        finishSegment.Dispose();
 
        
        frameSegment.Dispose();
        
    }

    protected virtual void PackContent()
    {
        
    }

    private void Update()
    {
        #if DEVELOPMENT
        
        
        #region FPS_DISPLAY
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


        ImGui.SetNextWindowPos(new Vector2(ImGui.GetIO().DisplaySize.X - 5, ImGui.GetIO().DisplaySize.Y - 5),
            ImGuiCond.Always, new Vector2(1, 1));
        ImGui.SetNextWindowBgAlpha(0.5f);
        if (ImGui.Begin("Info",
                ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text(" FPS: " + avgFPS.ToString("F0"));
            ImGui.Text(" MS: " + avgMS.ToString("F2"));
        }

        ImGui.End();

        ImGui.PopStyleVar();
        #endregion

        if (MenuContext.IsPressed("Content", "Pack"))
        {
            PackContent();
        }
        
        if (MenuContext.IsPressed("Scene View", "Camera", "Free") && WorkspaceSceneCamera != null)
        {
            WorkspaceSceneCamera.CameraMode = CameraMode.FreeCamera;
        }
        
        if (MenuContext.IsPressed("Scene View", "Camera", "Fly") && WorkspaceSceneCamera != null)
        {
            WorkspaceSceneCamera.CameraMode = CameraMode.FlyCamera;
        }
        
        if (MenuContext.IsPressed("Scene View", "Camera", "Game") && WorkspaceSceneCamera != null)
        {
            WorkspaceSceneCamera.CameraMode = CameraMode.GameCamera;
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

                foreach (var workspace in Workspace.Workspaces)
                {
                    workspace.CurrentEntity = null;
                }

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

        if (MenuContext.IsPressed("Workspace", "New"))
        {
            Workspace.Workspaces.Add(new Workspace(this));
        }
        
        ImGuizmo.Enable(true);

        InputManager.WindowOffset = WorkspaceGameWindow?.Position ?? Vector2.Zero;
        InputManager.WindowSize = new Vector2(WorkspaceGameWindow?.FramebufferSize.X ?? 0, WorkspaceGameWindow?.FramebufferSize.Y ?? 0);

        ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.Always);
        ImGui.SetNextWindowSize(ImGui.GetIO().DisplaySize, ImGuiCond.Always);
        
        if (ImGui.Begin("##workspaces",
                ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDocking |
                ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoNavInputs |
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.MenuBar))
        {
            // Draw the menu bar
            if (ImGui.BeginMenuBar())
            {
                MenuContext.Draw();
                ImGui.EndMenuBar();
            }
            
            if (ImGui.BeginTabBar("##workspace_tabbar",
                    ImGuiTabBarFlags.Reorderable | ImGuiTabBarFlags.AutoSelectNewTabs |
                    ImGuiTabBarFlags.FittingPolicyMask))
            {
                foreach (var workspace in Workspace.Workspaces)
                {
                    workspace.Update();
                }
                ImGui.EndTabBar();
            }


        }

        ImGui.End();

        EditorTool.DrawMenus(MenuContext);

        
#else
        InputManager.WindowOffset = new Vector2(0, 0);
        InputManager.WindowSize = new Vector2(MainWindow.FramebufferSize.X, MainWindow.FramebufferSize.Y);
#endif
        
        
        UpdateHook();

        Queue<Action> hooks = new Queue<Action>(UpdateHooks);
        foreach (var hook in hooks)
        {
            hook();
        }
        hooks.Clear();
        
        Scene?.Update(DeltaTime);
    }

    private void Render(Window window)
    {
        RenderContext.UseWindow(window);
        
        RenderHook(RenderContext);
        

#if DEVELOPMENT
        if (WorkspaceGameWindow?.Visible ?? false)
        {
            SceneRenderInformation renderInformation = new SceneRenderInformation
            {
                EntityCollection = Scene!,
                TargetSurface = WorkspaceGameWindow,
                DeltaTime = DeltaTime,
                UIContext = uiContext,
                RenderPipeline = RenderPipeline,
                RenderContext = RenderContext,
                
                CameraSettings = CameraSettings.Game,
                
                EnableCulling = EngineVarContext.Global.GetBool("e_cull"),
                PerformCullingPass = true,

                PostProcessing = EngineVarContext.Global.GetBool("e_post"),
                
                RenderUI = Scene!.Director!.RenderUI
            };

            sceneRenderer.PerformGameRender(renderInformation);
        }

        if (WorkspaceSceneWindow?.Visible ?? false)
        {
            SceneRenderInformation renderInformation = new SceneRenderInformation
            {
                EntityCollection = Scene!,
                TargetSurface = WorkspaceSceneWindow,
                DeltaTime = DeltaTime,
                UIContext = null,
                RenderPipeline = RenderPipeline,
                RenderContext = RenderContext,
                
                CameraSettings =  WorkspaceSceneCamera!.CreateCameraSettings(WorkspaceSceneWindow, MenuContext.IsFlagSet("View", "Gizmos"), Workspace.Current?.CurrentEntity),
                
                EnableCulling = EngineVarContext.Global.GetBool("e_cull"),
                PerformCullingPass = WorkspaceSceneCamera.CameraMode == CameraMode.FlyCamera,

                PostProcessing = EngineVarContext.Global.GetBool("e_post") &&  
                                 EngineVarContext.Global.GetBool("e_scene_post"),
            };

            sceneRenderer.PerformGameRender(renderInformation);
        }
#else
        SceneRenderInformation renderInformation = new SceneRenderInformation
        {
            EntityCollection = Scene!,
            TargetSurface = window,
            DeltaTime = DeltaTime,
            UIContext = uiContext,
            RenderPipeline = RenderPipeline,
            RenderContext = RenderContext,
                
            CameraSettings = CameraSettings.Game,
                
            EnableCulling = EngineVarContext.Global.GetBool("e_cull"),
            PerformCullingPass = true,

            PostProcessing = EngineVarContext.Global.GetBool("e_post"),
                
            RenderUI = Scene!.Director!.RenderUI
        };

        sceneRenderer.PerformGameRender(renderInformation);
#endif
    }
    

    public void FinalizeEngine()
    {
        EventBus.EventNow(new GameEvent(GameEvent.Finalizing));
        
        ThreadSafety.RunTasks();
        RenderContext.Dispose();
        MainWindow.Dispose();
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
        GC.Collect();
        Scene = scene;
        Scene.Director?.OnSceneMadeCurrent();
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
    
    
}

public struct GameData
{
    public string Name;
    public string Developer;
}