using SoulEngine.Rendering;
using SoulEngine.UI;

namespace SoulEngine.Renderer;

public struct PipelineData
{
    public RenderContext RenderContext;
    public IRenderSurface TargetSurface;
    public float DeltaTime;
    public CameraSettings CameraSettings;

    public UIContext? UIContext;
}