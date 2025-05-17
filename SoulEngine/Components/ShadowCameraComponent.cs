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

    [SerializedProperty("size")] public float Size;
    
    public ShadowCameraComponent(Entity entity) : base(entity)
    {
    }
    
    public Matrix4 GetView()
    {
        return Matrix4.LookAt(Entity.Position, Entity.Position + Entity.Forward, Vector3.UnitY);
    }

    public Matrix4 GetProjection()
    {
        return Matrix4.CreateOrthographic(Size, Size, 0.1f, 100f);
    }
}