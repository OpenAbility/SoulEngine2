using SoulEngine.Models;
using SoulEngine.Renderer;
using SoulEngine.Rendering;
using Scene = SoulEngine.Core.Scene;

namespace SoulEngine.Props;

[Prop("model_static")]
[Serializable]
public class StaticModelProp : Prop
{
    public readonly BoolProperty Visible;
    public readonly ResourceProperty<Model> ModelProperty;
    
    public StaticModelProp(Scene scene, string type, string name) : base(scene, type, name)
    {
        Visible = Register(new BoolProperty("visible", true));
        ModelProperty = Register(new ResourceProperty<Model>("model", "", scene.Game));
    }
    
    public override void Render(IRenderPipeline renderPipeline, SceneRenderData renderData, float deltaTime)
    {
        if(!Visible.Value)
            return;
        
        if(ModelProperty.Value == null)
            return;

        MeshRenderProperties renderProperties = new MeshRenderProperties();
        renderProperties.ModelMatrix = GlobalMatrix;

        foreach (var mesh in ModelProperty.Value.Meshes)
        {
            renderProperties.Mesh = mesh.ActualMesh;
            renderProperties.Material = mesh.Material;
            renderPipeline.SubmitMeshRender(DefaultRenderLayers.OpaqueLayer, renderProperties);
        }
    }
}