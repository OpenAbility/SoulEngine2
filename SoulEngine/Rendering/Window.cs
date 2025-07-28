using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Hexa.NET.ImPlot;
using OpenAbility.Logging;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SDL3;
using SoulEngine.Core;
using SoulEngine.Data;
using SoulEngine.Events;
using SoulEngine.Input;
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
            Game.InputBus.Event(new KeyEvent(FromSDL(sdlEvent.Key.Mod), FromSDL(sdlEvent.Key.Scancode), sdlEvent.Key.Repeat ? ButtonAction.Repeat : ButtonAction.Press));
            
        } else if (sdlEvent.Type == (uint)SDL.EventType.KeyUp)
        {
            Game.InputBus.Event(new KeyEvent(FromSDL(sdlEvent.Key.Mod), FromSDL(sdlEvent.Key.Scancode), ButtonAction.Release));
        } else if (sdlEvent.Type == (uint)SDL.EventType.TextInput)
        {
            string writtenString = Unsafe.GetString((byte*)sdlEvent.Text.Text);
            
            Game.InputBus.Event(new TypeEvent(writtenString));
        } else if (sdlEvent.Type == (uint)SDL.EventType.MouseWheel)
        {
            Game.InputBus.Event(new ScrollEvent(new Vector2(sdlEvent.Wheel.X, sdlEvent.Wheel.Y)));
        } else if (sdlEvent.Type == (uint)SDL.EventType.MouseButtonDown)
        {
            Game.InputBus.Event(new MouseEvent(0, FromSDLButton(sdlEvent.Button.Button), ButtonAction.Press));
        } else if (sdlEvent.Type == (uint)SDL.EventType.MouseButtonUp)
        {
            Game.InputBus.Event(new MouseEvent(0, FromSDLButton(sdlEvent.Button.Button), ButtonAction.Release));
        } else if (sdlEvent.Type == (uint)SDL.EventType.GamepadAdded)
        {
            Console.WriteLine("Plugged in gamepad!");
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


    private static KeyCode FromSDL(SDL.Scancode scancode)
    {
        return scancode switch
        {
            SDL.Scancode.Unknown => KeyCode.Unknown,
            SDL.Scancode.A => KeyCode.A,
            SDL.Scancode.B => KeyCode.B,
            SDL.Scancode.C => KeyCode.C,
            SDL.Scancode.D => KeyCode.D,
            SDL.Scancode.E => KeyCode.E,
            SDL.Scancode.F => KeyCode.F,
            SDL.Scancode.G => KeyCode.G,
            SDL.Scancode.H => KeyCode.H,
            SDL.Scancode.I => KeyCode.I,
            SDL.Scancode.J => KeyCode.J,
            SDL.Scancode.K => KeyCode.K,
            SDL.Scancode.L => KeyCode.L,
            SDL.Scancode.M => KeyCode.M,
            SDL.Scancode.N => KeyCode.N,
            SDL.Scancode.O => KeyCode.O,
            SDL.Scancode.P => KeyCode.P,
            SDL.Scancode.Q => KeyCode.Q,
            SDL.Scancode.R => KeyCode.R,
            SDL.Scancode.S => KeyCode.S,
            SDL.Scancode.T => KeyCode.T,
            SDL.Scancode.U => KeyCode.U,
            SDL.Scancode.V => KeyCode.V,
            SDL.Scancode.W => KeyCode.W,
            SDL.Scancode.X => KeyCode.X,
            SDL.Scancode.Y => KeyCode.Y,
            SDL.Scancode.Z => KeyCode.Z,
            SDL.Scancode.Alpha1 => KeyCode.D1,
            SDL.Scancode.Alpha2 => KeyCode.D2,
            SDL.Scancode.Alpha3 => KeyCode.D3,
            SDL.Scancode.Alpha4 => KeyCode.D4,
            SDL.Scancode.Alpha5 => KeyCode.D5,
            SDL.Scancode.Alpha6 => KeyCode.D6,
            SDL.Scancode.Alpha7 => KeyCode.D7,
            SDL.Scancode.Alpha8 => KeyCode.D8,
            SDL.Scancode.Alpha9 => KeyCode.D9,
            SDL.Scancode.Alpha0 => KeyCode.D0,
            SDL.Scancode.Return => KeyCode.Enter,
            SDL.Scancode.Escape => KeyCode.Escape,
            SDL.Scancode.Backspace => KeyCode.Backspace,
            SDL.Scancode.Tab => KeyCode.Tab,
            SDL.Scancode.Space => KeyCode.Space,
            SDL.Scancode.Minus => KeyCode.Minus,
            SDL.Scancode.Equals => KeyCode.Equal,
            SDL.Scancode.Leftbracket => KeyCode.LeftBracket,
            SDL.Scancode.Rightbracket => KeyCode.RightBracket,
            SDL.Scancode.Backslash => KeyCode.Backslash,
            SDL.Scancode.Semicolon => KeyCode.Semicolon,
            SDL.Scancode.Apostrophe => KeyCode.Apostrophe,
            SDL.Scancode.Grave => KeyCode.GraveAccent,
            SDL.Scancode.Comma => KeyCode.Comma,
            SDL.Scancode.Period => KeyCode.Period,
            SDL.Scancode.Slash => KeyCode.Slash,
            SDL.Scancode.Capslock => KeyCode.CapsLock,
            SDL.Scancode.F1 => KeyCode.F1,
            SDL.Scancode.F2 => KeyCode.F2,
            SDL.Scancode.F3 => KeyCode.F3,
            SDL.Scancode.F4 => KeyCode.F4,
            SDL.Scancode.F5 => KeyCode.F5,
            SDL.Scancode.F6 => KeyCode.F6,
            SDL.Scancode.F7 => KeyCode.F7,
            SDL.Scancode.F8 => KeyCode.F8,
            SDL.Scancode.F9 => KeyCode.F9,
            SDL.Scancode.F10 => KeyCode.F10,
            SDL.Scancode.F11 => KeyCode.F11,
            SDL.Scancode.F12 => KeyCode.F12,
            SDL.Scancode.Printscreen => KeyCode.PrintScreen,
            SDL.Scancode.Scrolllock => KeyCode.ScrollLock,
            SDL.Scancode.Pause => KeyCode.Pause,
            SDL.Scancode.Insert => KeyCode.Insert,
            SDL.Scancode.Home => KeyCode.Home,
            SDL.Scancode.Pageup => KeyCode.PageUp,
            SDL.Scancode.Delete => KeyCode.Delete,
            SDL.Scancode.End => KeyCode.End,
            SDL.Scancode.Pagedown => KeyCode.PageDown,
            SDL.Scancode.Right => KeyCode.Right,
            SDL.Scancode.Left => KeyCode.Left,
            SDL.Scancode.Down => KeyCode.Down,
            SDL.Scancode.Up => KeyCode.Up,
            SDL.Scancode.NumLockClear => KeyCode.NumLock,
            SDL.Scancode.KpDivide => KeyCode.KeyPadDivide,
            SDL.Scancode.KpMultiply => KeyCode.KeyPadMultiply,
            SDL.Scancode.KpMinus => KeyCode.KeyPadSubtract,
            SDL.Scancode.KpPlus => KeyCode.KeyPadAdd,
            SDL.Scancode.KpEnter => KeyCode.KeyPadEnter,
            SDL.Scancode.Kp1 => KeyCode.KeyPad1,
            SDL.Scancode.Kp2 => KeyCode.KeyPad2,
            SDL.Scancode.Kp3 => KeyCode.KeyPad3,
            SDL.Scancode.Kp4 => KeyCode.KeyPad4,
            SDL.Scancode.Kp5 => KeyCode.KeyPad5,
            SDL.Scancode.Kp6 => KeyCode.KeyPad6,
            SDL.Scancode.Kp7 => KeyCode.KeyPad7,
            SDL.Scancode.Kp8 => KeyCode.KeyPad8,
            SDL.Scancode.Kp9 => KeyCode.KeyPad9,
            SDL.Scancode.Kp0 => KeyCode.KeyPad0,
            SDL.Scancode.KpPeriod => KeyCode.KeyPadDecimal,
            SDL.Scancode.KpEquals => KeyCode.KeyPadEqual,
            SDL.Scancode.F13 => KeyCode.F13,
            SDL.Scancode.F14 => KeyCode.F14,
            SDL.Scancode.F15 => KeyCode.F15,
            SDL.Scancode.F16 => KeyCode.F16,
            SDL.Scancode.F17 => KeyCode.F17,
            SDL.Scancode.F18 => KeyCode.F18,
            SDL.Scancode.F19 => KeyCode.F19,
            SDL.Scancode.F20 => KeyCode.F20,
            SDL.Scancode.F21 => KeyCode.F21,
            SDL.Scancode.F22 => KeyCode.F22,
            SDL.Scancode.F23 => KeyCode.F23,
            SDL.Scancode.F24 => KeyCode.F24,
            SDL.Scancode.Menu => KeyCode.Menu,
            SDL.Scancode.LCtrl => KeyCode.LeftControl,
            SDL.Scancode.LShift => KeyCode.LeftShift,
            SDL.Scancode.LAlt => KeyCode.LeftAlt,
            SDL.Scancode.LGUI => KeyCode.LeftSuper,
            SDL.Scancode.RCtrl => KeyCode.RightControl,
            SDL.Scancode.RShift => KeyCode.RightShift,
            SDL.Scancode.RAlt => KeyCode.RightAlt,
            SDL.Scancode.RGUI => KeyCode.RightSuper,
            _ => KeyCode.Unknown
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
    

    public bool Fullscreen
    {
        get;

        set
        {
            SDL.SetWindowFullscreen(Handle, value);
            field = true;
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