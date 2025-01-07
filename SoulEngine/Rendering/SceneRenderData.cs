using OpenTK.Mathematics;

namespace SoulEngine.Rendering;

public struct SceneRenderData
{
    public IRenderSurface RenderSurface;
    public Matrix4 CameraProjectionMatrix;
    public Matrix4 CameraViewMatrix;
}