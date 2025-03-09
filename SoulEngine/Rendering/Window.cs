using System.Diagnostics;
using System.Text;
using OpenAbility.Logging;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SoulEngine.Core;
using SoulEngine.Events;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;
using Vector2 = System.Numerics.Vector2;

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

    private readonly Game Game;

    private readonly GLDebugProc DebugProc;

    private readonly GLFWCallbacks.CursorPosCallback cursorPosCallback;
    private readonly GLFWCallbacks.MouseButtonCallback mouseButtonCallback;
    private readonly GLFWCallbacks.KeyCallback keyCallback;
    private readonly GLFWCallbacks.CharCallback charCallback;
    private readonly GLFWCallbacks.ScrollCallback scrollCallback;

    /// <summary>
    /// Create a new window
    /// </summary>
    /// <param name="game">The game that owns this window</param>
    /// <param name="width">The window width</param>
    /// <param name="height">The window height</param>
    /// <param name="title">The window title</param>
    /// <param name="parent">The parent window, for context sharing etc</param>
    public Window(Game game, int width, int height, string title, Window? parent)
    {
        Game = game;
        
        GLFW.WindowHint(WindowHintBool.Resizable, game.EngineVar.GetBool("e_resizable"));
        GLFW.WindowHint(WindowHintBool.Visible, false);
        GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGlApi);
        GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 4);
        GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 5);
        GLFW.WindowHint(WindowHintBool.SrgbCapable, false);
        GLFW.WindowHint(WindowHintBool.ScaleToMonitor, true);
        
        #if !RELEASE
        GLFW.WindowHint(WindowHintBool.OpenGLDebugContext, true);
        #endif
        
        GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

        Handle = GLFW.CreateWindow(width, height, title, null, parent == null ? null : parent.Handle);

        if (Handle == null)
            throw new NullReferenceException("Window pointer is null!");
        
        GLFW.MakeContextCurrent(Handle);
        
        cursorPosCallback = (window, x, y) =>
        {
            Game.InputBus.Event(new CursorEvent(new Vector2((float)x, (float)y)));
        };
        GLFW.SetCursorPosCallback(Handle, cursorPosCallback);

        mouseButtonCallback = (window, button, action, mods) =>
        {
            Game.InputBus.Event(new MouseEvent(mods, button, action));
        };
        GLFW.SetMouseButtonCallback(Handle, mouseButtonCallback);

        keyCallback = (window, key, code, action, mods) =>
        {
            Game.InputBus.Event(new KeyEvent(mods, key, action));
        };
        GLFW.SetKeyCallback(Handle, keyCallback);

        charCallback = (window, codepoint) =>
        {
            Game.InputBus.Event(new TypeEvent(codepoint));
        };
        GLFW.SetCharCallback(Handle, charCallback);

        scrollCallback = (window, x, y) =>
        {
            Game.InputBus.Event(new ScrollEvent(new Vector2((float)x, (float)y)));
        };
        GLFW.SetScrollCallback(Handle, scrollCallback);
        
        GLFW.SwapInterval(1);
        
        
        GLLoader.LoadBindings(new GLFWBindingsContext());

        int majorVersion = GL.GetInteger(GetPName.MajorVersion);
        int minorVersion = GL.GetInteger(GetPName.MinorVersion);
        string? renderer = GL.GetString(StringName.Renderer);
        string? glsl = GL.GetString(StringName.ShadingLanguageVersion);
        
        Logger.Get("Rendering", "Window").Debug("Running OpenGL {}.{}, GLSL {} on '{}'", majorVersion, minorVersion, glsl ?? "NULL", renderer ?? "NULL");

        const string outOfDateError = "Either OpenGL >= 4.3 and ARB_direct_state_access, or OpenGL >= 4.5 required!";
        
        if (majorVersion < 4)
            throw new Exception(outOfDateError);
        
        if (minorVersion < 3)
            throw new Exception(outOfDateError);
        
        if (minorVersion < 5 && !GLFW.ExtensionSupported("ARB_direct_state_access"))
        {
            throw new Exception(outOfDateError);
        }
        
        // TODO: Maybe some people don't care?
        if (minorVersion < 6 && !GLFW.ExtensionSupported("GL_EXT_texture_filter_anisotropic"))
        {
            throw new Exception(outOfDateError);
        }
        
#if !RELEASE
        
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
        
        DebugProc = (source, type, id, severity, length, message, param) =>
        {
            if(type == DebugType.DebugTypePushGroup || type == DebugType.DebugTypePopGroup)
                return;
            
            if(severity == DebugSeverity.DebugSeverityNotification && !Game.EngineVar.GetBool("r_gl_notifs"))
                return;
            
            string msg =
                $"{source}, {type} ({severity}): {id}: {Encoding.UTF8.GetString(new Span<byte>((byte*)message, length))}";

            game.EventBus.Event(new RendererDebugCallback(msg));

            var logger = Logger.Get("Rendering", "OpenGL");
            if(type == DebugType.DebugTypeError)
                logger.Error(msg);
            else
                logger.Debug(msg);
            
            if(type == DebugType.DebugTypeError && Game.EngineVar.GetBool("r_gl_error_break"))
                Debugger.Break();
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

    public bool MouseCaptured
    {
        get => GLFW.GetInputMode(Handle, CursorStateAttribute.Cursor) == CursorModeValue.CursorDisabled;
        set => GLFW.SetInputMode(Handle, CursorStateAttribute.Cursor, value ? CursorModeValue.CursorDisabled : CursorModeValue.CursorNormal);
    }

    private Vector2i pos;
    private Vector2i size;

    public bool Fullscreen
    {
        get => GLFW.GetWindowMonitor(Handle) != null;
        set
        {
            if (value && !Fullscreen)
            {
                GLFW.GetWindowPos(Handle, out var x, out var y);
                pos.X = x;
                pos.Y = y;
                
                GLFW.GetWindowSize(Handle, out var width, out var height);
                size.X = width;
                size.Y = height;
                
                Monitor* primaryMonitor = GLFW.GetPrimaryMonitor();

                var videoMode = GLFW.GetVideoMode(primaryMonitor);


                GLFW.SetWindowMonitor(Handle, GLFW.GetPrimaryMonitor(), 0, 0, videoMode->Width, videoMode->Height,
                    videoMode->RefreshRate);
            }
            else if(Fullscreen)
            {
                GLFW.SetWindowMonitor(Handle, null, pos.X, pos.Y, size.X, size.Y, GLFW.DontCare);
            }
        }
    }



    public int GetSurfaceHandle()
    {
        return 0;
    }
}