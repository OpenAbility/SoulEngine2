using SoulEngine.Core;
using SoulEngine.Rendering;
using SoulEngine.UI;

namespace SoulEngine.Renderer;

public struct PipelineData()
{
    public Game Game = null!;
    
    public RenderContext RenderContext = null!;
    public IRenderSurface TargetSurface = null!;
    public float DeltaTime;
    public CameraSettings CameraSettings;

    public ShadowCameraSettings ShadowCameraSettings;

    public Colour AmbientLight;
    
    public bool EnableShadows = true;
    public bool RedrawShadows = true;

    public Action DrawGizmos = () => {};

    public UIContext? UIContext;

    public bool PostProcessing = true;
}