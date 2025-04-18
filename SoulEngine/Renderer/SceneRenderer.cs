using System.IO.Pipelines;
using OpenTK.Mathematics;
using SoulEngine.Components;
using SoulEngine.Core;
using SoulEngine.Data;
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
    private float rotateSpeed;
    
    public SceneRenderer(Scene scene)
    {
        Scene = scene;
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

    public void Render(IRenderPipeline renderPipeline, IRenderSurface surface, float deltaTime, CameraSettings cameraSettings, UIContext? uiContext)
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
            if(!EngineVarContext.Global.GetBool("e_frustum_cull") || frustum.InFrustum(prop.Position))
                prop.Render(renderPipeline, renderData, deltaTime);
        }
        
        if (cameraSettings.ShowUI && uiContext != null)
        {
            Vector2i targetResolution = new Vector2i(3840, 2160);

            float ratio = surfaceSize.X / surfaceSize.Y;
            
            uiContext.OnBegin(new Vector2i(targetResolution.X, (int)(targetResolution.X / ratio)));
            //uiContext.OnBegin(renderContext, surface.FramebufferSize);

            Scene.Director?.RenderUI(uiContext);
            
            uiContext.EnsureEnded();
        }
        
    }

    public void RenderGizmo(ref PipelineData pipelineData)
    {
        PipelineData dataRO = pipelineData;
        
        SceneRenderData renderData = new SceneRenderData();
        renderData.CameraSettings = pipelineData.CameraSettings;
        renderData.RenderSurface = pipelineData.TargetSurface;
        
        GizmoContext gizmoContext = new GizmoContext(pipelineData.Game);
        gizmoContext.ProjectionMatrix = pipelineData.CameraSettings.ProjectionMatrix;
        gizmoContext.ViewMatrix = pipelineData.CameraSettings.ViewMatrix;
        gizmoContext.SceneRenderData = renderData;
            
        foreach (var prop in Scene.Props)
        {
            pipelineData.DrawGizmos += () =>
            {
                gizmoContext.Selected = dataRO.CameraSettings.SelectedProp == prop;
                gizmoContext.ModelMatrix = prop.GlobalMatrix;
                prop.RenderGizmo(gizmoContext);
            };
        }
    }
}