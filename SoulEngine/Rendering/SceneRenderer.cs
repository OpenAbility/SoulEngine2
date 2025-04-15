
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Components;
using SoulEngine.Core;
using SoulEngine.Mathematics;
using SoulEngine.Props;
using SoulEngine.UI;

namespace SoulEngine.Rendering;

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

    public void Render(RenderContext renderContext, IRenderSurface surface, float deltaTime, CameraSettings cameraSettings)
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



        RenderPass pass = new RenderPass();
        pass.Name = Scene.ResourceID + " - Main Pass";
        pass.Surface = surface;
        pass.DepthStencilSettings.LoadOp = AttachmentLoadOp.Clear;
        pass.DepthStencilSettings.StoreOp = AttachmentStoreOp.Store;
        pass.DepthStencilSettings.ClearValue.Depth = 1;

        pass.ColorSettings =
        [
            new FramebufferAttachmentSettings()
            {
                LoadOp = AttachmentLoadOp.Clear,
                ClearValue = new FramebufferClearValue()
                {
                    Colour = Colour.Blank
                },
                StoreOp =  AttachmentStoreOp.Store
            }
        ];
        
        
        renderContext.BeginRendering(pass);
        
        renderContext.Enable(EnableCap.DepthTest);
        renderContext.Disable(EnableCap.CullFace);
        renderContext.DepthFunction(DepthFunction.Less);
        renderContext.Enable(EnableCap.FramebufferSrgb);
        renderContext.DepthRange(-1, 1);
        
        SceneRenderData renderData = new SceneRenderData();
        renderData.CameraSettings = cameraSettings;
        renderData.RenderSurface = surface;
        foreach (var prop in Scene.Props)
        {
            if(frustum.InFrustum(prop.Position))
                prop.Render(renderContext, renderData, deltaTime);
        }
        
        renderContext.Disable(EnableCap.FramebufferSrgb);

        if (cameraSettings.ShowUI)
        {
            Vector2i targetResolution = new Vector2i(3840, 2160);

            float ratio = surfaceSize.X / surfaceSize.Y;
            
            renderContext.Enable(EnableCap.Blend);
            renderContext.BlendFunction(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            
            uiContext.OnBegin(renderContext, new Vector2i(targetResolution.X, (int)(targetResolution.X / ratio)));
            //uiContext.OnBegin(renderContext, surface.FramebufferSize);

            Scene.Director?.RenderUI(uiContext);
            
            uiContext.EnsureEnded();
        }
        renderContext.EndRendering();

        if(cameraSettings.ShowGizmos) {
        
            pass.Name = Scene.ResourceID + " - Gizmo Pass";
            pass.ColorSettings =
            [
                new FramebufferAttachmentSettings()
                {
                    LoadOp = AttachmentLoadOp.Load,
                    ClearValue = new FramebufferClearValue()
                    {
                        Colour = Colour.Blank
                    },
                    StoreOp =  AttachmentStoreOp.Store
                }
            ];
            
            renderContext.BeginRendering(pass);

           
            gizmoContext.ProjectionMatrix = cameraSettings.ProjectionMatrix;
            gizmoContext.ViewMatrix = cameraSettings.ViewMatrix;
            gizmoContext.SceneRenderData = renderData;
            gizmoContext.CameraFrustum = frustum;
            gizmoContext.CurrentAspectRatio = surfaceSize.X / surfaceSize.Y;
            
            foreach (var prop in Scene.Props)
            {

                gizmoContext.Selected = cameraSettings.SelectedProp == prop;
                gizmoContext.ModelMatrix = prop.GlobalMatrix;
                prop.RenderGizmo(gizmoContext);
            }
            
            renderContext.EndRendering();
        }
        
    }
}