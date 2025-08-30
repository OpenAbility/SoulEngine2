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

    private readonly List<LightSubmitInformation> lights = new List<LightSubmitInformation>();

    private GpuBuffer<Matrix4> skeletonBuffer;

    public const BufferStorageMask SkeletonBufferParameters =
        BufferStorageMask.MapWriteBit | BufferStorageMask.DynamicStorageBit;

    public DefaultRenderPipeline()
    {
        vertexArray = new Vertex().CreateVertexArray();

        skeletonBuffer =
            new GpuBuffer<Matrix4>(EngineVarContext.Global.GetInt("e_buf_skele", 128), SkeletonBufferParameters);

        //int shadowResolution = EngineVarContext.Global.GetInt("e_shadow_res", 2048);

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
        if (renderProperties.Material == null!)
            renderProperties.Material = defaultMaterial;


        renderLayers[renderLayer].MeshRenders.Add(renderProperties);
    }

    public void SubmitDrawList(RenderLayer renderLayer, DrawListData drawListData)
    {
        if (drawListData.Material == null!)
            drawListData.Material = defaultMaterial;

        renderLayers[renderLayer].DrawLists.Add(drawListData);
    }


    public void SubmitLightDraw(in LightSubmitInformation light)
    {
        lights.Add(light);
    }

    public IEnumerable<RenderLayer> GetLayers()
    {
        yield return DefaultRenderLayers.OpaqueLayer;
    }
    
    public void DrawFrame(in PipelineData pipelineData)
    {
        

        using var profilerPass = Profiler.Instance.Segment("renderer");

        PostProcessedSurface? postProcessableSurface = null;
        PostProcessor? postProcessor = null;

        Framebuffer? targetFramebuffer = pipelineData.TargetSurface as Framebuffer;

        if (pipelineData.PostProcessing)
        {
            if (!postProcessors.TryGetValue(pipelineData.TargetSurface, out postProcessor))
            {
                postProcessor = new PostProcessor(pipelineData.Game, pipelineData.TargetSurface);
                postProcessors[pipelineData.TargetSurface] = postProcessor;
            }

            postProcessableSurface = postProcessor.InitializeFrameSurface();
            targetFramebuffer = postProcessableSurface.Framebuffer;
        }

        if (targetFramebuffer == null)
            return;

        ShaderBinder shaderBinder = new ShaderBinder();


        RenderContext renderContext = pipelineData.RenderContext;

        SkinningPass();


        #region SHADOW RENDER

        if (pipelineData is { EnableShadows: true, RedrawShadows: true })
        {
            foreach (var light in lights)
            {
                if(light.ShadowBuffer == null)
                    continue;
                
                for (int i = 0; i < light.ShadowMaps.Length; i++)
                {
                    ShadowBufferPass(shaderBinder, light.ShadowBuffer.ShadowLevels[i], light.ShadowMaps[i], pipelineData);
                }
            }

        }

        #endregion
        
        GL.TextureBarrier();
        
        #region G PASS

        {
            RenderPass gPass = new RenderPass
            {
                Name = "G-Buffer Pass",
                Surface = targetFramebuffer,
            };
            gPass.DepthStencilSettings.LoadOp = AttachmentLoadOp.Clear;
            gPass.DepthStencilSettings.StoreOp = AttachmentStoreOp.Store;
            gPass.DepthStencilSettings.ClearValue.Depth = 1;

            gPass.ColorSettings =
            [
                new FramebufferAttachmentSettings()
                {
                    LoadOp = AttachmentLoadOp.Clear,
                    StoreOp = AttachmentStoreOp.Store,
                    ClearValue = new FramebufferClearValue()
                    {
                        Colour = Colour.LightSkyBlue
                    }
                },
                new FramebufferAttachmentSettings()
                {
                    LoadOp = AttachmentLoadOp.Clear,
                    StoreOp = AttachmentStoreOp.Store,
                    ClearValue = new FramebufferClearValue()
                    {
                        Colour = Colour.Blank
                    }
                },
                new FramebufferAttachmentSettings()
                {
                    LoadOp = AttachmentLoadOp.Clear,
                    StoreOp = AttachmentStoreOp.DontStore,
                    ClearValue = new FramebufferClearValue()
                    {
                        Colour = pipelineData.AmbientLight
                    }
                }
            ];


            renderContext.BeginRendering(gPass);

            renderContext.Enable(EnableCap.DepthTest);
            renderContext.Enable(EnableCap.CullFace);
            renderContext.DepthFunction(DepthFunction.Lequal);
            renderContext.Disable(EnableCap.Blend);
            renderContext.DepthRange(0, 1);

            foreach (var layer in renderLayers.Values)
            {
                if (!layer.RenderLayer.DualPass)
                    continue;
                RenderLayerDraw draw = new RenderLayerDraw();
                draw.PipelineData = pipelineData;
                draw.RenderMode = RenderMode.DeferredPass;
                draw.Layer = layer;
                draw.Target = targetFramebuffer;

                DrawRenderLayer(shaderBinder, draw);
            }

            renderContext.EndRendering();
        }


        #endregion
        
        GL.TextureBarrier();
        
        #region LIGHT RENDER

        // Light source rendering
        {

            RenderPass lightPass = new RenderPass
            {
                Name = "Light Pass",
                Surface = targetFramebuffer,
            };
            lightPass.DepthStencilSettings.LoadOp = AttachmentLoadOp.Load;
            lightPass.DepthStencilSettings.StoreOp = AttachmentStoreOp.DontStore;

            lightPass.ColorSettings =
            [
                new FramebufferAttachmentSettings()
                {
                    LoadOp = AttachmentLoadOp.Load,
                    StoreOp = AttachmentStoreOp.DontStore
                },
                new FramebufferAttachmentSettings()
                {
                    LoadOp = AttachmentLoadOp.Load,
                    StoreOp = AttachmentStoreOp.DontStore
                },
                new FramebufferAttachmentSettings()
                {
                    LoadOp = AttachmentLoadOp.Clear,
                    ClearValue = new FramebufferClearValue()
                    {
                        Colour = pipelineData.AmbientLight
                    },
                    StoreOp = AttachmentStoreOp.Store
                }
            ];


            renderContext.BeginRendering(lightPass);

            renderContext.Disable(EnableCap.DepthTest);
            renderContext.Enable(EnableCap.CullFace);
            GL.CullFace(TriangleFace.Front);
            renderContext.Enable(EnableCap.Blend);

            renderContext.BlendFunction(2u, BlendingFactor.SrcAlpha, BlendingFactor.One);

            foreach (var light in lights)
            {
                light.LightMesh.LockUpdates();


                var vertexBuffer = light.LightMesh.GetVertexBuffer();
                var indexBuffer = light.LightMesh.GetIndexBuffer();

                if (vertexBuffer == null || indexBuffer == null)
                {
                    light.LightMesh.UnlockUpdates();
                    continue;
                }

                shaderBinder.ClearTextures();
                shaderBinder.BindShader(light.LightShader);

                shaderBinder.BindUniform("um_model", light.ModelMatrix, false);
                shaderBinder.BindUniform("um_view", pipelineData.CameraSettings.ViewMatrix, false);
                shaderBinder.BindUniform("um_projection", pipelineData.CameraSettings.ProjectionMatrix, false);

                shaderBinder.BindTexture("ut_colour", targetFramebuffer.ColourBuffer);
                shaderBinder.BindTexture("ut_depth", targetFramebuffer.DepthBuffer);
                shaderBinder.BindTexture("ut_normal", targetFramebuffer.NormalBuffer);

                shaderBinder.BindUniform("ub_shadows", pipelineData.EnableShadows ? 1 : 0);

                if (pipelineData.EnableShadows && light.ShadowBuffer != null)
                {                        
                    shaderBinder.BindTexture("ut_shadow_buffer", light.ShadowBuffer.TextureHandle);
                    
                    for (uint i = 0; i < light.ShadowMaps.Length; i++)
                    {

                        
                        shaderBinder.BindUniform("um_shadow_projections[0]#" + i, light.ShadowMaps[i].ProjectionMatrix,
                            false);
                        shaderBinder.BindUniform("um_shadow_views[0]#" + i, light.ShadowMaps[i].ViewMatrix, false);
                        
                    }
                    

                }
                


                shaderBinder.BindUniform("uv_direction", light.Direction);
                shaderBinder.BindUniform("uv_position", light.Position);

                shaderBinder.BindUniform("um_inv_cam",
                    pipelineData.CameraSettings.ProjectionMatrix.Inverted() *
                    pipelineData.CameraSettings.ViewMatrix.Inverted(), false);
                

                
                shaderBinder.BindUniform("ut_shadow_buffer_count", light.ShadowMaps.Length);

                light.ShaderBind?.Invoke(light);

                shaderBinder.BindBuffer("ib_vertex_buffer", vertexBuffer, 0, vertexBuffer.Length);
                Mesh.Draw(vertexBuffer, indexBuffer, indexBuffer.Length, vertexArray);


                light.LightMesh.UnlockUpdates();
            }

            renderContext.EndRendering();

            GL.CullFace(TriangleFace.Back);
        }

        #endregion
        
        GL.TextureBarrier();
        
        #region FINAL PASS

        {
            RenderPass finalPass = new RenderPass
            {
                Name = "Final Render Pass",
                Surface =
                    pipelineData.PostProcessing ? postProcessableSurface!.Framebuffer : pipelineData.TargetSurface,
            };
            finalPass.DepthStencilSettings.LoadOp = AttachmentLoadOp.Load;
            finalPass.DepthStencilSettings.StoreOp = AttachmentStoreOp.Store;
            finalPass.DepthStencilSettings.ClearValue.Depth = 1;

            finalPass.ColorSettings =
            [
                new FramebufferAttachmentSettings()
                {
                    LoadOp = AttachmentLoadOp.Load,
                    StoreOp = AttachmentStoreOp.Store
                },
                new FramebufferAttachmentSettings()
                {
                    LoadOp = AttachmentLoadOp.Load,
                    StoreOp = AttachmentStoreOp.DontStore
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
            renderContext.DepthRange(0, 1);
            renderContext.Disable(EnableCap.Blend);


            foreach (var layer in renderLayers.Values)
            {
                RenderLayerDraw draw = new RenderLayerDraw();
                draw.PipelineData = pipelineData;
                draw.RenderMode = RenderMode.FinalPass;
                draw.Layer = layer;
                draw.Target = targetFramebuffer;

                DrawRenderLayer(shaderBinder, draw);
            }

            renderContext.EndRendering();
        }

        #endregion
        
        GL.TextureBarrier();
        
        if (pipelineData.PostProcessing)
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
                    StoreOp = AttachmentStoreOp.Store
                }
            ];


            renderContext.BeginRendering(gizmoPass);

            renderContext.Disable(EnableCap.DepthTest);
            renderContext.Disable(EnableCap.CullFace);
            renderContext.Enable(EnableCap.Blend);
            //renderContext.Enable(EnableCap.FramebufferSrgb);
            renderContext.DepthRange(0, 1);
            renderContext.BlendFunction(0u, BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

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
                StoreOp = AttachmentStoreOp.Store
            }
        ];


        renderContext.BeginRendering(uiPass);

        renderContext.Disable(EnableCap.DepthTest);
        renderContext.Disable(EnableCap.CullFace);
        renderContext.DepthFunction(DepthFunction.Always);
        renderContext.Enable(EnableCap.Blend);
        renderContext.DepthRange(0, 1);

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

        lights.Clear();

        if (EngineVarContext.Global.GetBool("e_gl_wait", true))
            GL.Finish();
    }

    private void SkinningPass()
    {
        foreach (var layer in renderLayers.Values)
        {
            if (!layer.RenderLayer.Skeletal)
                continue;
            foreach (var render in layer.MeshRenders)
            {
                CalculateSkinning(render);
            }
        }
    }

    private void CalculateSkinning(in MeshRenderProperties render)
    {
        if (render.SkeletonBuffer == null || render.DeformationCache == null || render.Mesh == null!)
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


        BufferMapping<Matrix4> mapping = skeletonBuffer.Map(0, render.SkeletonBufferSize,
            MapBufferAccessMask.MapWriteBit | MapBufferAccessMask.MapInvalidateRangeBit);

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


    private void ShadowBufferPass(ShaderBinder binder, CSMShadowBuffer.ShadowLevel levelBuffer, ShadowLevelInformation shadowLevel, in PipelineData pipelineData)
    {
        RenderContext renderContext = pipelineData.RenderContext;

        RenderPass pass = new RenderPass
        {
            Name = "Shadow Pass",
            Surface = levelBuffer,
        };
        pass.DepthStencilSettings.LoadOp = AttachmentLoadOp.Clear;
        pass.DepthStencilSettings.StoreOp = AttachmentStoreOp.Store;
        pass.DepthStencilSettings.ClearValue.Depth = 1;



        renderContext.BeginRendering(pass);
        renderContext.Enable(EnableCap.DepthTest);
        renderContext.Enable(EnableCap.CullFace);
        renderContext.DepthFunction(DepthFunction.Less);
        renderContext.DepthRange(0, 1);

        GL.CullFace(TriangleFace.Front);

        foreach (var layer in renderLayers.Values)
        {
            if (!layer.RenderLayer.ShadowCasting)
                continue;
            
            

            RenderLayerDraw draw = new RenderLayerDraw
            {
                RenderMode = RenderMode.ShadowRendering,
                PipelineData = pipelineData,
                Layer = layer,
                ShadowLevel =  shadowLevel
            };

            DrawRenderLayer(binder, draw);
        }

        GL.CullFace(TriangleFace.Back);

        renderContext.EndRendering();
    }

    private void DrawRenderLayer(ShaderBinder binder, in RenderLayerDraw draw)
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

            binder.ClearTextures();
            binder.BindShader(render.Material.Shader);

            // Bind uniforms first, because of overlap - needs to be fixed lol
            render.Material.BindUniforms(binder);
            
            if (draw.RenderMode == RenderMode.ShadowRendering)
            {
                render.Material.BindShadowPassCamera(binder, draw.ShadowLevel.ViewMatrix, draw.ShadowLevel.ProjectionMatrix,
                    render.ModelMatrix);
            }
            else if (draw.RenderMode == RenderMode.FinalPass)
            {
                render.Material.BindCamera(binder, draw.PipelineData.CameraSettings, render.ModelMatrix);
                binder.BindUniform("ub_shaded", 1);

                binder.BindTexture("ut_lightBuffer", draw.Target.LightBuffer);
                binder.BindTexture("ut_normalBuffer", draw.Target.NormalBuffer);
                binder.BindTexture("ut_lightBuffer", draw.Target.LightBuffer);

            }
            else if (draw.RenderMode == RenderMode.DeferredPass)
            {
                render.Material.BindCamera(binder, draw.PipelineData.CameraSettings, render.ModelMatrix);
                binder.BindUniform("ub_shaded", 0);
            }

            binder.BindBuffer("ib_vertex_buffer", vertexBuffer, 0, vertexBuffer.Length);

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
        public ShadowLevelInformation ShadowLevel;



        public Framebuffer Target;
    }

    private enum RenderMode
    {
        ShadowRendering,
        DeferredPass,
        FinalPass
    }
}