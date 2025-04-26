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
    private IRenderSurface renderSurface;

    private readonly SortedSet<PostEffect> effects = new SortedSet<PostEffect>();

    private readonly Game game;
    private PostProcessedSurface? postProcessedSurface;

    public PostProcessor(Game game, IRenderSurface renderSurface)
    {
        this.game = game;
        
        this.renderSurface = renderSurface;
        
        //EnableEffect(new BloomEffect());
        EnableEffect(new ColourEffects());
        
    

    }
    

    public void EnableEffect(PostEffect effect)
    {
        effects.Add(effect);
    }

    public void DisableEffect(PostEffect effect)
    {
        effects.Remove(effect);
    }
    
    public PostProcessedSurface InitializeFrameSurface()
    {
        if (postProcessedSurface == null || postProcessedSurface.FramebufferSize != renderSurface.FramebufferSize)
        {
            postProcessedSurface = new PostProcessedSurface(renderSurface, this, game, renderSurface.FramebufferSize);
        }
        
        postProcessedSurface.InitializeFrame();
        return postProcessedSurface;
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