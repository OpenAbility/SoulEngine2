using OpenTK.Graphics.OpenGL;
using SoulEngine.Core;
using SoulEngine.Models;
using SoulEngine.Rendering;

namespace SoulEngine.Props;

[Prop("model_dynamic")]
[Serializable]
public class DynamicModelProp : Prop
{
    public readonly BoolProperty Visible;
    public readonly ResourceProperty<Model> ModelProperty;
    public readonly ResourceProperty<Model> JointModelProperty;
    
    private SkeletonInstance? skeletonInstance;
    
    public DynamicModelProp(Scene scene, string type, string name) : base(scene, type, name)
    {
        Visible = Register(new BoolProperty("visible", true)); ;

        ModelProperty = Register(new ResourceProperty<Model>("model", "mod/test.mdl", scene.Game));
        JointModelProperty = Register(new ResourceProperty<Model>("jointModel", "mod/joint.mdl", scene.Game));
    }

    public override void Update(float deltaTime)
    {
        if (skeletonInstance?.Skeleton != ModelProperty.Value?.Skeleton)
        {
            // The model uses a different skeleton.
            if (ModelProperty.Value?.Skeleton != null)
            {
                skeletonInstance = ModelProperty.Value.Skeleton.Instantiate();
            }
        }
    }
    


    public override void Render(RenderContext renderContext, SceneRenderData data, float deltaTime)
    {
        if(!Visible.Value)
            return;
        
        if(ModelProperty.Value == null)
            return;
        
        if(JointModelProperty.Value == null)
            return;
        
        if(skeletonInstance == null)
            return;
        
        renderContext.PushPassName(Name);


        foreach (var mesh in ModelProperty.Value.Meshes)
        {
            mesh.Material.Bind(data, GlobalMatrix);
            mesh.ActualMesh.Draw();
        }


        renderContext.PopPassName();
        
        
    }

    public override void RenderGizmo(GizmoContext context)
    {
        if(!Visible.Value)
            return;
        
        if(ModelProperty.Value == null)
            return;
        
        if(JointModelProperty.Value == null)
            return;
        
        if(skeletonInstance == null)
            return;
        
        for (int i = 0; i < skeletonInstance.Skeleton.JointCount; i++)
        {
            foreach (var mesh in JointModelProperty.Value.Meshes)
            {
                mesh.Material.Bind(context.SceneRenderData, GlobalMatrix * skeletonInstance.GetJointGlobalMatrix(skeletonInstance.Skeleton.GetJoint(i)));
                mesh.ActualMesh.Draw();
            }
        }
    }
}