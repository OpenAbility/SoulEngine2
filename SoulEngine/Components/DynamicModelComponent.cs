using System.Buffers;
using Hexa.NET.ImGui;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Animation;
using SoulEngine.Core;
using SoulEngine.Entities;
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
            field = value;
            FlushContextObjects();
        }
    }
    public AnimationPlayer? AnimationPlayer { get; private set; }

    public SkeletonInstance? SkeletonInstance;
    
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

    private static GpuBuffer<Matrix4>? skeletonBuffer;
    

    public override void Render(IRenderPipeline renderPipeline, float deltaTime)
    {
        if(!Visible)
            return;
        
        if(Model == null)
            return;
        
        if(SkeletonInstance == null)
            return;

        Matrix4[] skeletonBuffer = ArrayPool<Matrix4>.Shared.Rent(SkeletonInstance.Skeleton.JointCount);

        for (int i = 0; i < SkeletonInstance.Skeleton.JointCount; i++)
        {
            SkeletonJointData jointData = SkeletonInstance.Skeleton.GetJoint(i);
            skeletonBuffer[Model.skeletonToMeshJoints[jointData.SkeletonID]] = jointData.InverseBind * SkeletonInstance.GetJointGlobalMatrix(jointData);
        }
        
        
        MeshRenderProperties renderProperties = new MeshRenderProperties();
        renderProperties.ModelMatrix = Entity.GlobalMatrix;
        renderProperties.SkeletonBuffer = skeletonBuffer;
        renderProperties.SkeletonBufferPool = ArrayPool<Matrix4>.Shared;
        renderProperties.SkeletonBufferSize = SkeletonInstance.Skeleton.JointCount;

        foreach (var mesh in Model.Meshes)
        {
            renderProperties.Mesh = mesh.ActualMesh;
            renderProperties.Material = mesh.Material;
            renderPipeline.SubmitMeshRender(DefaultRenderLayers.OpaqueLayer, renderProperties);
        }
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
        
        for (int i = 0; i < SkeletonInstance.Skeleton.JointCount; i++)
        {
            Matrix4 matrix4 = SkeletonInstance.GetJointGlobalMatrix(SkeletonInstance.Skeleton.GetJoint(i)) *
                              Entity.GlobalMatrix;

            context.ModelMatrix = matrix4;
            context.BillboardedSprite(Scene.Game.ResourceManager.Load<Texture>("icons/bone.png"), 0.2f);
        }
    }
}