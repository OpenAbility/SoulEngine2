using OpenTK.Mathematics;

namespace SoulEngine.Renderer;

public struct ShadowCameraSettings
{
    public Matrix4 ViewMatrix;
    public Matrix4 ProjectionMatrix;

    public Vector3 Direction;
}