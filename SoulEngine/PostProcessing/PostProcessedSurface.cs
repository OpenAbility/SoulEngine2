using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Rendering;

namespace SoulEngine.PostProcessing;

public class PostProcessedSurface : IRenderSurface
{
    public readonly Framebuffer Framebuffer;
    public readonly PostProcessor PostProcessor;
    public readonly IRenderSurface UnderlyingSurface;

    public bool RebuildFrameResources => frameCount <= 1;
    private int frameCount;

    public readonly Game Game;
    
    public PostProcessedSurface(IRenderSurface underlying, PostProcessor postProcessor, Game game, Vector2i size)
    {
        PostProcessor = postProcessor;
        Framebuffer = new Framebuffer(game, size);
        UnderlyingSurface = underlying;

        LastUsedBuffer = Framebuffer;
        this.Game = game;
    }
    
    public void BindFramebuffer() => Framebuffer.BindFramebuffer();
    public Vector2i FramebufferSize => Framebuffer.FramebufferSize;
    public int GetSurfaceHandle() => Framebuffer.GetSurfaceHandle();

    public Framebuffer LastUsedBuffer { get; private set; }
    

    internal void InitializeFrame()
    {
        frameCount++;
        LastUsedBuffer = Framebuffer;
    }
    

    public void MarkLatest(Framebuffer framebuffer)
    {
        LastUsedBuffer = framebuffer;
    }
}