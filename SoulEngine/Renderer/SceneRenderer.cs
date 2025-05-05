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
public class SceneRenderer : EngineObject
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

    public void Render(SceneRendererData data, ref PipelineData pipelineData)
    {
        Vector2 surfaceSize = data.FramebufferSize;
        
        if (data.CameraSettings.CameraMode == CameraMode.GameCamera)
        {
            CameraComponent? camera = Scene.Camera;
            if (camera == null)
                return;


            data.CameraSettings.ViewMatrix = camera.GetView();
            data.CameraSettings.ProjectionMatrix = camera.GetProjection(surfaceSize.X / surfaceSize.Y);
            

            data.CameraSettings.CameraPosition = camera.Entity.Position;
            data.CameraSettings.CameraDirection = camera.Entity.Forward;
            data.CameraSettings.CameraRight = camera.Entity.Right;
            data.CameraSettings.CameraUp = camera.Entity.Up;
            data.CameraSettings.FieldOfView = camera.FieldOfView;
            data.CameraSettings.NearPlane = camera.NearPlane;
            data.CameraSettings.FarPlane = camera.FarPlane;

        }
        else if (data.CameraSettings.CameraMode == CameraMode.FreeCamera)
        {
            CameraComponent? camera = Scene.Camera;
            if (camera != null)
            {
                data.CameraSettings.CameraPosition = camera.Entity.Position;
                data.CameraSettings.CameraDirection = camera.Entity.Forward;
                data.CameraSettings.CameraRight = camera.Entity.Right;
                data.CameraSettings.CameraUp = camera.Entity.Up;
                data.CameraSettings.FieldOfView = camera.FieldOfView;
                data.CameraSettings.NearPlane = camera.NearPlane;
                data.CameraSettings.FarPlane = camera.FarPlane;
            }
        }
        
        Frustum frustum = Frustum.CreateFromCamera(data.CameraSettings.CameraPosition, 
            data.CameraSettings.CameraDirection, 
            data.CameraSettings.CameraRight, 
            data.CameraSettings.CameraUp, 
            surfaceSize.X / surfaceSize.Y, 
            data.CameraSettings.FieldOfView, data.CameraSettings.NearPlane, data.CameraSettings.FarPlane);
        
        foreach (var entity in Scene.Entities)
        {
            if(data.CullPass)
                entity.WasCulled = !frustum.InFrustum(entity.Position);
            
            if(!entity.WasCulled)
                entity.Render(data.RenderPipeline, data.DeltaTime);
        }
        
        if (data.CameraSettings.ShowUI && data.UIContext != null)
        {
            Vector2i targetResolution = new Vector2i(3840, 2160);

            float ratio = surfaceSize.X / surfaceSize.Y;
            
            data.UIContext.OnBegin(new Vector2i(targetResolution.X, (int)(targetResolution.X / ratio)));

            Scene.Director?.RenderUI(data.UIContext);
            
            data.UIContext.EnsureEnded();
        }
        
    }

    public void RenderGizmo(ref PipelineData pipelineData)
    {
        PipelineData dataRO = pipelineData;
        
        GizmoContext gizmoContext = new GizmoContext(pipelineData.Game);
        gizmoContext.ProjectionMatrix = pipelineData.CameraSettings.ProjectionMatrix;
        gizmoContext.ViewMatrix = pipelineData.CameraSettings.ViewMatrix;
            
        foreach (var entity in Scene.Entities)
        {
            pipelineData.DrawGizmos += () =>
            {
                gizmoContext.Selected = dataRO.CameraSettings.SelectedEntity == entity;
                gizmoContext.ModelMatrix = entity.GlobalMatrix;
                entity.RenderGizmo(gizmoContext);
            };
        }
    }
}