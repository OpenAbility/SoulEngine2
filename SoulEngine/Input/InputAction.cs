using OpenTK.Windowing.GraphicsLibraryFramework;
using SoulEngine.Events;

namespace SoulEngine.Input;

public class InputAction
{
    private readonly EventListener<InputEvent> listener;
    private readonly InputManager manager;
    
    public InputAction(InputManager manager, EventBus<InputEvent> eventBus, string name, Keys? keyBinding, MouseButton? mouseBinding, JoystickHats? joystickButton, int controllerIndex)
    {
        this.manager = manager;
        
        Name = name;
        KeyBinding = keyBinding;
        MouseBinding = mouseBinding;
        JoystickButton = joystickButton;
        ControllerIndex = controllerIndex;

        eventBus.BeginListen(Listener);

        eventBus.OnDispatch += _ =>
        {
            Pressed = false;
            Released = false;
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
            if (keyEvent.Action == OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Press)
                Press();
            else if (keyEvent.Action == OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Release)
                Release();
        }
        else if (inputEvent is MouseEvent mouseEvent && mouseEvent.Button == MouseBinding)
        {
            if (mouseEvent.Action == OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Press)
                Press();
            else if (mouseEvent.Action == OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Release)
                Release();
        }
            
        // TODO: Controller inputs
    }

    public Keys? KeyBinding;
    public MouseButton? MouseBinding;
    public JoystickHats? JoystickButton;
    public int ControllerIndex;
    public string Name;
    public bool IgnoresWindow;

    private bool down;
    private bool pressed;
    private bool released;
    
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