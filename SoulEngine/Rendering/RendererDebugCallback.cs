using SoulEngine.Events;

namespace SoulEngine.Rendering;

public class RendererDebugCallback : GameEvent
{
    public readonly string Message;
    
    public RendererDebugCallback(string message) : base(RendererDebugCallback)
    {
        Message = message;
    }
}