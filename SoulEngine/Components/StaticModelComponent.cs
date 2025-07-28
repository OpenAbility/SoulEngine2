using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Entities;
using SoulEngine.Mathematics;
using SoulEngine.Models;
using SoulEngine.Props;
using SoulEngine.Renderer;
using SoulEngine.Rendering;

namespace SoulEngine.Components;

[Component("model_static")]
[Serializable]
public class StaticModelComponent : Component
{
    [SerializedProperty("visible")] public bool Visible = true;
    [SerializedProperty("model")] public Model? Model;

    
    public StaticModelComponent(Entity entity) : base(entity)
    {
    }
    
    public override void Render(IRenderPipeline renderPipeline, float deltaTime)
    {
        if(!Visible)
            return;
        
        if(Model == null)
            return;

        MeshRenderProperties renderProperties = new MeshRenderProperties();
        renderProperties.ModelMatrix = Entity.GlobalMatrix;

        foreach (var mesh in Model.Meshes)
        {
            renderProperties.Mesh = mesh.ActualMesh;
            renderProperties.Material = mesh.Material;
            renderPipeline.SubmitMeshRender(DefaultRenderLayers.OpaqueLayer, renderProperties);
        }
    }

    public override AABB RenderingBoundingBox()
    {
        if (Model == null)
            return new AABB();

        return Model.BoundingBox;
    }

    public override void RenderGizmo(GizmoContext context)
    {
        base.RenderGizmo(context);
        
        if(!Visible)
            return;
        
        if(Model == null)
            return;
        
        if(Model == null)
            return;

        context.ModelMatrix = Matrix4.Identity;
        Model.BoundingBox.Translated(Entity.GlobalMatrix).Draw(context, Colour.White);
    }
    
}