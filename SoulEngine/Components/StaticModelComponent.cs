using SoulEngine.Core;
using SoulEngine.Models;
using SoulEngine.Props;
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
    
    public override void Render(RenderContext renderContext, SceneRenderData data, float deltaTime)
    {
        if(!Visible.Value)
            return;
        
        if(ModelProperty.Value == null)
            return;
        
        renderContext.PushPassName(Entity.Name);
        
        foreach (var mesh in ModelProperty.Value.Meshes)
        {
            mesh.Material.Bind(data, Entity.GlobalMatrix);
            mesh.Material.Shader.Uniform1i("ub_skeleton", 0);
            mesh.ActualMesh.Draw();
        }
        
        renderContext.PopPassName();
    }
}