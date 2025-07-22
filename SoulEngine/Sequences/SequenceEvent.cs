namespace SoulEngine.Sequences;

public abstract class SequenceEvent : IComparable<SequenceEvent>
{
    public TimeSpan StartTime;
    
    public TimeSpan Duration = TimeSpan.Zero;
    
    public string DisplayName;
    public Colour Colour = Colour.Wheat;

    public EventRenderMode RenderMode = EventRenderMode.Circle;

    public int TimelineID;

    protected SequenceEvent()
    {
        StartTime = TimeSpan.Zero;
        DisplayName = GetType().Name;
    }

    public int CompareTo(SequenceEvent? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return StartTime.CompareTo(other.StartTime);
    }
}

public enum EventRenderMode
{
    Diamond,
    Circle
}

public class TestEvent : SequenceEvent
{
    
}