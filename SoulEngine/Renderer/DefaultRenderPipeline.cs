using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Rendering;
using SoulEngine.Resources;

namespace SoulEngine.Renderer;

public class DefaultRenderPipeline : IRenderPipeline
{

    private readonly Material defaultMaterial = ResourceManager.Global.Load<Material>("default.mat");
    private readonly List<MeshRenderProperties> renders = new List<MeshRenderProperties>();

    private GpuBuffer<Matrix4>? skeletonBuffer;
    
    public void SubmitMeshRender(RenderLayer renderLayer, MeshRenderProperties renderProperties)
    {
        if(renderProperties.Material == null!)
            renderProperties.Material = defaultMaterial;
        
        
        renders.Add(renderProperties);
    }

    public IEnumerable<RenderLayer> GetLayers()
    {
        yield return DefaultRenderLayers.OpaqueLayer;
    }

    public void DrawFrame(RenderContext renderContext, IRenderSurface targetSurface, float deltaTime, CameraSettings cameraSettings)
    {
        
        RenderPass pass = new RenderPass
        {
            Name = "Main Pass",
            Surface = targetSurface,
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
            render.Material.Bind(cameraSettings, render.ModelMatrix);
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
        renderContext.Disable(EnableCap.FramebufferSrgb);
        
        renders.Clear();
    }
}