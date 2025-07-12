using System.Numerics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SDL3;
using SoulEngine.Core;
using SoulEngine.Events;

namespace SoulEngine.Input;

public class InputManager : EngineObject
{

    private readonly List<InputAction> actions = new List<InputAction>();
    private readonly Dictionary<int, Gamepad> gamepads = new Dictionary<int, Gamepad>();
    private readonly EventBus<InputEvent> eventBus;
    private readonly Game game;

    public Vector2 MouseDelta => RawMousePosition - (previousMousePosition ?? RawMousePosition);
    public Vector2 MousePosition => RawMousePosition - WindowOffset;
    
    public Vector2 WindowOffset { get; internal set; }
    public Vector2 WindowSize { get; internal set; }
    public Vector2 RawMousePosition { get; private set; }

    public bool MouseInWindow =>
        ((RawMousePosition.X >= WindowOffset.X && RawMousePosition.X < WindowOffset.X + WindowSize.X &&
        RawMousePosition.Y >= WindowOffset.Y && RawMousePosition.Y < WindowOffset.Y + WindowSize.Y) || (game.MainWindow.MouseCaptured && game.Visible)) && game.Visible;

    private Vector2? previousMousePosition;

    public IEnumerable<InputAction> Actions => actions;
    
    public InputManager(Game game, EventBus<InputEvent> eventBus)
    {
        this.game = game;
        eventBus.OnDispatch += _ => BeforeDispatch();
        eventBus.BeginListen(Listener);
        this.eventBus = eventBus;
    }

    public Gamepad? GetGamepad(int index)
    {
        return gamepads.GetValueOrDefault(index);
    }

    public void RegisterGamepad(int index, Gamepad gamepad)
    {
        gamepads[index] = gamepad;
    }

    public void DeleteGamepad(int index)
    {
        gamepads.Remove(index);
    }

    private void Listener(InputEvent inputEvent, bool unhandled)
    {
        if (inputEvent is CursorEvent cursorEvent)
        {
            RawMousePosition = cursorEvent.Position;
            previousMousePosition ??= RawMousePosition;
        }
    }
    public ActionBuilder Action() => new ActionBuilder(this);
    public ActionBuilder Action(string name) => new ActionBuilder(this).Name(name);    
    private void BeforeDispatch()
    {
        previousMousePosition = RawMousePosition;
        
        
    }


    public class ActionBuilder
    {
        private bool finished = false;
        private readonly InputManager manager;
        private string name = "";
        private Keys? key;
        private MouseButton? mouseButton;
        private SDL.GamepadButton? gamepadButton;
        private SDL.GamepadAxis? gamepadAxis;
        private int controllerIndex = 0;
        
        internal ActionBuilder(InputManager inputManager)
        {
            this.manager = inputManager;
        }

        public ActionBuilder Name(string name)
        {
            this.name = name;
            return this;
        }
        
        public ActionBuilder Bind(Keys? key)
        {
            this.key = key;
            return this;
        }
        
        public ActionBuilder Bind(MouseButton? button)
        {
            this.mouseButton = button;
            return this;
        }
        
        public ActionBuilder Bind(SDL.GamepadButton? button)
        {
            this.gamepadButton = button;
            return this;
        }
        
        public ActionBuilder Bind(SDL.GamepadAxis? axis)
        {
            this.gamepadAxis = axis;
            return this;
        }
        
        public ActionBuilder Controller(int controller)
        {
            this.controllerIndex = controller;
            return this;
        }

        public InputAction Finish()
        {
            if (finished)
                throw new Exception("Builder is already done!");
            finished = true;

            InputAction inputAction = new InputAction(manager, manager.eventBus, name, key, mouseButton, gamepadButton, gamepadAxis, controllerIndex);
            
            manager.actions.Add(inputAction);

            return inputAction;
        }
    }
}