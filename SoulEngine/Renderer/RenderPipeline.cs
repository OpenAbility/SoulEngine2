using SoulEngine.Rendering;

namespace SoulEngine.Renderer;

/// <summary>
/// Handles the full rendering from scene input to output
/// </summary>
public interface IRenderPipeline
{
    public void SubmitMeshRender(RenderLayer renderLayer, MeshRenderProperties renderProperties);
    public IEnumerable<RenderLayer> GetLayers();
    
    public void DrawFrame(RenderContext renderContext, IRenderSurface targetSurface, float deltaTime, CameraSettings cameraSettings);
}