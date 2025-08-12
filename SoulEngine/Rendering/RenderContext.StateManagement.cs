using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using OpenAbility.Logging;
using OpenTK.Graphics.OpenGL;
using Buffer = OpenTK.Graphics.OpenGL.Buffer;

namespace SoulEngine.Rendering;

public partial class RenderContext
{
    private static readonly EnableCap[] ValidCaps =
    [
        EnableCap.LineSmooth,
        EnableCap.PolygonSmooth,
        EnableCap.CullFace,
        EnableCap.DepthTest,
        EnableCap.StencilTest,
        EnableCap.Dither,
        EnableCap.Blend,
        EnableCap.ColorLogicOp,
        EnableCap.ScissorTest,
        EnableCap.PolygonOffsetPoint,
        EnableCap.PolygonOffsetLine,
        EnableCap.ClipDistance0,
        EnableCap.ClipDistance1,
        EnableCap.ClipDistance2,
        EnableCap.ClipDistance3,
        EnableCap.ClipDistance4,
        EnableCap.ClipDistance5,
        EnableCap.ClipDistance6,
        EnableCap.ClipDistance7,
        EnableCap.PolygonOffsetFill,
        EnableCap.Multisample,
        EnableCap.MultisampleSgis,
        EnableCap.SampleAlphaToCoverage,
        EnableCap.SampleAlphaToMaskSgis,
        EnableCap.SampleAlphaToOne,
        EnableCap.SampleAlphaToOneSgis,
        EnableCap.SampleCoverage,
        EnableCap.DebugOutputSynchronous,
        EnableCap.ProgramPointSize,
        EnableCap.DepthClamp,
        EnableCap.TextureCubeMapSeamless,
        EnableCap.SampleShading,
        EnableCap.RasterizerDiscard,
        EnableCap.PrimitiveRestartFixedIndex,
        EnableCap.FramebufferSrgb,
        EnableCap.SampleMask,
        EnableCap.PrimitiveRestart,
        EnableCap.DebugOutput,
    ];

    private Dictionary<EnableCap, bool> enabled = new Dictionary<EnableCap, bool>();

    private Stack<RenderPass> renderPassStack = new Stack<RenderPass>();
    private uint currentGroupID = 0;
    private DepthFunction depthFunction;

    public void Enable(EnableCap cap)
    {
        if(enabled[cap])
            return;
        enabled[cap] = true;
        GL.Enable(cap);
    }
    
    public void Disable(EnableCap cap)
    {
        if(!enabled[cap])
            return;
        enabled[cap] = false;
        GL.Disable(cap);
    }

    public void RebuildState()
    {
        enabled.Clear();
        foreach (var cap in ValidCaps)
        {
            if(enabled.ContainsKey(cap) && enabled[cap] != GL.IsEnabled(cap))
                Logger.Get<RenderContext>().Warning("Enable cap " + enabled[cap] + " was changed outside of RenderContext!");
            enabled[cap] = GL.IsEnabled(cap);
        }

        currentSurface = GL.GetInteger(GetPName.DrawFramebufferBinding);
        depthFunction = (DepthFunction)GL.GetInteger(GetPName.DepthFunc);
    }
    
    private int currentSurface;

    private void PushPassState(RenderPass pass)
    {
        int fbo = pass.Surface.GetSurfaceHandle();
        
        currentSurface = fbo;
        pass.Surface.BindFramebuffer();
        
        
       
        if (pass.DepthStencilSettings.LoadOp == AttachmentLoadOp.Clear)
        {
            GL.ClearDepthf(pass.DepthStencilSettings.ClearValue.Depth);
            GL.ClearStencil(pass.DepthStencilSettings.ClearValue.Stencil);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            //GL.ClearNamedFramebufferfi(fbo, Buffer.Stenci, 0, pass.DepthStencilSettings.ClearValue.Depth, pass.DepthStencilSettings.ClearValue.Stencil);
        }
        else if (pass.DepthStencilSettings.LoadOp == AttachmentLoadOp.DontCare)
        {
            GL.InvalidateNamedFramebufferData(fbo, 1, [FramebufferAttachment.DepthStencilAttachment]);
        }

        if (pass.DepthStencilSettings.StoreOp == AttachmentStoreOp.Store)
        {
            GL.DepthMask(true);
        } else if (pass.DepthStencilSettings.StoreOp == AttachmentStoreOp.Store)
        {
            GL.DepthMask(false);
        }
        


        Span<float> cBuf = stackalloc float[4];
        
        for (int i = 0; i < pass.ColorSettings.Length; i++)
        {
            if (pass.ColorSettings[i].LoadOp == AttachmentLoadOp.Clear)
            {
                
                cBuf[0] = pass.ColorSettings[i].ClearValue.Colour.R;
                cBuf[1] = pass.ColorSettings[i].ClearValue.Colour.G;
                cBuf[2] = pass.ColorSettings[i].ClearValue.Colour.B;
                cBuf[3] = pass.ColorSettings[i].ClearValue.Colour.A;
                
                GL.ClearNamedFramebufferf(fbo, Buffer.Color, i, cBuf);
            }
            else if (pass.ColorSettings[i].LoadOp == AttachmentLoadOp.DontCare)
            {
                GL.InvalidateNamedFramebufferData(fbo, 1, [(FramebufferAttachment)((uint)FramebufferAttachment.ColorAttachment0 + i)]);
            }
            
            
            if (pass.ColorSettings[i].StoreOp == AttachmentStoreOp.Store)
            {
                GL.ColorMaski((uint)i, true, true, true, true);
            }
            else if (pass.ColorSettings[i].StoreOp == AttachmentStoreOp.DontStore)
            {
                GL.ColorMaski((uint)i, false, false, false, false);
            }
        }

    }

    public void BeginRendering(RenderPass pass)
    {
        //RebuildState();
        if(pass.Name != null)
            PushPassName(pass.Name);
        
        renderPassStack.Push(pass);
        PushPassState(pass);
    }

    public void EndRendering()
    {
        //RebuildState();
        if(renderPassStack.Peek().Name != null)
            GL.PopDebugGroup();
        
        renderPassStack.Pop();
        if(renderPassStack.Count != 0)
            PushPassState(renderPassStack.Peek());
    }


    public void DepthFunction(DepthFunction depthFunction)
    {
        if(depthFunction != this.depthFunction)
            GL.DepthFunc(depthFunction);
        this.depthFunction = depthFunction;
    }
    
    public void PushPassName(string name)
    {
#if DEBUG || DEVELOPMENT
        GL.PushDebugGroup(DebugSource.DebugSourceApplication, currentGroupID++, name.Length, name);
#endif
    }

    public void PopPassName()
    {
#if DEBUG || DEVELOPMENT
        GL.PopDebugGroup();
#endif
    }

    public void PresentDisplay(Framebuffer framebuffer, Rectangle src, Rectangle dst)
    {
        GL.BlitNamedFramebuffer(framebuffer.Handle, 0, src.X, src.Y, src.X + src.Width, src.Y + src.Height, dst.X, dst.Y, dst.X + dst.Width, dst.Y + dst.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
    }

    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureRendering()
    {
        if (renderPassStack.Count <= 0)
            throw new Exception("Not currently rendering!");
    }
    
    public void BindShader(Shader shader)
    {
        EnsureRendering();
        shader.Bind();
    }

    public void DepthRange(float from, float to)
    {
        GL.DepthRangef(from, to);
    }

    public void DepthBias(float factor, float units)
    {
        GL.PolygonOffset(factor, units);
    }

    public void BlendFunction(BlendingFactor sfactor, BlendingFactor dfactor)
    {
        GL.BlendFunc(sfactor, dfactor);
    }
    
    public void BlendFunction(uint index, BlendingFactor sfactor, BlendingFactor dfactor)
    {
        GL.BlendFunci(index, sfactor, dfactor);
    }
    
    
}