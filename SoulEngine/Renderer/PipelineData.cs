using SoulEngine.Core;
using SoulEngine.Rendering;
using SoulEngine.UI;

namespace SoulEngine.Renderer;

public struct PipelineData()
{
    public Game Game;
    
    public RenderContext RenderContext;
    public IRenderSurface TargetSurface;
    public float DeltaTime;
    public CameraSettings CameraSettings;

    public Action DrawGizmos = () => {};

    public UIContext? UIContext;

    public bool PostProcessing = true;
}