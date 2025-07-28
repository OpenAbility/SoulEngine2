using OpenAbility.Logging;
using SDL3;
using SoulEngine.Core;

namespace SoulEngine.Input;

public class Gamepad
{

    private static readonly Logger Logger = Logger.Get<Gamepad>();
    private static readonly Dictionary<uint, Gamepad> registered = new ();

    private readonly IntPtr gamepad;
    
    public readonly string Name;
    public readonly SDL.GamepadType Type;
    public bool Connected => SDL.GamepadConnected(gamepad);

    public int PlayerIndex
    {
        get => SDL.GetGamepadPlayerIndex(gamepad);
        set => SDL.SetGamepadPlayerIndex(gamepad, value);
    }
    
    
    
    public Gamepad(uint joystick)
    {
        gamepad = SDL.OpenGamepad(joystick);
        
        Name = SDL.GetGamepadName(gamepad) ?? "Invalid Gamepad";
        Type = SDL.GetGamepadType(gamepad);
    }

    public float Axis(SDL.GamepadAxis axis) => SDL.GetGamepadAxis(gamepad, axis) / 32767f;
    public bool Button(SDL.GamepadButton button) => SDL.GetGamepadButton(gamepad, button);
    public SDL.GamepadButtonLabel ButtonLabel(SDL.GamepadButton button) => SDL.GetGamepadButtonLabel(gamepad, button);

    public static void Scan()
    {
        uint[]? gamepads = SDL.GetGamepads(out _);
        if (gamepads == null)
        {
            Logger.Warning("Could not poll gamepads: {}", SDL.GetError());
            return;
        }
        
        foreach (var pad in gamepads)
        {
            if(registered.ContainsKey(pad))
                continue;
            Gamepad instance = new Gamepad(pad);
            registered.Add(pad, instance);
        }
        
    }
    
}