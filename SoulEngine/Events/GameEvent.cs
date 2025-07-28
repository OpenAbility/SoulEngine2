namespace SoulEngine.Events;

public class GameEvent
{
    public const string Update = "update";
    public const string Finalizing = "finalizing";
    public const string RendererDebugCallback = "renderer_debug_callback";
    
    
    public readonly string EventType;

    public GameEvent(string eventType)
    {
        EventType = eventType;
    }
}
