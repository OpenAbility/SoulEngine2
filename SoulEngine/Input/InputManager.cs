using System.Numerics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SoulEngine.Core;
using SoulEngine.Events;

namespace SoulEngine.Input;

public class InputManager
{

    private readonly List<InputAction> actions = new List<InputAction>();
    private readonly EventBus<InputEvent> eventBus;
    private readonly Game game;

    public Vector2 MouseDelta => RawMousePosition - (previousMousePosition ?? RawMousePosition);
    public Vector2 MousePosition => RawMousePosition - WindowOffset;
    
    public Vector2 WindowOffset { get; internal set; }
    public Vector2 WindowSize { get; internal set; }
    public Vector2 RawMousePosition { get; private set; }

    public bool MouseInWindow =>
        ((RawMousePosition.X >= WindowOffset.X && RawMousePosition.X < WindowOffset.X + WindowSize.X &&
        RawMousePosition.Y >= WindowOffset.Y && RawMousePosition.Y < WindowOffset.Y + WindowSize.Y) || game.MainWindow.MouseCaptured) && game.Visible;

    private Vector2? previousMousePosition;

    public IEnumerable<InputAction> Actions => actions;
    
    public InputManager(Game game, EventBus<InputEvent> eventBus)
    {
        this.game = game;
        eventBus.OnDispatch += _ => BeforeDispatch();
        eventBus.BeginListen(Listener);
        this.eventBus = eventBus;
    }

    private void Listener(InputEvent inputEvent, bool unhandled)
    {
        if (inputEvent is CursorEvent cursorEvent)
        {
            RawMousePosition = cursorEvent.Position;
            previousMousePosition ??= RawMousePosition;
        }
    }

    public InputAction Action(string name, Keys? keyBinding, MouseButton? mouseBinding, JoystickHats? joystickButton,
        int controllerIndex)
    {
        InputAction action =
            new InputAction(this, eventBus, name, keyBinding, mouseBinding, joystickButton, controllerIndex);
        actions.Add(action);
        return action;
    }

    public InputAction Action(string name, Keys keyBinding) => Action(name, keyBinding, null, null, 0);
    public InputAction Action(string name, Keys keyBinding, JoystickHats joystickButton) => Action(name, keyBinding, null, joystickButton, 0);
    public InputAction Action(string name, Keys keyBinding, JoystickHats joystickButton, int controllerIndex) => Action(name, keyBinding, null, joystickButton, controllerIndex);
    
    public InputAction Action(string name, MouseButton mouseButton) => Action(name, null, mouseButton, null, 0);
    public InputAction Action(string name, MouseButton mouseButton, JoystickHats joystickButton) => Action(name, null, mouseButton, joystickButton, 0);
    public InputAction Action(string name, MouseButton mouseButton, JoystickHats joystickButton, int controllerIndex) => Action(name, null, mouseButton, joystickButton, controllerIndex);

    private void BeforeDispatch()
    {
        previousMousePosition = RawMousePosition;
    }
}