using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Hexa.NET.ImPlot;
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
public unsafe class Window : EngineObject, IRenderSurface, IDisposable
{
    static Window()
    {
        if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Gamepad | SDL.InitFlags.Joystick | SDL.InitFlags.Haptic))
        {
            SDL.Quit();
        }
    }
    
    private static readonly Dictionary<uint, Window> windowIds = new Dictionary<uint, Window>();
    
    public readonly IntPtr Handle;
    
    private bool shouldClose;

    private readonly Game Game;

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

        SDL.WindowFlags windowFlags = SDL.WindowFlags.Hidden | SDL.WindowFlags.OpenGL;

        if (EngineVarContext.Global.GetBool("e_resizable"))
            windowFlags |= SDL.WindowFlags.Resizable;

        if (EngineVarContext.Global.GetBool("e_hidpi"))
            windowFlags |= SDL.WindowFlags.HighPixelDensity;
        
        Handle = SDL.CreateWindow(title, width, height, windowFlags);

        if (EngineVarContext.Global.GetBool("e_hidpi"))
            SDL.ShowSimpleMessageBox(SDL.MessageBoxFlags.Warning, "HiDpi Warning",
                "The engine var 'e_hidpi' was set to true - this is still experimental!", Handle);
        
        windowIds[SDL.GetWindowID(Handle)] = this;
        
        // TODO: This should be toggled by the game whenever needed
        TextInput = true;
        
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
        SDL.HideWindow(Handle);
    }
    

    public static void Poll()
    {
        while (SDL.PollEvent(out var sdlEvent))
        {
            uint id = SDL.GetWindowID(SDL.GetWindowFromEvent(sdlEvent));

            if (windowIds.TryGetValue(id, out var registeredWindow))
                registeredWindow.Event(sdlEvent);

        }
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

    public void Swap()
    {
        SDL.GLSwapWindow(Handle);
    }

    /// <summary>
    /// Should this window close
    /// </summary>
    public bool ShouldClose
    {
        get => shouldClose;
        set => shouldClose = value;
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
            SDL.GetWindowSizeInPixels(Handle, out var w, out var h);

            return new Vector2i(w, h);
        }
    }

    public Vector2i WindowSize
    {
        get
        {
            SDL.GetWindowSize(Handle, out var w, out var h);
            return new Vector2i(w, h);
        }
    }

    public bool MouseCaptured
    {
        get => SDL.GetWindowRelativeMouseMode(Handle);
        set => SDL.SetWindowRelativeMouseMode(Handle, value);
    }

    private Vector2i pos;
    private Vector2i size;
    private bool fullscreen;

    public bool Fullscreen
    {
        get
        {
            return fullscreen;
        }

        set
        {
            SDL.SetWindowFullscreen(Handle, value);
        }
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

    private bool disposed;

    public void Dispose()
    {
        if(disposed)
            return;
        disposed = true;

        windowIds.Remove(SDL.GetWindowID(Handle));
        SDL.DestroyWindow(Handle);

    }

}