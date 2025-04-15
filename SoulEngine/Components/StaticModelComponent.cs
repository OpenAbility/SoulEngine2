using SoulEngine.Core;
using SoulEngine.Models;
using SoulEngine.Props;
using SoulEngine.Renderer;
using SoulEngine.Rendering;

namespace SoulEngine.Components;

[Component("model_static")]
[Serializable]
public class StaticModelComponent : Component
{
    public readonly BoolProperty Visible;
    public readonly ResourceProperty<Model> ModelProperty;

    
    public StaticModelComponent(Entity entity) : base(entity)
    {
        Visible = Register(new BoolProperty("visible", true));
        ModelProperty = Register(new ResourceProperty<Model>("model", "", Entity.Scene.Game));
    }
    
    public override void Render(IRenderPipeline renderPipeline, SceneRenderData data, float deltaTime)
    {
        if(!Visible.Value)
            return;
        
        if(ModelProperty.Value == null)
            return;

        MeshRenderProperties renderProperties = new MeshRenderProperties();
        renderProperties.ModelMatrix = Entity.GlobalMatrix;

        foreach (var mesh in ModelProperty.Value.Meshes)
        {
            renderProperties.Mesh = mesh.ActualMesh;
            renderProperties.Material = mesh.Material;
            renderPipeline.SubmitMeshRender(DefaultRenderLayers.OpaqueLayer, renderProperties);
        }
    }
}