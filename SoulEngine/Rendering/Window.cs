using System.Text;
using OpenAbility.Logging;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SoulEngine.Core;

namespace SoulEngine.Rendering;

using GLFWwindow = OpenTK.Windowing.GraphicsLibraryFramework.Window;

/// <summary>
/// Provides a native window
/// </summary>
public unsafe class Window : IRenderSurface
{

    private static readonly GLFWCallbacks.ErrorCallback GLFWError = (error, description) =>
    {
        Logger.Get("Rendering", "GLFW").Error("{}: {}", error, description);
    };


    static Window()
    {
        if (!GLFW.Init())
        {
            GLFW.Terminate();
            throw new Exception("Could not initialize GLFW!");
        }

        GLFW.SetErrorCallback(GLFWError);
    }

    public readonly GLFWwindow* Handle;

    private readonly GLDebugProc DebugProc;
    
    /// <summary>
    /// Create a new window
    /// </summary>
    /// <param name="width">The window width</param>
    /// <param name="height">The window height</param>
    /// <param name="title">The window title</param>
    /// <param name="parent">The parent window, for context sharing etc</param>
    public Window(Game game, int width, int height, string title, Window? parent)
    {
        GLFW.WindowHint(WindowHintBool.Resizable, game.EngineVar.GetBool("e_resizable"));
        GLFW.WindowHint(WindowHintBool.Visible, false);
        GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGlApi);
        GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 4);
        GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 5);
        GLFW.WindowHint(WindowHintBool.SrgbCapable, true);
        GLFW.WindowHint(WindowHintBool.ScaleToMonitor, true);
        
        #if !RELEASE
        GLFW.WindowHint(WindowHintBool.OpenGLDebugContext, true);
        #endif
        
        GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

        Handle = GLFW.CreateWindow(width, height, title, null, parent == null ? null : parent.Handle);

        if (Handle == null)
            throw new NullReferenceException("Window pointer is null!");
        
        GLFW.MakeContextCurrent(Handle);
        GLLoader.LoadBindings(new GLFWBindingsContext());

        int majorVersion = GL.GetInteger(GetPName.MajorVersion);
        int minorVersion = GL.GetInteger(GetPName.MinorVersion);
        string? renderer = GL.GetString(StringName.Renderer);
        string? glsl = GL.GetString(StringName.ShadingLanguageVersion);
        
        Logger.Get("Rendering", "Window").Debug("Running OpenGL {}.{}, GLSL {} on '{}'", majorVersion, minorVersion, glsl ?? "NULL", renderer ?? "NULL");

        const string outOfDateError = "Either OpenGL >= 4.2 and ARB_direct_state_access, or OpenGL >= 4.5 required!";
        
        if (majorVersion < 4)
            throw new Exception(outOfDateError);
        
        if (minorVersion < 5 && !GLFW.ExtensionSupported("ARB_direct_state_access"))
        {
            throw new Exception(outOfDateError);
        }
        
#if !RELEASE
        
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
        
        DebugProc = (source, type, id, severity, length, message, param) =>
        {
            string msg =
                $"{source}, {type} ({severity}): {id}: {Encoding.UTF8.GetString(new Span<byte>((byte*)message, length))}";
            
            game.EventBus.Event(new RendererDebugCallback(msg));
            Logger.Get("Rendering", "OpenGL").Debug(msg);
        };
        
        GL.DebugMessageCallback(DebugProc, IntPtr.Zero);
        
#endif
    }

    /// <summary>
    /// Should this window be visible?
    /// </summary>
    public void Show()
    {
        GLFW.ShowWindow(Handle);
    }
    
    /// <summary>
    /// Should this window be hidden
    /// </summary>
    public void Hide()
    {
        GLFW.HideWindow(Handle);
    }

    /// <summary>
    /// Make this the current window
    /// </summary>
    public void MakeCurrent()
    {
        GLFW.MakeContextCurrent(Handle);
    }

    public static void Poll()
    {
        GLFW.PollEvents();
    }

    public void Swap()
    {
        GLFW.SwapBuffers(Handle);
    }

    /// <summary>
    /// Should this window close
    /// </summary>
    public bool ShouldClose
    {
        get => GLFW.WindowShouldClose(Handle);
        set => GLFW.SetWindowShouldClose(Handle, value);
    }

    public void BindFramebuffer()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);
    }
    

    public Vector2i FramebufferSize
    {
        get
        {
            GLFW.GetFramebufferSize(Handle, out var w, out var h);
            return new Vector2i(w, h);
        }
    }
}