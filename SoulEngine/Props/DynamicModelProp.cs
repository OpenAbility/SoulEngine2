using System.Buffers;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Animation;
using SoulEngine.Core;
using SoulEngine.Mathematics;
using SoulEngine.Models;
using SoulEngine.Renderer;
using SoulEngine.Rendering;
using SoulEngine.Util;

namespace SoulEngine.Props;

[Prop("model_dynamic", Icon = "skeleton")]
[Serializable]
public class DynamicModelProp : Prop
{
    public readonly BoolProperty Visible;
    public readonly ResourceProperty<Model> ModelProperty;
    public readonly ResourceProperty<Model> JointModelProperty;
    public readonly ResourceProperty<AnimationClip> AnimationProperty;

    private SkeletonJointData? selectedJoint;
    
    private SkeletonInstance? skeletonInstance;
    public SingleAnimationPlayer? AnimationPlayer;
    
    public DynamicModelProp(Scene scene, string type, string name) : base(scene, type, name)
    {
        Visible = Register(new BoolProperty("visible", true)); ;

        ModelProperty = Register(new ResourceProperty<Model>("model", "", scene.Game));
        JointModelProperty = Register(new ResourceProperty<Model>("jointModel", "mod/joint.mdl", scene.Game));
        AnimationProperty = Register(new ResourceProperty<AnimationClip>("animation", "", scene.Game));
    }

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

    
    public override void Render(IRenderPipeline renderPipeline, SceneRenderData renderData, float deltaTime)
    {
        if(!Visible.Value)
            return;
        
        if(ModelProperty.Value == null)
            return;
        
        if(JointModelProperty.Value == null)
            return;
        
        if(skeletonInstance == null)
            return;

        Matrix4[] skeletonBuffer = ArrayPool<Matrix4>.Shared.Rent(skeletonInstance.Skeleton.JointCount);

        for (int i = 0; i < skeletonInstance.Skeleton.JointCount; i++)
        {
            SkeletonJointData jointData = skeletonInstance.Skeleton.GetJoint(i);
            skeletonBuffer[ModelProperty.Value.skeletonToMeshJoints[jointData.SkeletonID]] = jointData.InverseBind * skeletonInstance.GetJointGlobalMatrix(jointData);
        }
        
        
        MeshRenderProperties renderProperties = new MeshRenderProperties();
        renderProperties.ModelMatrix = GlobalMatrix;
        renderProperties.SkeletonBuffer = skeletonBuffer;
        renderProperties.SkeletonBufferPool = ArrayPool<Matrix4>.Shared;
        renderProperties.SkeletonBufferSize = skeletonInstance.Skeleton.JointCount;

        foreach (var mesh in ModelProperty.Value.Meshes)
        {
            renderProperties.Mesh = mesh.ActualMesh;
            renderProperties.Material = mesh.Material;
            renderPipeline.SubmitMeshRender(DefaultRenderLayers.OpaqueLayer, renderProperties);
        }
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
                ImGuizmoOperation.Translate | ImGuizmoOperation.Rotate | ImGuizmoOperation.Scale, ImGuizmoMode.World, ref model[0]))
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
        base.RenderGizmo(context);
        
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
            Matrix4 matrix4 = skeletonInstance.GetJointGlobalMatrix(skeletonInstance.Skeleton.GetJoint(i)) *
                              GlobalMatrix;

            context.ModelMatrix = matrix4;
            context.BillboardedSprite(Scene.Game.ResourceManager.Load<Texture>("icons/bone.dds"), 0.2f);
        }
    }
}