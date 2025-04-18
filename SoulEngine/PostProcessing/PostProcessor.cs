using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.PostProcessing.Effects;
using SoulEngine.Rendering;
using SoulEngine.Resources;
using SoulEngine.UI.Rendering;

namespace SoulEngine.PostProcessing;

public class PostProcessor
{
    private readonly Dictionary<IRenderSurface, PostProcessedSurface?> framebuffers =
        new Dictionary<IRenderSurface, PostProcessedSurface?>();

    private readonly SortedSet<PostEffect> effects = new SortedSet<PostEffect>();

    private readonly Game game;

    public PostProcessor(Game game)
    {
        this.game = game;
        
        //EnableEffect(new BloomEffect());
        EnableEffect(new ColourEffects());
   
    }

    public void RegisterSurface(IRenderSurface renderSurface)
    {
        framebuffers.TryAdd(renderSurface, null);
    }

    public void DeregisterSurface(IRenderSurface renderSurface)
    {
        framebuffers.Remove(renderSurface);
    }

    public void EnableEffect(PostEffect effect)
    {
        effects.Add(effect);
    }

    public void DisableEffect(PostEffect effect)
    {
        effects.Remove(effect);
    }
    
    public PostProcessedSurface InitializeFrameSurface(IRenderSurface source)
    {
        PostProcessedSurface? framebuffer = framebuffers[source];
        if (framebuffer == null || framebuffer.FramebufferSize != source.FramebufferSize)
        {
            framebuffer = new PostProcessedSurface(source, this, game, source.FramebufferSize);
            framebuffers[source] = framebuffer;
        }
        
        framebuffer.InitializeFrame();
        
        return framebuffer;
    }

    public void FinishedDrawing(RenderContext renderContext, PostProcessedSurface surface)
    {
        renderContext.PushPassName("post");
        renderContext.Disable(EnableCap.DepthTest);
        renderContext.Disable(EnableCap.CullFace);
        
        foreach (var effect in effects)
        {
            renderContext.PushPassName(effect.GetType().Name);
            effect.PerformEffect(surface);
            renderContext.PopPassName();
        }
        
        surface.UnderlyingSurface.BindFramebuffer();

        Shader shader = ResourceManager.Global.Load<Shader>("shader/ui.program");
        shader.Bind();
        shader.Matrix("um_projection", Matrix4.CreateOrthographicOffCenter(0, 1, 0, 1, 0, 100), false);
        surface.LastUsedBuffer.BindColour(0);

        DrawList drawList = new DrawList(PrimitiveType.TriangleFan);

        drawList.Vertex(0, 0, 0, 0, Colour.White)
            .Vertex(0, 1, 0, 1, Colour.White)
            .Vertex(1, 1, 1, 1, Colour.White)
            .Vertex(1, 0, 1, 0, Colour.White);
        
        drawList.Submit();
        
        /*
        GL.BlitNamedFramebuffer(surface.LastUsedBuffer.Handle, surface.UnderlyingSurface.GetSurfaceHandle(), 
            0, 0, surface.FramebufferSize.X, surface.FramebufferSize.Y,
            0, 0, surface.FramebufferSize.X, surface.FramebufferSize.Y,
            ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit, 
            BlitFramebufferFilter.Nearest);
        */
        renderContext.PopPassName();
    }
    
    
}