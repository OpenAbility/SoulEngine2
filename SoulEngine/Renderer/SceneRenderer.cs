using OpenTK.Mathematics;
using SoulEngine.Components;
using SoulEngine.Core;
using SoulEngine.Mathematics;
using SoulEngine.Rendering;
using SoulEngine.UI;

namespace SoulEngine.Renderer;

/// <summary>
/// Renders a scene
/// </summary>
public class SceneRenderer
{
    public readonly Scene Scene;

    private readonly GizmoContext gizmoContext;
    private readonly UIContext uiContext;

    private float rotateSpeed;
    
    public SceneRenderer(Scene scene)
    {
        Scene = scene;
        uiContext = new UIContext(Scene.Game);
        gizmoContext = new GizmoContext(Scene.Game);

    }

    public CameraSettings? MakePipelineCamera(CameraSettings cameraSettings, IRenderSurface surface)
    {
        Vector2 surfaceSize = surface.FramebufferSize;
        
        if (cameraSettings.CameraMode == CameraMode.GameCamera)
        {
            CameraComponent? camera = Scene.Camera;
            if (camera == null)
                return null;


            cameraSettings.ViewMatrix = camera.GetView();
            cameraSettings.ProjectionMatrix = camera.GetProjection(surfaceSize.X / surfaceSize.Y);
            

            cameraSettings.CameraPosition = camera.Entity.Position;
            cameraSettings.CameraDirection = camera.Entity.Forward;
            cameraSettings.CameraRight = camera.Entity.Right;
            cameraSettings.CameraUp = camera.Entity.Up;
            cameraSettings.FieldOfView = camera.FieldOfView;
            cameraSettings.NearPlane = camera.NearPlane;
            cameraSettings.FarPlane = camera.FarPlane;

        }
        else if (cameraSettings.CameraMode == CameraMode.FreeCamera)
        {
            CameraComponent? camera = Scene.Camera;
            if (camera != null)
            {
                cameraSettings.CameraPosition = camera.Entity.Position;
                cameraSettings.CameraDirection = camera.Entity.Forward;
                cameraSettings.CameraRight = camera.Entity.Right;
                cameraSettings.CameraUp = camera.Entity.Up;
                cameraSettings.FieldOfView = camera.FieldOfView;
                cameraSettings.NearPlane = camera.NearPlane;
                cameraSettings.FarPlane = camera.FarPlane;
            }
        }

        return cameraSettings;
    }

    public void Render(IRenderPipeline renderPipeline, IRenderSurface surface, float deltaTime, CameraSettings cameraSettings)
    {
        
        Vector2 surfaceSize = surface.FramebufferSize;
        
        if (cameraSettings.CameraMode == CameraMode.GameCamera)
        {
            CameraComponent? camera = Scene.Camera;
            if (camera == null)
                return;


            cameraSettings.ViewMatrix = camera.GetView();
            cameraSettings.ProjectionMatrix = camera.GetProjection(surfaceSize.X / surfaceSize.Y);
            

            cameraSettings.CameraPosition = camera.Entity.Position;
            cameraSettings.CameraDirection = camera.Entity.Forward;
            cameraSettings.CameraRight = camera.Entity.Right;
            cameraSettings.CameraUp = camera.Entity.Up;
            cameraSettings.FieldOfView = camera.FieldOfView;
            cameraSettings.NearPlane = camera.NearPlane;
            cameraSettings.FarPlane = camera.FarPlane;

        }
        else if (cameraSettings.CameraMode == CameraMode.FreeCamera)
        {
            CameraComponent? camera = Scene.Camera;
            if (camera != null)
            {
                cameraSettings.CameraPosition = camera.Entity.Position;
                cameraSettings.CameraDirection = camera.Entity.Forward;
                cameraSettings.CameraRight = camera.Entity.Right;
                cameraSettings.CameraUp = camera.Entity.Up;
                cameraSettings.FieldOfView = camera.FieldOfView;
                cameraSettings.NearPlane = camera.NearPlane;
                cameraSettings.FarPlane = camera.FarPlane;
            }
        }
        
        Frustum frustum = Frustum.CreateFromCamera(cameraSettings.CameraPosition, cameraSettings.CameraDirection, cameraSettings.CameraRight, cameraSettings.CameraUp, surfaceSize.X / surfaceSize.Y, cameraSettings.FieldOfView, cameraSettings.NearPlane, cameraSettings.FarPlane);
        
        SceneRenderData renderData = new SceneRenderData();
        renderData.CameraSettings = cameraSettings;
        renderData.RenderSurface = surface;
        foreach (var prop in Scene.Props)
        {
            if(frustum.InFrustum(prop.Position))
                prop.Render(renderPipeline, renderData, deltaTime);
        }
        
    }
}