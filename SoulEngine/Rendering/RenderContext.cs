

using System.Diagnostics;
using System.Text;
using OpenAbility.Logging;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SDL3;
using SoulEngine.Core;
using SoulEngine.Data;

namespace SoulEngine.Rendering;

public unsafe partial class RenderContext : EngineObject, IDisposable
{
    private IntPtr glContext;
    private readonly GLDebugProc DebugProc;

    private Window currentWindow;

    public readonly bool SupportsLineDirectives;
    
    public RenderContext(Game game, Window contextOwner)
    {
        // Initialize OpenGL
        
        // Setup parameters
        SDL.GLSetAttribute(SDL.GLAttr.ContextMajorVersion, 4);
        SDL.GLSetAttribute(SDL.GLAttr.ContextMinorVersion, 5);
        SDL.GLSetAttribute(SDL.GLAttr.ContextProfileMask, (int)SDL.GLProfile.Core);
        //SDL.GLSetAttribute(SDL.GLAttr.FrameBufferSRGBCapable, 1);
        SDL.GLSetAttribute(SDL.GLAttr.DoubleBuffer, 0);
        SDL.GLSetAttribute(SDL.GLAttr.AcceleratedVisual, 1);
#if !RELEASE
        SDL.GLSetAttribute(SDL.GLAttr.ContextFlags, (int)SDL.GLContextFlag.Debug);
#endif

        glContext = SDL.GLCreateContext(contextOwner.Handle);
        
        
        SDL.GLMakeCurrent(contextOwner.Handle, glContext);
        currentWindow = contextOwner;
        GLLoader.LoadBindings(new SDLBindingsContext());
        
        if (!SDL.GLSetSwapInterval(-1))
        {
            Logger.Get<Window>().Warning("Adaptive V-Sync not available: " + SDL.GetError());
            if (!SDL.GLSetSwapInterval(1))
            {
                Logger.Get<Window>().Warning("V-Sync not available: " + SDL.GetError());
            }
        }

        if (!EngineVarContext.Global.GetBool("e_vsync", true))
            SDL.GLSetSwapInterval(0);

        int majorVersion = GL.GetInteger(GetPName.MajorVersion);
        int minorVersion = GL.GetInteger(GetPName.MinorVersion);
        string? renderer = GL.GetString(StringName.Renderer);
        string? glsl = GL.GetString(StringName.ShadingLanguageVersion);

        Logger.Get("Rendering", "Window").Debug("Running OpenGL {}.{}, GLSL {} on '{}'", majorVersion, minorVersion,
            glsl ?? "NULL", renderer ?? "NULL");

        const string outOfDateError = "Either OpenGL >= 4.3 and ARB_direct_state_access, or OpenGL >= 4.5 required!";

        if (majorVersion < 4)
            throw new Exception(outOfDateError);

        if (minorVersion < 3)
            throw new Exception(outOfDateError);

        SupportsLineDirectives = SDL.GLExtensionSupported("GL_ARB_shading_language_include");

        if (minorVersion < 5 && !SDL.GLExtensionSupported("ARB_direct_state_access"))
        {
            throw new Exception(outOfDateError);
        }

        // TODO: Maybe some people don't care?
        if (minorVersion < 6 && !SDL.GLExtensionSupported("GL_EXT_texture_filter_anisotropic"))
        {
            throw new Exception(outOfDateError);
        }

#if !RELEASE

        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);

        DebugProc = (source, type, id, severity, length, message, param) =>
        {
            if (type == DebugType.DebugTypePushGroup || type == DebugType.DebugTypePopGroup)
                return;

            if (severity == DebugSeverity.DebugSeverityNotification && !EngineVarContext.Global.GetBool("r_gl_notifs"))
                return;

            string msg =
                $"{source}, {type} ({severity}): {id}: {Encoding.UTF8.GetString(new Span<byte>((byte*)message, length))}";

            game.EventBus.Event(new RendererDebugCallback(msg));

            var logger = Logger.Get("Rendering", "OpenGL");
            if (type == DebugType.DebugTypeError)
                logger.Error(msg);
            else
                logger.Debug(msg);

            if (type == DebugType.DebugTypeError && EngineVarContext.Global.GetBool("r_gl_error_break"))
                Debugger.Break();
        };

        GL.DebugMessageCallback(DebugProc, IntPtr.Zero);

#endif
        
        
        RebuildState();
    }

    public void UseWindow(Window window)
    {
        SDL.GLMakeCurrent(window.Handle, glContext);
    }

    public Window GetCurrentWindow() => currentWindow;


    private bool disposed;
    public void Dispose()
    {
        if(disposed)
            return;
        disposed = true;
        SDL.GLDestroyContext(glContext);
    }
}