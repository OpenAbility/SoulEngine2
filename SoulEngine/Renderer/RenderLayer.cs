using SoulEngine.Core;

namespace SoulEngine.Renderer;

public class RenderLayer : EngineObject
{
    public bool DualPass;
    public bool ShadowReceiver;
    public bool ShadowCasting;
    public bool Skeletal;
}