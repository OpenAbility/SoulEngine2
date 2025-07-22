using System.Diagnostics;
using OpenAbility.Logging;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Compute;
using SoulEngine.Core;
using SoulEngine.Data;
using SoulEngine.Mathematics;
using SoulEngine.PostProcessing;
using SoulEngine.Rendering;
using SoulEngine.Resources;
using SoulEngine.UI.Rendering;

namespace SoulEngine.Renderer;

public class DefaultRenderPipeline : EngineObject, IRenderPipeline
{

    private static readonly Logger Logger = Logger.Get<DefaultRenderPipeline>();
    
    private readonly Material defaultMaterial = ResourceManager.Global.Load<Material>("default.mat");

    private readonly ComputeShader skinningShader =
        ResourceManager.Global.Load<ComputeShader>("shader/comp/skeleton_anim.comp");

    private readonly int vertexArray;
    private readonly Dictionary<IRenderSurface, PostProcessor> postProcessors =
        new Dictionary<IRenderSurface, PostProcessor>();

    private readonly Dictionary<RenderLayer, RenderLayerData> renderLayers =
        new Dictionary<RenderLayer, RenderLayerData>();
    
    private GpuBuffer<Matrix4> skeletonBuffer;
    
    private readonly Depthbuffer[] shadowBuffers = new Depthbuffer[3];
    private int lastShadowBufferSize = -1;

    public const BufferStorageMask SkeletonBufferParameters =
        BufferStorageMask.MapWriteBit | BufferStorageMask.DynamicStorageBit;

    public DefaultRenderPipeline()
    {
        vertexArray = new Vertex().CreateVertexArray();

        skeletonBuffer =
            new GpuBuffer<Matrix4>(EngineVarContext.Global.GetInt("e_buf_skele", 128), SkeletonBufferParameters);

        int shadowResolution = EngineVarContext.Global.GetInt("e_shadow_res", 2048);
        for (int i = 0; i < shadowBuffers.Length; i++)
        {
            shadowBuffers[i] = new Depthbuffer(new Vector2i(shadowResolution));
        }

        lastShadowBufferSize = shadowResolution;
        
        RegisterRenderLayer(DefaultRenderLayers.OpaqueLayer);
    }

    public void RegisterRenderLayer(RenderLayer renderLayer)
    {
        renderLayers.Add(renderLayer, new RenderLayerData(renderLayer));
    }

    public void DeregisterRenderLayer(RenderLayer renderLayer)
    {
        renderLayers.Remove(renderLayer);
    }
    
    public void SubmitMeshRender(RenderLayer renderLayer, MeshRenderProperties renderProperties)
    {
        if(renderProperties.Material == null!)
            renderProperties.Material = defaultMaterial;
        
        
        renderLayers[renderLayer].MeshRenders.Add(renderProperties);
    }

    public void SubmitDrawList(RenderLayer renderLayer, DrawListData drawListData)
    {
        if(drawListData.Material == null!)
            drawListData.Material = defaultMaterial;
        
        renderLayers[renderLayer].DrawLists.Add(drawListData);
    }

    public IEnumerable<RenderLayer> GetLayers()
    {
        yield return DefaultRenderLayers.OpaqueLayer;
    }

    public void DrawFrame(PipelineData pipelineData)
    {
        
        using var profilerPass = Profiler.Instance.Segment("renderer");

        if (lastShadowBufferSize != EngineVarContext.Global.GetInt("e_shadow_res", 2048))
        {
            int shadowResolution = EngineVarContext.Global.GetInt("e_shadow_res", 2048);
            for (int i = 0; i < shadowBuffers.Length; i++)
            {
                shadowBuffers[i] = new Depthbuffer(new Vector2i(shadowResolution));
            }

            lastShadowBufferSize = shadowResolution;
        }

        PostProcessedSurface? postProcessableSurface = null;
        PostProcessor? postProcessor = null;
        
        if (pipelineData.PostProcessing)
        {
            if (!postProcessors.TryGetValue(pipelineData.TargetSurface, out postProcessor))
            {
                postProcessor = new PostProcessor(pipelineData.Game, pipelineData.TargetSurface);
                postProcessors[pipelineData.TargetSurface] = postProcessor;
            }
            
            postProcessableSurface = postProcessor.InitializeFrameSurface();
        }

        RenderContext renderContext = pipelineData.RenderContext;
        
        SkinningPass();

        if (pipelineData.EnableShadows && pipelineData.RedrawShadows)
        {
            ShadowsPass(pipelineData);
        }

        #region G PASS
        
        {
            RenderPass gPass = new RenderPass
            {
                Name = "G-Buffer Pass",
                Surface = pipelineData.PostProcessing ? postProcessableSurface! : pipelineData.TargetSurface,
            };
            gPass.DepthStencilSettings.LoadOp = AttachmentLoadOp.Clear;
            gPass.DepthStencilSettings.StoreOp = AttachmentStoreOp.Store;
            gPass.DepthStencilSettings.ClearValue.Depth = 1;

            gPass.ColorSettings =
            [
                new FramebufferAttachmentSettings()
                {
                    LoadOp = AttachmentLoadOp.Clear,
                    ClearValue = new FramebufferClearValue()
                    {
                        Colour = Colour.SkyBlue
                    },
                    StoreOp = AttachmentStoreOp.DontStore
                },
                new FramebufferAttachmentSettings()
                {
                    LoadOp = AttachmentLoadOp.DontCare,
                    StoreOp = AttachmentStoreOp.Store
                }
            ];


            renderContext.BeginRendering(gPass);

            renderContext.Enable(EnableCap.DepthTest);
            renderContext.Enable(EnableCap.CullFace);
            renderContext.DepthFunction(DepthFunction.Less);
            renderContext.DepthRange(-1, 1);

            foreach (var layer in renderLayers.Values)
            {
                if(!layer.RenderLayer.DualPass)
                    continue;
                RenderLayerDraw draw = new RenderLayerDraw();
                draw.PipelineData = pipelineData;
                draw.RenderMode = RenderMode.DeferredPass;
                draw.Layer = layer;
                
                DrawRenderLayer(draw);
            }

            renderContext.EndRendering();
        }
                

        #endregion

        #region FINAL PASS

        {
            RenderPass finalPass = new RenderPass
            {
                Name = "Final Render Pass",
                Surface = pipelineData.PostProcessing ? postProcessableSurface! : pipelineData.TargetSurface,
            };
            finalPass.DepthStencilSettings.LoadOp = AttachmentLoadOp.Load;
            finalPass.DepthStencilSettings.StoreOp = AttachmentStoreOp.DontStore;

            finalPass.ColorSettings =
            [
                new FramebufferAttachmentSettings()
                {
                    LoadOp = AttachmentLoadOp.Load,
                    ClearValue = new FramebufferClearValue()
                    {
                        Colour = Colour.Blank
                    },
                    StoreOp = AttachmentStoreOp.Store
                },
                new FramebufferAttachmentSettings()
                {
                    LoadOp = AttachmentLoadOp.Load,
                    StoreOp = AttachmentStoreOp.DontStore
                }
            ];


            renderContext.BeginRendering(finalPass);

            renderContext.Enable(EnableCap.DepthTest);
            renderContext.Enable(EnableCap.CullFace);
            renderContext.DepthFunction(DepthFunction.Lequal);
            renderContext.DepthRange(-1, 1);

            foreach (var layer in renderLayers.Values)
            {
                RenderLayerDraw draw = new RenderLayerDraw();
                draw.PipelineData = pipelineData;
                draw.RenderMode = RenderMode.FinalPass;
                draw.Layer = layer;
                
                DrawRenderLayer(draw);
            }
            
            renderContext.EndRendering();
        }

        #endregion


        if(pipelineData.PostProcessing)
            postProcessor!.FinishedDrawing(renderContext, postProcessableSurface!);

        
        if (pipelineData.CameraSettings.ShowGizmos)
        {
            RenderPass gizmoPass = new RenderPass
            {
                Name = "Gizmo Pass",
                Surface = pipelineData.TargetSurface,
            };
            gizmoPass.DepthStencilSettings.LoadOp = AttachmentLoadOp.DontCare;
            gizmoPass.DepthStencilSettings.StoreOp = AttachmentStoreOp.DontStore;
            gizmoPass.DepthStencilSettings.ClearValue.Depth = 1;

            gizmoPass.ColorSettings =
            [
                new FramebufferAttachmentSettings()
                {
                    LoadOp = AttachmentLoadOp.Load,
                    StoreOp =  AttachmentStoreOp.Store
                }
            ];
        
        
            renderContext.BeginRendering(gizmoPass);
        
            renderContext.Disable(EnableCap.DepthTest);
            renderContext.Disable(EnableCap.CullFace);
            renderContext.Enable(EnableCap.Blend);
            //renderContext.Enable(EnableCap.FramebufferSrgb);
            renderContext.DepthRange(-1, 1);
            
            pipelineData.DrawGizmos();
            
            renderContext.EndRendering();
        }
        
        
        RenderPass uiPass = new RenderPass
        {
            Name = "UI Pass",
            Surface = pipelineData.TargetSurface,
        };
        uiPass.DepthStencilSettings.LoadOp = AttachmentLoadOp.Clear;
        uiPass.DepthStencilSettings.StoreOp = AttachmentStoreOp.DontStore;
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
        
        renderContext.BlendFunction(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        //renderContext.Disable(EnableCap.FramebufferSrgb);

        if (pipelineData.UIContext != null)
        {
            pipelineData.UIContext.DrawAll();
        }
        renderContext.EndRendering();


        foreach (var layer in renderLayers.Values)
        {
            layer.MeshRenders.Clear();
            layer.DrawLists.Clear();
        }
        
        if(EngineVarContext.Global.GetBool("e_gl_wait", true))
            GL.Finish();
    }

    private void SkinningPass()
    {
        foreach (var layer in renderLayers.Values)
        {
            if(!layer.RenderLayer.Skeletal)
                continue;
            foreach (var render in layer.MeshRenders)
            {
                CalculateSkinning(render);
            }
        }
    }

    private void CalculateSkinning(MeshRenderProperties render)
    {
        if(render.SkeletonBuffer == null || render.DeformationCache == null || render.Mesh == null!)
            return;
        
        render.Mesh.LockUpdates();
            
        GpuBuffer<Vertex>? vertexBuffer = render.Mesh.GetVertexBuffer();
        GpuBuffer<uint>? indexBuffer = render.Mesh.GetIndexBuffer();
        GpuBuffer<VertexSkinning>? skinningBuffer = render.Mesh.GetSkinningBuffer();

        if (vertexBuffer == null || indexBuffer == null || skinningBuffer == null)
        {
            render.Mesh.UnlockUpdates();
            return;
        }

        using ProfilerSegment segment = Profiler.Instance.Segment("frame.skinning");
        
        // Allocate the skeleton buffer
        if (skeletonBuffer.Length < render.SkeletonBufferSize)
        {
            Logger.Debug("Resizing the skeleton buffer!");
            skeletonBuffer.Dispose();
            skeletonBuffer = new GpuBuffer<Matrix4>((int)(render.SkeletonBufferSize * 1.5f), SkeletonBufferParameters);
        }
        

        BufferMapping<Matrix4> mapping = skeletonBuffer.Map(0, render.SkeletonBufferSize, MapBufferAccessMask.MapWriteBit | MapBufferAccessMask.MapInvalidateRangeBit);
        
        for (int i = 0; i < render.SkeletonBufferSize; i++)
        {
            mapping.Span[i] = render.SkeletonBuffer[i];
        }

        mapping.Dispose();

        skinningShader.BindBuffer("ib_joint_buffer", skeletonBuffer, 0, render.SkeletonBufferSize);
        skinningShader.BindBuffer("ib_vertex_buffer", vertexBuffer, 0, vertexBuffer.Length);
        skinningShader.BindBuffer("ib_weight_buffer", skinningBuffer, 0, skinningBuffer.Length);
        skinningShader.BindBuffer("ob_vertex_buffer", render.DeformationCache, 0, render.DeformationCache.Length);
        skinningShader.Dispatch(
            new Vector3i((int)MathF.Ceiling((float)vertexBuffer.Length / skinningShader.WorkGroupSize.X), 1, 1));
    }
    

    private void ShadowsPass(PipelineData pipelineData)
    {
        for (int i = 0; i < shadowBuffers.Length; i++)
        {
            ShadowBufferPass(pipelineData, i);
        }
    }

    private void ShadowBufferPass(PipelineData pipelineData, int bufferID)
    {
        RenderContext renderContext = pipelineData.RenderContext;
        
        RenderPass pass = new RenderPass
        {
            Name = "Shadow Pass",
            Surface = shadowBuffers[bufferID],
        };
        pass.DepthStencilSettings.LoadOp = AttachmentLoadOp.Clear;
        pass.DepthStencilSettings.StoreOp = AttachmentStoreOp.Store;
        pass.DepthStencilSettings.ClearValue.Depth = 1;
        
        
        renderContext.BeginRendering(pass);
        renderContext.Enable(EnableCap.DepthTest);
        renderContext.Enable(EnableCap.CullFace);
        renderContext.DepthFunction(DepthFunction.Less);
        renderContext.DepthRange(-1, 1);
        
        GL.CullFace(TriangleFace.Front);

        foreach (var layer in renderLayers.Values)
        {
            if(!layer.RenderLayer.ShadowCasting)
                continue;
            
            RenderLayerDraw draw = new RenderLayerDraw
            {
                RenderMode = RenderMode.ShadowRendering,
                PipelineData = pipelineData,
                ShadowIndex = bufferID,
                Layer = layer
            };

            DrawRenderLayer(draw);
        }
        
        GL.CullFace(TriangleFace.Back);
        
        renderContext.EndRendering();
    }

    private void DrawRenderLayer(RenderLayerDraw draw)
    {
        foreach (var render in draw.Layer.MeshRenders)
        {

            render.Mesh.LockUpdates();

            GpuBuffer<Vertex>? vertexBuffer = render.Mesh.GetVertexBuffer();
            GpuBuffer<uint>? indexBuffer = render.Mesh.GetIndexBuffer();

            if (render.PerformSkeletonDeformation)
                vertexBuffer = render.DeformationCache;

            if (vertexBuffer == null || indexBuffer == null)
            {
                render.Mesh.UnlockUpdates();
                continue;
            }


            render.Material.BindShader();

            if (draw.RenderMode == RenderMode.ShadowRendering)
            {
                render.Material.BindShadowPassCamera(draw.PipelineData.ShadowCameraSettings, render.ModelMatrix);
                render.Material.Shader.Uniform1i("ui_shadow_index", 1 << draw.ShadowIndex);
            } else if (draw.RenderMode == RenderMode.FinalPass)
            {
                bool shadows = draw.PipelineData.EnableShadows && draw.Layer.RenderLayer.ShadowReceiver;
                
                render.Material.BindCamera(draw.PipelineData.CameraSettings, render.ModelMatrix);
                render.Material.BindShadows(draw.PipelineData.ShadowCameraSettings, shadowBuffers);
                render.Material.Shader.Uniform1i("ub_shadows", shadows ? 1 : 0);
                render.Material.Shader.Uniform1i("ui_shadow_index", 1);
                render.Material.Shader.Uniform1i("ub_shaded", 1);
                
            } else if (draw.RenderMode == RenderMode.DeferredPass)
            {
                render.Material.BindCamera(draw.PipelineData.CameraSettings, render.ModelMatrix);
                render.Material.Shader.Uniform1i("ui_shadow_index", 1);
                render.Material.Shader.Uniform1i("ub_shaded", 0);
            }
            
            render.Material.BindUniforms();
            
            render.Material.Shader.BindBuffer("ib_vertex_buffer", vertexBuffer, 0, vertexBuffer.Length);
            
            Mesh.Draw(vertexBuffer, indexBuffer, indexBuffer.Length, vertexArray);

            render.Mesh.UnlockUpdates();
        }
    }


    private class RenderLayerData
    {
        public readonly List<MeshRenderProperties> MeshRenders = new List<MeshRenderProperties>();
        public readonly List<DrawListData> DrawLists = new List<DrawListData>();

        public readonly RenderLayer RenderLayer;

        public RenderLayerData(RenderLayer renderLayer)
        {
            RenderLayer = renderLayer;
        }
    }
    
    private struct RenderLayerDraw
    {
        public PipelineData PipelineData;
        public RenderLayerData Layer;
        public RenderMode RenderMode;

        public int ShadowIndex;
    }
    
    private enum RenderMode
    {
        ShadowRendering,
        DeferredPass,
        FinalPass
    }
}