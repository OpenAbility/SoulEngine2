using OpenTK.Graphics.OpenGL;
using SoulEngine.Core;
using SoulEngine.PostProcessing.Effects;
using SoulEngine.Rendering;

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
        
        //EnableEffect(new ColourEffects());
        //EnableEffect(new BloomEffect());
    }

    public void RegisterSurface(IRenderSurface renderSurface)
    {
        framebuffers.Add(renderSurface, null);
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
        
        foreach (var effect in effects)
        {
            renderContext.PushPassName(effect.GetType().Name);
            effect.PerformEffect(surface);
            renderContext.PopPassName();
        }
        
        GL.BlitNamedFramebuffer(surface.LastUsedBuffer.Handle, surface.UnderlyingSurface.GetSurfaceHandle(), 
            0, 0, surface.FramebufferSize.X, surface.FramebufferSize.Y,
            0, 0, surface.FramebufferSize.X, surface.FramebufferSize.Y,
            ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit, 
            BlitFramebufferFilter.Nearest);
        
        renderContext.PopPassName();
    }
    
    
}