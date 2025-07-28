using SoulEngine.Core;
using SoulEngine.Rendering;
using SoulEngine.UI;

namespace SoulEngine.Renderer;

public struct SceneRenderInformation()
{
    public IEntityCollection EntityCollection = null!;
    public IRenderSurface TargetSurface = null!;
    public float DeltaTime;
    public UIContext? UIContext;
    public IRenderPipeline RenderPipeline = null!;
    public RenderContext RenderContext = null!;

    public CameraSettings CameraSettings;

    public bool PerformCullingPass;
    public bool EnableCulling;

    public bool PostProcessing;

    public Action<UIContext> RenderUI = (context) => { };
}