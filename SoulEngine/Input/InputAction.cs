using SDL3;
using SoulEngine.Core;
using SoulEngine.Events;

namespace SoulEngine.Input;

public class InputAction : EngineObject
{
    private readonly InputManager manager;
    
    public InputAction(InputManager manager, EventBus<InputEvent> eventBus, string name, KeyCode? keyBinding, MouseButton? mouseBinding, SDL.GamepadButton? gamepadButton, SDL.GamepadAxis? gamepadAxis, int controllerIndex)
    {
        this.manager = manager;
        
        Name = name;
        KeyBinding = keyBinding;
        MouseBinding = mouseBinding;
        GamepadButton = gamepadButton;
        GamepadAxis = gamepadAxis;
        ControllerIndex = controllerIndex;

        eventBus.BeginListen(Listener);

        eventBus.OnDispatch += _ =>
        {
            Pressed = false;
            Released = false;
            PollJoystick();
        };
    }

    public InputAction IgnoreWindow()
    {
        IgnoresWindow = true;
        return this;
    }
    
    private void Listener(InputEvent inputEvent, bool unhandled)
    {
        if(inputEvent is CursorEvent)
            return;
            
        if (inputEvent is KeyEvent keyEvent && keyEvent.Key == KeyBinding)
        {
            if (keyEvent.Action == ButtonAction.Press)
                Press();
            else if (keyEvent.Action == ButtonAction.Release)
                Release();
        }
        else if (inputEvent is MouseEvent mouseEvent && mouseEvent.Button == MouseBinding)
        {
            if (mouseEvent.Action == ButtonAction.Press)
                Press();
            else if (mouseEvent.Action == ButtonAction.Release)
                Release();
        }
    }

    private void PollJoystick()
    {
        // TODO: Joystick input

        Gamepad? pad = manager.GetGamepad(ControllerIndex);
        if(pad == null)
            return;

        if (GamepadButton != null)
        {
            if (pad.Button(GamepadButton.Value))
            {
                Press();
            }
            else
            {
                Release();
            }
        }

        if (GamepadAxis != null)
        {
            value = pad.Axis(GamepadAxis.Value);
        }
        

    }

    public KeyCode? KeyBinding;
    public MouseButton? MouseBinding;
    public SDL.GamepadButton? GamepadButton;
    public SDL.GamepadAxis? GamepadAxis;
    public int ControllerIndex;
    public string Name;
    public bool IgnoresWindow;

    private bool down;
    private bool pressed;
    private bool released;
    private float value;

    public float Value => down ? 1 : value;
    
    public bool Down
    {
        get => down && (IgnoresWindow || manager.MouseInWindow);
        private set => down = value;
    }
    
    public bool Pressed
    {
        get => pressed && (IgnoresWindow || manager.MouseInWindow);
        private set => pressed = value;
    }
    
    public bool Released
    {
        get => released && (IgnoresWindow || manager.MouseInWindow);
        private set => released = value;
    }
    
    
    public void Press()
    {
        Pressed = true;
        Down = true;
    }

    public void Release()
    {
        Released = true;
        Down = false;
    }
}