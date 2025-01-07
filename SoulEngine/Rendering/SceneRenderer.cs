using OpenTK.Mathematics;
using SoulEngine.Core;

namespace SoulEngine.Rendering;

/// <summary>
/// Renders a scene
/// </summary>
public class SceneRenderer
{
    public readonly Scene Scene;

    public SceneRenderer(Scene scene)
    {
        Scene = scene;
    }

    public void Render(IRenderSurface surface, float deltaTime)
    {
        surface.BindFramebuffer();

        SceneRenderData renderData = new SceneRenderData();
        Vector2 surfaceSize = surface.FramebufferSize;
        
        renderData.RenderSurface = surface;
        renderData.CameraProjectionMatrix =
            Matrix4.CreatePerspectiveFieldOfView(60 * MathF.PI / 180f, surfaceSize.X / surfaceSize.Y, 0.1f, 1000f);
        renderData.CameraViewMatrix = Matrix4.LookAt(new Vector3(2, 2, 2), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

        foreach (var prop in Scene.Props.Values)
        {
            prop.Render(renderData, deltaTime);
        }
        
    }
}