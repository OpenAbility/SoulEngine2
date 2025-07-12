using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Entities;
using SoulEngine.Mathematics;
using SoulEngine.Props;
using SoulEngine.Rendering;

namespace SoulEngine.Components;

[Component("shadow_camera", Icon = "camera")]
public class ShadowCameraComponent : Component
{

    [SerializedProperty("size")] public float Size = 3;
    [SerializedProperty("follow_camera")] public bool FollowCamera = true;
    
    [SerializedProperty("near")] public float Near = 0.1f;
    [SerializedProperty("far")] public float Far = 100f;
    
    
    public ShadowCameraComponent(Entity entity) : base(entity)
    {
    }

    public override void Update(float deltaTime)
    {
        if (FollowCamera && Scene.Camera != null)
            Entity.Position = Scene.Camera.Entity.Position;
    }

    public Matrix4 GetView()
    {
        return Matrix4.LookAt(Entity.Position, Entity.Position + Entity.Forward, Vector3.UnitY);
    }

    public Matrix4 GetProjection()
    {
        return Matrix4.CreateOrthographicOffCenter(-Size / 2f, Size / 2f, -Size / 2f, Size / 2f, Near, Far);
    }

    public override void RenderGizmo(GizmoContext context)
    {
        context.Begin(PrimitiveType.Lines);
        
        context.Vertex(new Vector3(-Size / 2f, -Size / 2f,  -Near));
        context.Vertex(new Vector3(-Size / 2f,  Size / 2f,  -Near));
        
        context.Vertex(new Vector3(-Size / 2f,  Size / 2f,  -Near));
        context.Vertex(new Vector3( Size / 2f,  Size / 2f,  -Near));
        
        context.Vertex(new Vector3( Size / 2f,  Size / 2f,  -Near));
        context.Vertex(new Vector3( Size / 2f,  -Size / 2f, -Near));

        context.Vertex(new Vector3( Size / 2f,  -Size / 2f, -Near));
        context.Vertex(new Vector3(-Size / 2f,  -Size / 2f, -Near));
        
        context.Vertex(new Vector3(-Size / 2f, -Size / 2f,  -Far));
        context.Vertex(new Vector3(-Size / 2f,  Size / 2f,  -Far));
        
        context.Vertex(new Vector3(-Size / 2f,  Size / 2f,  -Far));
        context.Vertex(new Vector3( Size / 2f,  Size / 2f,  -Far));
        
        context.Vertex(new Vector3( Size / 2f,  Size / 2f,  -Far));
        context.Vertex(new Vector3( Size / 2f,  -Size / 2f, -Far));

        context.Vertex(new Vector3( Size / 2f,  -Size / 2f, -Far));
        context.Vertex(new Vector3(-Size / 2f,  -Size / 2f, -Far));
        
        context.Vertex(new Vector3(-Size / 2f, -Size / 2f,  -Near));
        context.Vertex(new Vector3(-Size / 2f, -Size / 2f,  -Far));
        
        context.Vertex(new Vector3(-Size / 2f,  Size / 2f,  -Near));
        context.Vertex(new Vector3(-Size / 2f,  Size / 2f,  -Far));
        
        context.Vertex(new Vector3( Size / 2f,  Size / 2f,  -Near));
        context.Vertex(new Vector3( Size / 2f,  Size / 2f,  -Far));
        
        context.Vertex(new Vector3( Size / 2f,  -Size / 2f, -Near));
        context.Vertex(new Vector3( Size / 2f,  -Size / 2f, -Far));
        
        context.End();
    }
}