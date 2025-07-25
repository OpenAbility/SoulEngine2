using System.Numerics;
using SoulEngine.Input;

namespace SoulEngine.Events;

public class InputEvent : HandledEvent
{
    
}

public class CursorEvent : InputEvent
{

    public readonly Vector2 Position;

    public CursorEvent(Vector2 position)
    {
        Position = position;
    }
}

public class MouseEvent : InputEvent
{

    public readonly KeyModifiers Modifier;
    public readonly MouseButton Button;
    public readonly ButtonAction Action;

    public MouseEvent(KeyModifiers modifier, MouseButton button, ButtonAction action)
    {
        Modifier = modifier;
        Button = button;
        Action = action;
    }
}

public class KeyEvent : InputEvent
{

    public readonly KeyModifiers Modifier;
    public readonly KeyCode Key;
    public readonly ButtonAction Action;

    public KeyEvent(KeyModifiers modifier, KeyCode key, ButtonAction action)
    {
        Modifier = modifier;
        Key = key;
        Action = action;
    }
}

public class TypeEvent : InputEvent
{

    public readonly string Text;

    public TypeEvent(string text)
    {
        Text = text;
    }
}

public class ScrollEvent : InputEvent
{

    public readonly Vector2 Delta;

    public ScrollEvent(Vector2 delta)
    {
        Delta = delta;
    }
}