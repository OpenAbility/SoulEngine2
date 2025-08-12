using OpenTK.Mathematics;
using SoulEngine.Components;
using SoulEngine.Core;
using SoulEngine.Data;
using SoulEngine.Entities;
using SoulEngine.Mathematics;
using SoulEngine.Rendering;
using SoulEngine.UI;

namespace SoulEngine.Renderer;

public class SceneRenderer2 : EngineObject
{
    private readonly GizmoContext gizmoContext;
    private readonly Game game;
    
    public SceneRenderer2(Game game)
    {
        this.game = game;
        gizmoContext = new GizmoContext(game);
    }

    public CameraSettings ApplyCameraSettings(IEntityCollection scene, CameraSettings cameraSettings, IRenderSurface targetSurface)
    {
        Vector2 surfaceSize = targetSurface.FramebufferSize;
        
        if (cameraSettings.CameraMode == CameraMode.GameCamera)
        {
            CameraComponent? camera = scene.Camera;
            if (camera == null)
            {
                cameraSettings.ProjectionMatrix = Matrix4.Zero;
                return cameraSettings;
            }


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
            CameraComponent? camera = scene.Camera;
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

    private void DrawGizmos(IEntityCollection scene,  PipelineData pipelineData, Predicate<Entity> predicate)
    {
        Entity? selectedEntity = pipelineData.CameraSettings.SelectedEntity;

        gizmoContext.ProjectionMatrix = pipelineData.CameraSettings.ProjectionMatrix;
        gizmoContext.ViewMatrix = pipelineData.CameraSettings.ViewMatrix;

        foreach (var entity in scene.EntityEnumerable)
        {
            if(!predicate(entity))
                continue;
            
            gizmoContext.Selected = selectedEntity == entity;
            gizmoContext.ModelMatrix = entity.GlobalMatrix;
            entity.RenderGizmo(gizmoContext);
        }
        
    }

    public void PerformEntityCulling(IEntityCollection scene, CameraSettings cameraSettings, IRenderSurface targetSurface)
    {
        Frustum frustum = Frustum.CreateFromCamera(cameraSettings.CameraPosition, 
            cameraSettings.CameraDirection, 
            cameraSettings.CameraRight, 
            (float)targetSurface.FramebufferSize.X / targetSurface.FramebufferSize.Y, 
            cameraSettings.FieldOfView, cameraSettings.NearPlane, cameraSettings.FarPlane);

        foreach (var entity in scene.EntityEnumerable)
        { 
            entity.WasCulled = !frustum.InFrustum(entity.RenderingBoundingBox());
        }
    }

    public void PerformEntityRender(IEntityCollection scene, float deltaTime, IRenderPipeline pipeline, bool enableCulling)
    {
        foreach (var entity in scene.EntityEnumerable)
        { 
            if(enableCulling && entity.WasCulled)
                continue;
            
            entity.Render(pipeline, deltaTime);
        }
    }

    public void PushShadowInformation(IEntityCollection scene, ref PipelineData pipelineData)
    {
        pipelineData.EnableShadows = false;

        ShadowCameraComponent? shadowCamera = scene.ShadowCamera;
        if (shadowCamera != null)
        {
            pipelineData.ShadowCameraSettings.ViewMatrix = shadowCamera.GetView();
            pipelineData.ShadowCameraSettings.ProjectionMatrix = shadowCamera.GetProjection();
            pipelineData.ShadowCameraSettings.Direction = shadowCamera.Entity.Forward;
            
            pipelineData.EnableShadows = EngineVarContext.Global.GetBool("e_shadows", true);
        }
    }

    public void PerformGameRender(SceneRenderInformation info)
    {
        PipelineData pipelineData = new PipelineData();
        pipelineData.Game = game;
        pipelineData.RenderContext = info.RenderContext;
        pipelineData.CameraSettings = ApplyCameraSettings(info.EntityCollection, info.CameraSettings, info.TargetSurface);
        pipelineData.DeltaTime = info.DeltaTime;
        pipelineData.TargetSurface = info.TargetSurface;
        pipelineData.UIContext = info.UIContext;
        pipelineData.PostProcessing = EngineVarContext.Global.GetBool("e_post", true) && info.PostProcessing;

        PushShadowInformation(info.EntityCollection, ref pipelineData);
        if(info.PerformCullingPass)
            PerformEntityCulling(info.EntityCollection, pipelineData.CameraSettings, info.TargetSurface);
        PerformEntityRender(info.EntityCollection, info.DeltaTime, info.RenderPipeline, info.EnableCulling);

        if (info.CameraSettings.ShowGizmos)
            pipelineData.DrawGizmos = () => DrawGizmos(info.EntityCollection, pipelineData, info.GizmoPredicate);
        
        if (info.CameraSettings.ShowUI && info.UIContext != null)
        {
            Vector2i targetResolution = new Vector2i(3840, 2160);

            float ratio = (float)info.TargetSurface.FramebufferSize.X / info.TargetSurface.FramebufferSize.Y;
            
            info.UIContext.OnBegin(new Vector2i(targetResolution.X, (int)(targetResolution.X / ratio)));

            info.RenderUI(info.UIContext);
            
            info.UIContext.EnsureEnded();
        }
        
        info.RenderPipeline.DrawFrame(pipelineData);
    }
}