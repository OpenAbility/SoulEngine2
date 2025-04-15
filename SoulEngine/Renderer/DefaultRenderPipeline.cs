using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Rendering;
using SoulEngine.Resources;
using SoulEngine.UI.Rendering;

namespace SoulEngine.Renderer;

public class DefaultRenderPipeline : IRenderPipeline
{

    private readonly Material defaultMaterial = ResourceManager.Global.Load<Material>("default.mat");
    private readonly List<MeshRenderProperties> renders = new List<MeshRenderProperties>();
    private readonly List<DrawListData> drawLists = new List<DrawListData>();

    private GpuBuffer<Matrix4>? skeletonBuffer;
    
    public void SubmitMeshRender(RenderLayer renderLayer, MeshRenderProperties renderProperties)
    {
        if(renderProperties.Material == null!)
            renderProperties.Material = defaultMaterial;
        
        
        renders.Add(renderProperties);
    }

    public void SubmitDrawList(RenderLayer renderLayer, DrawListData drawListData)
    {
        if(drawListData.Material == null!)
            drawListData.Material = defaultMaterial;
        
        
        drawLists.Add(drawListData);
    }

    public IEnumerable<RenderLayer> GetLayers()
    {
        yield return DefaultRenderLayers.OpaqueLayer;
    }

    public void DrawFrame(PipelineData pipelineData)
    {
        RenderContext renderContext = pipelineData.RenderContext;
        
        RenderPass pass = new RenderPass
        {
            Name = "Main Pass",
            Surface = pipelineData.TargetSurface,
        };
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

        foreach (var render in renders)
        {
            render.Material.Bind(pipelineData.CameraSettings, render.ModelMatrix);
            render.Material.Shader.Uniform1i("ub_skeleton", 0);

            if (render.SkeletonBuffer != null)
            {
                // TODO: Move this around to reduce copies etc
                
                // Allocate the skeleton buffer
                if ((skeletonBuffer?.Length ?? 0) < render.SkeletonBufferSize)
                {
                    skeletonBuffer?.Dispose();
                    skeletonBuffer = new GpuBuffer<Matrix4>((int)(render.SkeletonBufferSize * 1.5f),
                        BufferStorageMask.MapWriteBit | BufferStorageMask.DynamicStorageBit | BufferStorageMask.MapCoherentBit | BufferStorageMask.MapPersistentBit | BufferStorageMask.ClientStorageBit);
                }
                
                
                BufferMapping<Matrix4> mapping = skeletonBuffer!.Map(0,  render.SkeletonBufferSize,
                    MapBufferAccessMask.MapCoherentBit | MapBufferAccessMask.MapPersistentBit | MapBufferAccessMask.MapWriteBit |
                    MapBufferAccessMask.MapInvalidateRangeBit);


                for (int i = 0; i < render.SkeletonBufferSize; i++)
                {
                    mapping.Span[i] = render.SkeletonBuffer[i];
                }
        
                mapping.Dispose();
                
                
                render.Material.Shader.Uniform1i("ub_skeleton", 1);
                render.Material.Shader.BindBuffer("um_joint_buffer", skeletonBuffer, 0, render.SkeletonBufferSize);
            }
            
            render.Mesh.Draw();
        }
        renderContext.EndRendering();
        
        
        RenderPass uiPass = new RenderPass
        {
            Name = "UI Pass",
            Surface = pipelineData.TargetSurface,
        };
        uiPass.DepthStencilSettings.LoadOp = AttachmentLoadOp.Clear;
        uiPass.DepthStencilSettings.StoreOp = AttachmentStoreOp.DontCare;
        uiPass.DepthStencilSettings.ClearValue.Depth = 1;

        uiPass.ColorSettings =
        [
            new FramebufferAttachmentSettings()
            {
                LoadOp = AttachmentLoadOp.Load,
                StoreOp =  AttachmentStoreOp.Store
            }
        ];
        
        
        renderContext.BeginRendering(uiPass);
        
        renderContext.Disable(EnableCap.DepthTest);
        renderContext.Disable(EnableCap.CullFace);
        renderContext.DepthFunction(DepthFunction.Always);
        renderContext.Enable(EnableCap.Blend);
        renderContext.DepthRange(-1, 1);
        
        renderContext.Disable(EnableCap.FramebufferSrgb);

        if (pipelineData.UIContext != null)
        {
            pipelineData.UIContext.DrawAll();
        }
        renderContext.EndRendering();


        renders.Clear();
        drawLists.Clear();
    }
}