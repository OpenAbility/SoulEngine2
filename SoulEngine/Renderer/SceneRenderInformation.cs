using SoulEngine.Core;
using SoulEngine.Rendering;
using SoulEngine.UI;

namespace SoulEngine.Renderer;

public struct SceneRenderInformation()
{
    public IEntityCollection EntityCollection;
    public IRenderSurface TargetSurface;
    public float DeltaTime;
    public UIContext? UIContext;
    public IRenderPipeline RenderPipeline;
    public RenderContext RenderContext;

    public CameraSettings CameraSettings;

    public bool PerformCullingPass;
    public bool EnableCulling;

    public bool PostProcessing;

    public Action<UIContext> RenderUI = (context) => { };
}