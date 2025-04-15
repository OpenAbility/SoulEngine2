using SoulEngine.Rendering;
using SoulEngine.UI.Rendering;

namespace SoulEngine.Renderer;

/// <summary>
/// Handles the full rendering from scene input to output
/// </summary>
public interface IRenderPipeline
{
    public void SubmitMeshRender(RenderLayer renderLayer, MeshRenderProperties renderProperties);
    public void SubmitDrawList(RenderLayer renderLayer, DrawListData drawListData);
    public IEnumerable<RenderLayer> GetLayers();
    
    public void DrawFrame(PipelineData pipelineData);
}