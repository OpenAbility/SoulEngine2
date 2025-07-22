using SoulEngine.Core;

namespace SoulEngine.Renderer;

public class RenderLayer : EngineObject
{
    public bool DualPass = true;
    public bool ShadowReceiver = true;
    public bool ShadowCasting = true;
    public bool Skeletal = true;
}