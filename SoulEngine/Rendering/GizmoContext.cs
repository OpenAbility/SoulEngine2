using OpenTK.Mathematics;

namespace SoulEngine.Rendering;

public class GizmoContext
{
    public bool Selected { get; internal set; }

    public Matrix4 ModelMatrix { get; internal set; }
    public Matrix4 ViewMatrix { get; internal set; }
    public Matrix4 ProjectionMatrix { get; internal set; }

    public readonly RenderContext RenderContext;

    public SceneRenderData SceneRenderData { get; internal set; }

    private List<Vertex> vertices = new List<Vertex>();

    public GizmoContext(RenderContext renderContext)
    {
        RenderContext = renderContext;
    }

    public void Begin()
    {
        
    }

    public void End()
    {
        
    }
}