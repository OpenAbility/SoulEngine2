using Hexa.NET.ImGui;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Animation;
using SoulEngine.Core;
using SoulEngine.Models;
using SoulEngine.Props;
using SoulEngine.Rendering;

namespace SoulEngine.Components;

[Component("model_dynamic")]
[Serializable]
public class DynamicModelComponent : Component
{
    public DynamicModelComponent(Entity entity) : base(entity)
    {
        Visible = Register(new BoolProperty("visible", true)); ;

        ModelProperty = Register(new ResourceProperty<Model>("model", "", Game));
        JointModelProperty = Register(new ResourceProperty<Model>("jointModel", "mod/joint.mdl", Game));
        AnimationProperty = Register(new ResourceProperty<AnimationClip>("animation", "", Game));
    }
    
     public readonly BoolProperty Visible;
    public readonly ResourceProperty<Model> ModelProperty;
    public readonly ResourceProperty<Model> JointModelProperty;
    public readonly ResourceProperty<AnimationClip> AnimationProperty;

    private SkeletonJointData? selectedJoint;
    
    private SkeletonInstance? skeletonInstance;
    public SingleAnimationPlayer? AnimationPlayer;
    

    public override void Update(float deltaTime)
    {
        if (skeletonInstance?.Skeleton != ModelProperty.Value?.Skeleton)
        {
            // The model uses a different skeleton.
            if (ModelProperty.Value?.Skeleton != null)
            {
                skeletonInstance = ModelProperty.Value.Skeleton.Instantiate();
                AnimationPlayer = new SingleAnimationPlayer(skeletonInstance);
            }
        }

        if (AnimationPlayer?.CurrentClip != AnimationProperty.Value)
        {
            if (AnimationPlayer != null)
            {
                AnimationPlayer.CurrentClip = AnimationProperty.Value;
                AnimationPlayer.Playing?.Restart();
            }
        }

        AnimationPlayer?.Apply();

        if(AnimationPlayer?.Playing is { Playing: false })
            AnimationPlayer.Playing.Restart();
        
    }

    private static GpuBuffer<Matrix4>? skeletonBuffer;

    protected override void OnEdit()
    {
        if(skeletonInstance == null)
            return;

        if (ImGui.Button("Reset Selection"))
        {
            selectedJoint = null;
        }

        for (int i = 0; i < skeletonInstance.Skeleton.JointCount; i++)
        {
            var joint = skeletonInstance.Skeleton.GetJoint(i);
            if (ImGui.CollapsingHeader(joint.Name + " - " + joint.SkeletonID))
            {
                ImGui.PushID(joint.Name);
                if (ImGui.Button("Select"))
                {
                    selectedJoint = joint;
                }

                if (ImGui.Button("Reset"))
                {
                    skeletonInstance.TranslateJoint(joint, joint.DefaultMatrix);
                }
                
                ImGui.PopID();

            }
        }

        /*
        ImGui.BeginDisabled(animationContext == null);
        
        if(ImGui.Button("Play"))
            animationContext!.Play();
        ImGui.SameLine();
        if(ImGui.Button("Pause"))
            animationContext!.Pause();
        ImGui.SameLine();
        if(ImGui.Button("Stop"))
            animationContext!.Stop();
        
        if(ImGui.Button("Restart"))
            animationContext!.Restart();
        ImGui.SameLine();
        ImGui.Text(animationContext?.Elapsed.ToString() ?? "NO ANIM");
        
        ImGui.EndDisabled();
        */
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

        if ((skeletonBuffer?.Length ?? 0) < skeletonInstance.Skeleton.JointCount)
        {
            skeletonBuffer?.Dispose();
            skeletonBuffer = new GpuBuffer<Matrix4>((int)(skeletonInstance.Skeleton.JointCount * 1.5f),
                BufferStorageMask.MapWriteBit | BufferStorageMask.DynamicStorageBit | BufferStorageMask.MapCoherentBit | BufferStorageMask.MapPersistentBit | BufferStorageMask.ClientStorageBit);
        }

        BufferMapping<Matrix4> mapping = skeletonBuffer!.Map(0, skeletonInstance.Skeleton.JointCount,
            MapBufferAccessMask.MapCoherentBit | MapBufferAccessMask.MapPersistentBit | MapBufferAccessMask.MapWriteBit |
            MapBufferAccessMask.MapInvalidateRangeBit);


        for (int i = 0; i < skeletonInstance.Skeleton.JointCount; i++)
        {
            SkeletonJointData jointData = skeletonInstance.Skeleton.GetJoint(i);
            mapping.Span[ModelProperty.Value.skeletonToMeshJoints[jointData.SkeletonID]] = jointData.InverseBind * skeletonInstance.GetJointGlobalMatrix(jointData);
        }
        
        mapping.Dispose();

        foreach (var mesh in ModelProperty.Value.Meshes)
        {
            
            mesh.Material.Bind(data, Entity.GlobalMatrix);
            mesh.Material.Shader.Uniform1i("ub_skeleton", 1);
            mesh.Material.Shader.BindBuffer("um_joint_buffer", skeletonBuffer, 0, skeletonInstance.Skeleton.JointCount);
            mesh.ActualMesh.Draw();
        }
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
                Matrix4 matrix4 = skeletonInstance.GetJointGlobalMatrix(skeletonInstance.Skeleton.GetJoint(i)) *
                                  Entity.GlobalMatrix;
                mesh.Material.Bind(context.SceneRenderData, matrix4);
                mesh.ActualMesh.Draw();
            }
        }
    }
}