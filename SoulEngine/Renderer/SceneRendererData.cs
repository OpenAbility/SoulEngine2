using Microsoft.VisualBasic;
using OpenTK.Mathematics;
using SoulEngine.Rendering;
using SoulEngine.UI;

namespace SoulEngine.Renderer;

public struct SceneRendererData
{
    public IRenderPipeline RenderPipeline;

    public float DeltaTime;

    public CameraSettings CameraSettings;

    public UIContext? UIContext;

    public Vector2i FramebufferSize;
    public bool CullPass;
}