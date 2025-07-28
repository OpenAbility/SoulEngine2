using SoulEngine.Core;
using SoulEngine.Rendering;
using SoulEngine.Resources;

namespace SoulEngine.PostProcessing.Effects;

public class BloomEffect : PostEffect
{
    private readonly BlurPass blurPass = new BlurPass(2);

    private Framebuffer framebuffer = null!;
    private readonly Shader mix;

    public BloomEffect() : base(2)
    {
        mix = ResourceManager.Global.Load<Shader>("shader/post/bloom_mix.program");
    }


    public override void PerformEffect(PostProcessedSurface surface)
    {
        if (surface.RebuildFrameResources)
            framebuffer = new Framebuffer(surface.Game, surface.FramebufferSize);
        
        Framebuffer buffer = blurPass.Perform(surface.LastUsedBuffer, surface.Game);
        
        Game.Current.RenderContext.PushPassName("mix");
        
        framebuffer.BindFramebuffer();
        surface.LastUsedBuffer.BindColour(0);
        buffer.BindColour(1);
        mix.Bind();
        mix.Uniform1i("ut_colour0", 0);
        mix.Uniform1i("ut_blurred", 1);
        
        DrawQuad();
        
        surface.MarkLatest(framebuffer);
        
              
        Game.Current.RenderContext.PopPassName();

    }
}