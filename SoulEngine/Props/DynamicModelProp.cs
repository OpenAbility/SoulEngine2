using ImGuiNET;
using ImGuizmoNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Mathematics;
using SoulEngine.Models;
using SoulEngine.Rendering;
using SoulEngine.Util;

namespace SoulEngine.Props;

[Prop("model_dynamic")]
[Serializable]
public class DynamicModelProp : Prop
{
    public readonly BoolProperty Visible;
    public readonly ResourceProperty<Model> ModelProperty;
    public readonly ResourceProperty<Model> JointModelProperty;

    private SkeletonJointData? selectedJoint;
    
    private SkeletonInstance? skeletonInstance;
    
    public DynamicModelProp(Scene scene, string type, string name) : base(scene, type, name)
    {
        Visible = Register(new BoolProperty("visible", true)); ;

        ModelProperty = Register(new ResourceProperty<Model>("model", "", scene.Game));
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
            
            mesh.Material.Bind(data, GlobalMatrix);
            mesh.Material.Shader.Uniform1i("ub_skeleton", 1);
            mesh.Material.Shader.BindBuffer("um_joint_buffer", skeletonBuffer, 0, skeletonInstance.Skeleton.JointCount);
            mesh.ActualMesh.Draw();
        }


        renderContext.PopPassName();
        
        
    }

    public override void RenderMoveGizmo(Matrix4 viewMatrix, Matrix4 projectionMatrix)
    {
        if (selectedJoint == null)
        {
            base.RenderMoveGizmo(viewMatrix, projectionMatrix);
            return;
        }
        
        float[] view = new float[16];
        viewMatrix.MatrixToArray(ref view);

        float[] projection = new float[16];
        projectionMatrix.MatrixToArray(ref projection);

        float[] model = new float[16];
        (skeletonInstance!.GetJointGlobalMatrix(selectedJoint) * GlobalMatrix).MatrixToArray(ref model);

        ImGuizmo.SetID(GetHashCode());
        if (ImGuizmo.Manipulate(ref view[0], ref projection[0],
                OPERATION.TRANSLATE | OPERATION.ROTATE | OPERATION.SCALE, MODE.LOCAL, ref model[0]))
        {
            Matrix4 newModel = EngineUtility.ArrayToMatrix(model);

            if (selectedJoint.Parent != null)
            {
                newModel *= skeletonInstance.GetJointGlobalMatrix(selectedJoint.Parent).Inverted();
            }

            newModel *= GlobalMatrix.Inverted();

            skeletonInstance.TranslateJoint(selectedJoint, newModel);
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
                mesh.Material.Bind(context.SceneRenderData, skeletonInstance.GetJointGlobalMatrix(skeletonInstance.Skeleton.GetJoint(i)) * GlobalMatrix);
                mesh.ActualMesh.Draw();
            }
        }
    }
}