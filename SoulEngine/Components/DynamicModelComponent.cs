using System.Buffers;
using Hexa.NET.ImGui;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Animation;
using SoulEngine.Core;
using SoulEngine.Entities;
using SoulEngine.Mathematics;
using SoulEngine.Models;
using SoulEngine.Props;
using SoulEngine.Renderer;
using SoulEngine.Rendering;
using SoulEngine.Resources;

namespace SoulEngine.Components;

[Component("model_dynamic", Icon = "skeleton")]
[Serializable]
public class DynamicModelComponent : Component
{
    [SerializedProperty("visible")] public bool Visible = true;

    [SerializedProperty("model")]
    public Model? Model
    {
        get => field;
        set
        {
            if(field == value)
                return;
            
            if(deformationCache != null)
                deformationCache.Dispose();
            deformationCache = null;
            
            field = value;
            FlushContextObjects();
        }
    }
    public AnimationPlayer? AnimationPlayer { get; private set; }

    public SkeletonInstance? SkeletonInstance;

    private DeformationCache? deformationCache;
    
    public DynamicModelComponent(Entity entity) : base(entity)
    {

    }
    
    private void FlushContextObjects()
    {
        if (SkeletonInstance?.Skeleton != Model?.Skeleton)
        {
            // The model uses a different skeleton.
            if (Model?.Skeleton != null)
            {
                SkeletonInstance = Model.Skeleton.Instantiate();
                AnimationPlayer = new AnimationPlayer(SkeletonInstance);
            }
        }
        
        
    }
    
    public override void Update(float deltaTime)
    {
        AnimationPlayer?.Apply();
    }

    public override void Render(IRenderPipeline renderPipeline, float deltaTime)
    {
        if(!Visible)
            return;
        
        if(Model == null)
            return;
        
        if(SkeletonInstance == null)
            return;
        
        if(deformationCache == null)
            deformationCache = Model.GenerateDeformationCache();

        Matrix4[] skeletonBuffer = ArrayPool<Matrix4>.Shared.Rent(SkeletonInstance.Skeleton.JointCount);

        for (int i = 0; i < SkeletonInstance.Skeleton.JointCount; i++)
        {
            SkeletonJointData jointData = SkeletonInstance.Skeleton.GetJoint(i);

            if (Model.SkeletonToMeshJoints.TryGetValue(jointData.SkeletonID, out var bufferIndex))
            {
                skeletonBuffer[bufferIndex] = jointData.InverseBind * SkeletonInstance.GetJointGlobalMatrix(jointData);
            }
        }
        
        
        MeshRenderProperties renderProperties = new MeshRenderProperties();
        renderProperties.ModelMatrix = Entity.GlobalMatrix;
        renderProperties.SkeletonBuffer = skeletonBuffer;
        renderProperties.SkeletonBufferPool = ArrayPool<Matrix4>.Shared;
        renderProperties.SkeletonBufferSize = SkeletonInstance.Skeleton.JointCount;
        renderProperties.PerformSkeletonDeformation = true;
   
        foreach (var mesh in Model.Meshes)
        {
            renderProperties.Mesh = mesh.ActualMesh;
            renderProperties.Material = mesh.Material;
            
            renderProperties.DeformationCache = deformationCache!.GetBuffer(mesh.Index);
            
            renderPipeline.SubmitMeshRender(DefaultRenderLayers.OpaqueLayer, renderProperties);
        }
    }
    
    public override AABB RenderingBoundingBox()
    {
        // TODO: Joint AABB
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
        
        if(SkeletonInstance == null)
            return;

        Colour colour = Colour.White;

        if (!Scene.Camera?.CurrentFrustum.InFrustum(Entity.Position) ?? true)
            colour = Colour.Red;
        
                
        context.ModelMatrix = Matrix4.Identity;
        Model.BoundingBox.Translated(Entity.GlobalMatrix).Draw(context, colour);
        
        for (int i = 0; i < SkeletonInstance.Skeleton.JointCount; i++)
        {
            SkeletonJointData joint = SkeletonInstance.Skeleton.GetJoint(i);
            
            Matrix4 matrix4 = SkeletonInstance.GetJointGlobalMatrix(joint) * Entity.GlobalMatrix;

            context.ModelMatrix = matrix4;
            context.BillboardedSprite(Scene.Game.ResourceManager.Load<Texture>("icons/bone.png"), 0.2f);
        }
    }
}