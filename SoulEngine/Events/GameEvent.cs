namespace SoulEngine.Events;

public class GameEvent
{
    public const string Update = "update";
    public const string Finalizing = "finalizing";
    
    
    public readonly string EventType;

    public GameEvent(string eventType)
    {
        EventType = eventType;
    }
}
