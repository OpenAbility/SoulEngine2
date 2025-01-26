using SoulEngine.Models;
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
        ModelProperty = Register(new ResourceProperty<Model>("model", "mod/test.mdl", scene.Game));
    }

    public override void Render(RenderContext renderContext, SceneRenderData data, float deltaTime)
    {
        if(!Visible.Value)
            return;
        
        if(ModelProperty.Value == null)
            return;
        
        renderContext.PushPassName(Name);
        
        foreach (var mesh in ModelProperty.Value.Meshes)
        {
            mesh.Material.Bind(data, GlobalMatrix);
            mesh.ActualMesh.Draw();
        }
        
        renderContext.PopPassName();
    }
}