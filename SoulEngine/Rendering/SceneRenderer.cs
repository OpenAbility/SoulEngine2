
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
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

    private readonly UIContext uiContext;

    private float rotateSpeed;
    
    public SceneRenderer(Scene scene)
    {
        Scene = scene;
        uiContext = new UIContext(Scene.Game);

    }

    public void Render(RenderContext renderContext, IRenderSurface surface, float deltaTime, CameraSettings cameraSettings)
    {
        
        Vector2 surfaceSize = surface.FramebufferSize;

        if (cameraSettings.CameraMode == CameraMode.GameCamera)
        {
            CameraProp? cameraProp = Scene.Camera;
            if (cameraProp == null)
                return;


            cameraSettings.ViewMatrix = cameraProp.GetView();
            cameraSettings.ProjectionMatrix = cameraProp.GetProjection(surfaceSize.X / surfaceSize.Y);


            cameraSettings.CameraPosition = cameraProp.Position;
            cameraSettings.CameraDirection = cameraProp.Forward;
        }

        else if (cameraSettings.CameraMode == CameraMode.FreeCamera)
        {
            CameraProp? cameraProp = Scene.Camera;
            if (cameraProp != null)
            {
                
                cameraSettings.CameraPosition = cameraProp.Position;
                cameraSettings.CameraDirection = cameraProp.Forward;
            }
        }


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
        renderContext.DepthRange(-1, 1);
        
        SceneRenderData renderData = new SceneRenderData();
        renderData.CameraSettings = cameraSettings;
        renderData.RenderSurface = surface;

        foreach (var prop in Scene.Props)
        {
            prop.Render(renderContext, renderData, deltaTime);
        }

        if (cameraSettings.ShowUI)
        {
            Vector2i targetResolution = new Vector2i(3840, 2160);

            float widthScale = targetResolution.Y / surfaceSize.Y;
            
            renderContext.Enable(EnableCap.Blend);
            renderContext.BlendFunction(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            
            //uiContext.OnBegin(renderContext, new Vector2i((int)(surfaceSize.X * widthScale), targetResolution.Y));
            uiContext.OnBegin(renderContext, surface.FramebufferSize);

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

            GizmoContext gizmoContext = new GizmoContext(renderContext);
            gizmoContext.ProjectionMatrix = cameraSettings.ProjectionMatrix;
            gizmoContext.ViewMatrix = cameraSettings.ViewMatrix;
            gizmoContext.SceneRenderData = renderData;
            
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