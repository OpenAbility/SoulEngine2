using OpenTK.Mathematics;
using SoulEngine.Entities;
using SoulEngine.Renderer;
using SoulEngine.Rendering;
using SoulEngine.Resources;

namespace SoulEngine.Components.Primitives;

[Component("renderer_cube")]
public class CubeRendererComponent(Entity entity) : Component(entity)
{
    private Mesh? mesh;

    private bool edited;

    [SerializedProperty("offset")]
    public Vector3 Offset
    {
        get;
        set { field = value; edited = true; }
    }
    [SerializedProperty("size")] public Vector3 Size
    {
        get;
        set { field = value; edited = true; }
    }

    [SerializedProperty("material")] public Material? Material;

    public override void Render(IRenderPipeline renderPipeline, float deltaTime)
    {
        if (mesh == null || edited)
            mesh = Game.Primitives.GenerateCube(Size, Offset);
        
        if(Material == null)
            return;

        MeshRenderProperties meshRender = new MeshRenderProperties
        {
            Material = Material,
            Mesh = mesh,
            ModelMatrix = Entity.GlobalMatrix,
            PerformSkeletonDeformation = false
        };

        renderPipeline.SubmitMeshRender(DefaultRenderLayers.OpaqueLayer, meshRender);
    }
}