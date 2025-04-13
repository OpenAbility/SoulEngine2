using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using OpenAbility.Logging;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SDL3;
using SoulEngine.Core;
using SoulEngine.Data;
using SoulEngine.Events;
using Unsafe = SoulEngine.Util.Unsafe;
using Vector2 = System.Numerics.Vector2;

namespace SoulEngine.Rendering;

/// <summary>
/// Provides a native window
/// </summary>
public unsafe class Window : IRenderSurface, IDisposable
{

#if !SDL
    private static readonly GLFWCallbacks.ErrorCallback GLFWError = (error, description) =>
    {
        Logger.Get("Rendering", "GLFW").Error("{}: {}", error, description);
    };
#endif



    static Window()
    {
#if SDL
        if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Gamepad | SDL.InitFlags.Joystick | SDL.InitFlags.Haptic))
        {
            SDL.Quit();
        }

#else

        if (!GLFW.Init())
        {
            GLFW.Terminate();
            throw new Exception("Could not initialize GLFW!");
        }

        GLFW.SetErrorCallback(GLFWError);

#endif
    }

#if SDL
    private static readonly Dictionary<uint, Window> windowIds = new Dictionary<uint, Window>();
    
    public readonly IntPtr Handle;
    private readonly nint GLContext;
    private bool shouldClose;
#else
    public readonly GLFWwindow* Handle;
#endif

    private readonly Game Game;
    private readonly GLDebugProc DebugProc;

#if !SDL
    private readonly GLFWCallbacks.CursorPosCallback cursorPosCallback;
    private readonly GLFWCallbacks.MouseButtonCallback mouseButtonCallback;
    private readonly GLFWCallbacks.KeyCallback keyCallback;
    private readonly GLFWCallbacks.CharCallback charCallback;
    private readonly GLFWCallbacks.ScrollCallback scrollCallback;
#endif

    /// <summary>
    /// Create a new window
    /// </summary>
    /// <param name="game">The game that owns this window</param>
    /// <param name="width">The window width</param>
    /// <param name="height">The window height</param>
    /// <param name="title">The window title</param>
    public Window(Game game, int width, int height, string title)
    {
        Game = game;

#if !SDL

        GLFW.WindowHint(WindowHintBool.Resizable, EngineVarContext.Global.GetBool("e_resizable"));
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

#else

        SDL.WindowFlags windowFlags = SDL.WindowFlags.Hidden | SDL.WindowFlags.OpenGL;

        if (EngineVarContext.Global.GetBool("e_resizable"))
            windowFlags |= SDL.WindowFlags.Resizable;

        if (EngineVarContext.Global.GetBool("e_hidpi"))
            windowFlags |= SDL.WindowFlags.HighPixelDensity;
        
        Handle = SDL.CreateWindow(title, width, height, windowFlags);

        if (EngineVarContext.Global.GetBool("e_hidpi"))
            SDL.ShowSimpleMessageBox(SDL.MessageBoxFlags.Warning, "HiDpi Warning",
                "The engine var 'e_hidpi' was set to true - this is still experimental!", Handle);
        
        SDL.GLSetAttribute(SDL.GLAttr.ContextMajorVersion, 4);
        SDL.GLSetAttribute(SDL.GLAttr.ContextMinorVersion, 5);
        SDL.GLSetAttribute(SDL.GLAttr.ContextProfileMask, (int)SDL.GLProfile.Core);
        SDL.GLSetAttribute(SDL.GLAttr.FrameBufferSRGBCapable, 1);
        SDL.GLSetAttribute(SDL.GLAttr.DoubleBuffer, 1);
        SDL.GLSetAttribute(SDL.GLAttr.AcceleratedVisual, 1);
        
#if !RELEASE
        SDL.GLSetAttribute(SDL.GLAttr.ContextFlags, (int)SDL.GLContextFlag.Debug);
#endif
        
        GLContext = SDL.GLCreateContext(Handle);


        
        SDL.GLMakeCurrent(Handle, GLContext);
        GLLoader.LoadBindings(new SDLBindingsContext());

        SDL.GLSetSwapInterval(1);

        windowIds[SDL.GetWindowID(Handle)] = this;
        
        // TODO: This should be toggled by the game whenever needed
        TextInput = true;
        
#endif




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

        if (minorVersion < 5 && !ExtensionSupported("ARB_direct_state_access"))
        {
            throw new Exception(outOfDateError);
        }

        // TODO: Maybe some people don't care?
        if (minorVersion < 6 && !ExtensionSupported("GL_EXT_texture_filter_anisotropic"))
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
    }

    private bool ExtensionSupported(string name)
    {
#if SDL
        return SDL.GLExtensionSupported(name);
#else
        return GLFW.ExtensionSupported(name);
#endif
    }

    /// <summary>
    /// Should this window be visible?
    /// </summary>
    public void Show()
    {
#if SDL
        SDL.ShowWindow(Handle);
#else
        GLFW.ShowWindow(Handle);
#endif
    }

    /// <summary>
    /// Should this window be hidden
    /// </summary>
    public void Hide()
    {
#if SDL
        SDL.HideWindow(Handle);
#else
        GLFW.HideWindow(Handle);
#endif
    }

    /// <summary>
    /// Make this the current window
    /// </summary>
    public void MakeCurrent()
    {
#if SDL
        SDL.GLMakeCurrent(Handle, GLContext);
#else
        GLFW.MakeContextCurrent(Handle);
#endif
    }

    public static void Poll()
    {
#if SDL
        while (SDL.PollEvent(out var sdlEvent))
        {
            uint id = SDL.GetWindowID(SDL.GetWindowFromEvent(sdlEvent));

            if (windowIds.TryGetValue(id, out var registeredWindow))
                registeredWindow.Event(sdlEvent);

        }
        
        
#else
        GLFW.PollEvents();
#endif
    }

    private void Event(SDL.Event sdlEvent)
    {
        if (sdlEvent.Type == (uint)SDL.EventType.Quit)
            shouldClose = true;
        else if (sdlEvent.Type == (uint)SDL.EventType.WindowCloseRequested)
            shouldClose = true;
        else if (sdlEvent.Type == (uint)SDL.EventType.MouseMotion)
        {
            Game.InputBus.Event(new CursorEvent(new Vector2(sdlEvent.Motion.X, sdlEvent.Motion.Y)));
        } else if (sdlEvent.Type == (uint)SDL.EventType.KeyDown)
        {
            Game.InputBus.Event(new KeyEvent(FromSDL(sdlEvent.Key.Mod), FromSDL(sdlEvent.Key.Scancode), sdlEvent.Key.Repeat ? InputAction.Repeat : InputAction.Press));
            
        } else if (sdlEvent.Type == (uint)SDL.EventType.KeyUp)
        {
            Game.InputBus.Event(new KeyEvent(FromSDL(sdlEvent.Key.Mod), FromSDL(sdlEvent.Key.Scancode), InputAction.Release));
        } else if (sdlEvent.Type == (uint)SDL.EventType.TextInput)
        {
            string writtenString = Unsafe.GetString((byte*)sdlEvent.Text.Text);
            
            Game.InputBus.Event(new TypeEvent(writtenString));
        } else if (sdlEvent.Type == (uint)SDL.EventType.MouseWheel)
        {
            Game.InputBus.Event(new ScrollEvent(new Vector2(sdlEvent.Wheel.X, sdlEvent.Wheel.Y)));
        } else if (sdlEvent.Type == (uint)SDL.EventType.MouseButtonDown)
        {
            Game.InputBus.Event(new MouseEvent(0, FromSDLButton(sdlEvent.Button.Button), InputAction.Press));
        } else if (sdlEvent.Type == (uint)SDL.EventType.MouseButtonUp)
        {
            Game.InputBus.Event(new MouseEvent(0, FromSDLButton(sdlEvent.Button.Button), InputAction.Release));
        }
        

    }
    
    #if SDL

    private static MouseButton FromSDLButton(byte button)
    {
        button -= 1;
        if (button == 1)
            button = 2;
        else if (button == 2)
            button = 1;
        return (MouseButton)button;
    }
    
    private static KeyModifiers FromSDL(SDL.Keymod keymod)
    {
        KeyModifiers modifiers = 0;

        if ((keymod & SDL.Keymod.Alt) != 0)
            modifiers |= KeyModifiers.Alt;
        if ((keymod & SDL.Keymod.Ctrl) != 0)
            modifiers |= KeyModifiers.Control;
        if ((keymod & SDL.Keymod.Shift) != 0)
            modifiers |= KeyModifiers.Shift;
        if ((keymod & SDL.Keymod.Caps) != 0)
            modifiers |= KeyModifiers.CapsLock;
        if ((keymod & SDL.Keymod.Num) != 0)
            modifiers |= KeyModifiers.NumLock;
        if ((keymod & SDL.Keymod.GUI) != 0)
            modifiers |= KeyModifiers.Super;
        
        return modifiers;
    }


    private static Keys FromSDL(SDL.Scancode scancode)
    {
        return scancode switch
        {
            SDL.Scancode.Unknown => Keys.Unknown,
            SDL.Scancode.A => Keys.A,
            SDL.Scancode.B => Keys.B,
            SDL.Scancode.C => Keys.C,
            SDL.Scancode.D => Keys.D,
            SDL.Scancode.E => Keys.E,
            SDL.Scancode.F => Keys.F,
            SDL.Scancode.G => Keys.G,
            SDL.Scancode.H => Keys.H,
            SDL.Scancode.I => Keys.I,
            SDL.Scancode.J => Keys.J,
            SDL.Scancode.K => Keys.K,
            SDL.Scancode.L => Keys.L,
            SDL.Scancode.M => Keys.M,
            SDL.Scancode.N => Keys.N,
            SDL.Scancode.O => Keys.O,
            SDL.Scancode.P => Keys.P,
            SDL.Scancode.Q => Keys.Q,
            SDL.Scancode.R => Keys.R,
            SDL.Scancode.S => Keys.S,
            SDL.Scancode.T => Keys.T,
            SDL.Scancode.U => Keys.U,
            SDL.Scancode.V => Keys.V,
            SDL.Scancode.W => Keys.W,
            SDL.Scancode.X => Keys.X,
            SDL.Scancode.Y => Keys.Y,
            SDL.Scancode.Z => Keys.Z,
            SDL.Scancode.Alpha1 => Keys.D1,
            SDL.Scancode.Alpha2 => Keys.D2,
            SDL.Scancode.Alpha3 => Keys.D3,
            SDL.Scancode.Alpha4 => Keys.D4,
            SDL.Scancode.Alpha5 => Keys.D5,
            SDL.Scancode.Alpha6 => Keys.D6,
            SDL.Scancode.Alpha7 => Keys.D7,
            SDL.Scancode.Alpha8 => Keys.D8,
            SDL.Scancode.Alpha9 => Keys.D9,
            SDL.Scancode.Alpha0 => Keys.D0,
            SDL.Scancode.Return => Keys.Enter,
            SDL.Scancode.Escape => Keys.Escape,
            SDL.Scancode.Backspace => Keys.Backspace,
            SDL.Scancode.Tab => Keys.Tab,
            SDL.Scancode.Space => Keys.Space,
            SDL.Scancode.Minus => Keys.Minus,
            SDL.Scancode.Equals => Keys.Equal,
            SDL.Scancode.Leftbracket => Keys.LeftBracket,
            SDL.Scancode.Rightbracket => Keys.RightBracket,
            SDL.Scancode.Backslash => Keys.Backslash,
            SDL.Scancode.Semicolon => Keys.Semicolon,
            SDL.Scancode.Apostrophe => Keys.Apostrophe,
            SDL.Scancode.Grave => Keys.GraveAccent,
            SDL.Scancode.Comma => Keys.Comma,
            SDL.Scancode.Period => Keys.Period,
            SDL.Scancode.Slash => Keys.Slash,
            SDL.Scancode.Capslock => Keys.CapsLock,
            SDL.Scancode.F1 => Keys.F1,
            SDL.Scancode.F2 => Keys.F2,
            SDL.Scancode.F3 => Keys.F3,
            SDL.Scancode.F4 => Keys.F4,
            SDL.Scancode.F5 => Keys.F5,
            SDL.Scancode.F6 => Keys.F6,
            SDL.Scancode.F7 => Keys.F7,
            SDL.Scancode.F8 => Keys.F8,
            SDL.Scancode.F9 => Keys.F9,
            SDL.Scancode.F10 => Keys.F10,
            SDL.Scancode.F11 => Keys.F11,
            SDL.Scancode.F12 => Keys.F12,
            SDL.Scancode.Printscreen => Keys.PrintScreen,
            SDL.Scancode.Scrolllock => Keys.ScrollLock,
            SDL.Scancode.Pause => Keys.Pause,
            SDL.Scancode.Insert => Keys.Insert,
            SDL.Scancode.Home => Keys.Home,
            SDL.Scancode.Pageup => Keys.PageUp,
            SDL.Scancode.Delete => Keys.Delete,
            SDL.Scancode.End => Keys.End,
            SDL.Scancode.Pagedown => Keys.PageDown,
            SDL.Scancode.Right => Keys.Right,
            SDL.Scancode.Left => Keys.Left,
            SDL.Scancode.Down => Keys.Down,
            SDL.Scancode.Up => Keys.Up,
            SDL.Scancode.NumLockClear => Keys.NumLock,
            SDL.Scancode.KpDivide => Keys.KeyPadDivide,
            SDL.Scancode.KpMultiply => Keys.KeyPadMultiply,
            SDL.Scancode.KpMinus => Keys.KeyPadSubtract,
            SDL.Scancode.KpPlus => Keys.KeyPadAdd,
            SDL.Scancode.KpEnter => Keys.KeyPadEnter,
            SDL.Scancode.Kp1 => Keys.KeyPad1,
            SDL.Scancode.Kp2 => Keys.KeyPad2,
            SDL.Scancode.Kp3 => Keys.KeyPad3,
            SDL.Scancode.Kp4 => Keys.KeyPad4,
            SDL.Scancode.Kp5 => Keys.KeyPad5,
            SDL.Scancode.Kp6 => Keys.KeyPad6,
            SDL.Scancode.Kp7 => Keys.KeyPad7,
            SDL.Scancode.Kp8 => Keys.KeyPad8,
            SDL.Scancode.Kp9 => Keys.KeyPad9,
            SDL.Scancode.Kp0 => Keys.KeyPad0,
            SDL.Scancode.KpPeriod => Keys.KeyPadDecimal,
            SDL.Scancode.KpEquals => Keys.KeyPadEqual,
            SDL.Scancode.F13 => Keys.F13,
            SDL.Scancode.F14 => Keys.F14,
            SDL.Scancode.F15 => Keys.F15,
            SDL.Scancode.F16 => Keys.F16,
            SDL.Scancode.F17 => Keys.F17,
            SDL.Scancode.F18 => Keys.F18,
            SDL.Scancode.F19 => Keys.F19,
            SDL.Scancode.F20 => Keys.F20,
            SDL.Scancode.F21 => Keys.F21,
            SDL.Scancode.F22 => Keys.F22,
            SDL.Scancode.F23 => Keys.F23,
            SDL.Scancode.F24 => Keys.F24,
            SDL.Scancode.Menu => Keys.Menu,
            SDL.Scancode.LCtrl => Keys.LeftControl,
            SDL.Scancode.LShift => Keys.LeftShift,
            SDL.Scancode.LAlt => Keys.LeftAlt,
            SDL.Scancode.LGUI => Keys.LeftSuper,
            SDL.Scancode.RCtrl => Keys.RightControl,
            SDL.Scancode.RShift => Keys.RightShift,
            SDL.Scancode.RAlt => Keys.RightAlt,
            SDL.Scancode.RGUI => Keys.RightSuper,
            _ => Keys.Unknown
        };
    }
    #endif

    public void Swap()
    {
#if SDL
        SDL.GLSwapWindow(Handle);
#else
        GLFW.SwapBuffers(Handle);
#endif

    }

    /// <summary>
    /// Should this window close
    /// </summary>
    public bool ShouldClose
    {
#if SDL
        get => shouldClose;
        set => shouldClose = value;
#else
        get => GLFW.WindowShouldClose(Handle);
        set => GLFW.SetWindowShouldClose(Handle, value);
#endif
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
#if SDL
            SDL.GetWindowSizeInPixels(Handle, out var w, out var h);
#else
            GLFW.GetFramebufferSize(Handle, out var w, out var h);
#endif

            return new Vector2i(w, h);
        }
    }

    public Vector2i WindowSize
    {
        get
        {
#if SDL
            SDL.GetWindowSize(Handle, out var w, out var h);
            return new Vector2i(w, h);
#else
            return FramebufferSize;
#endif
        }
    }

    public bool MouseCaptured
    {
#if SDL
        get => SDL.GetWindowRelativeMouseMode(Handle);
        set => SDL.SetWindowRelativeMouseMode(Handle, value);
#else
        get => GLFW.GetInputMode(Handle, CursorStateAttribute.Cursor) == CursorModeValue.CursorDisabled;
        set => GLFW.SetInputMode(Handle, CursorStateAttribute.Cursor, value ? CursorModeValue.CursorDisabled : CursorModeValue.CursorNormal);

#endif
    }

    private Vector2i pos;
    private Vector2i size;
    private bool fullscreen;

    public bool Fullscreen
    {
#if SDL

        get
        {
            return fullscreen;
        }

        set
        {
            SDL.SetWindowFullscreen(Handle, value);
        }

#else
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
       
#endif
    }

    public bool TextInput
    {
        get => SDL.TextInputActive(Handle);
        set
        {
            if (value)
                SDL.StartTextInput(Handle);
            else
                SDL.StopTextInput(Handle);
        }
    }



    public int GetSurfaceHandle()
    {
        return 0;
    }

    public void Dispose()
    {

        windowIds.Remove(SDL.GetWindowID(Handle));
        SDL.GLDestroyContext(Handle);
        SDL.DestroyWindow(Handle);

    }

}