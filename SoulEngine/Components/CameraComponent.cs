using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Mathematics;
using SoulEngine.Props;
using SoulEngine.Rendering;

namespace SoulEngine.Components;

[Component("camera")]
public class CameraComponent : Component, IComparable<CameraComponent>
{
    [SerializedProperty("fov")] public float FieldOfView { get;
        set => field = Mathf.Clamp(value, 0.1f, 179.9f);
    } = 60;
    [SerializedProperty("near")] public float NearPlane { get; set => field = Mathf.Clamp(value, float.Epsilon, FarPlane - 0.01f); } = 0.1f;
    [SerializedProperty("far")] public float FarPlane { get; set => field = Mathf.Clamp(value, NearPlane + 0.01f, 10_000_000f); }= 1000f;
    
    [SerializedProperty("priority")] public int Priority { get; set; }
    
    
    public CameraComponent(Entity entity) : base(entity)
    {
    }
    
    public Matrix4 GetView()
    {
        return Matrix4.LookAt(Entity.Position, Entity.Position + Entity.Forward, Vector3.UnitY);
    }

    public Matrix4 GetProjection(float aspect)
    {
        return Matrix4.CreatePerspectiveFieldOfView(FieldOfView * MathF.PI / 180f, aspect, NearPlane, FarPlane);
    }

    public override void RenderGizmo(GizmoContext context)
    {
        base.RenderGizmo(context);


        Matrix4 oldModel = context.ModelMatrix;

        context.ModelMatrix = Matrix4.Identity;
        context.Begin(PrimitiveType.Lines);

        Vector3[] frustumPoints = Frustum.CreateFromCamera(Entity.Position, Entity.Forward, Entity.Right, Vector3.UnitY,
            context.CurrentAspectRatio, FieldOfView, NearPlane, FarPlane).GetCorners();

        context.Vertex(frustumPoints[0]);
        context.Vertex(frustumPoints[1]);

        context.Vertex(frustumPoints[1]);
        context.Vertex(frustumPoints[3]);

        context.Vertex(frustumPoints[3]);
        context.Vertex(frustumPoints[2]);

        context.Vertex(frustumPoints[2]);
        context.Vertex(frustumPoints[0]);

        context.Vertex(frustumPoints[4]);
        context.Vertex(frustumPoints[5]);

        context.Vertex(frustumPoints[5]);
        context.Vertex(frustumPoints[7]);

        context.Vertex(frustumPoints[7]);
        context.Vertex(frustumPoints[6]);

        context.Vertex(frustumPoints[6]);
        context.Vertex(frustumPoints[4]);

        context.Vertex(frustumPoints[0]);
        context.Vertex(frustumPoints[4]);

        context.Vertex(frustumPoints[1]);
        context.Vertex(frustumPoints[5]);

        context.Vertex(frustumPoints[2]);
        context.Vertex(frustumPoints[6]);

        context.Vertex(frustumPoints[3]);
        context.Vertex(frustumPoints[7]);

        context.End();
        context.ModelMatrix = oldModel;

    }

    public int CompareTo(CameraComponent? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Priority.CompareTo(other.Priority);
    }
}