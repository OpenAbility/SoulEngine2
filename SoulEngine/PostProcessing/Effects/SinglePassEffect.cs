using OpenTK.Graphics.OpenGL;
using SoulEngine.Rendering;
using SoulEngine.Resources;
using SoulEngine.UI.Rendering;

namespace SoulEngine.PostProcessing.Effects;

public abstract class SinglePassEffect : PostEffect
{
    private readonly Shader shader;

    private Framebuffer target;
    
    public SinglePassEffect(int priority, string shaderPath) : base(0)
    {
        shader = ResourceManager.Global.Load<Shader>(shaderPath);
    }

    public override void PerformEffect(PostProcessedSurface surface)
    {

        if (surface.RebuildFrameResources)
        {
            target = new Framebuffer(surface.Game, surface.FramebufferSize);
        }
        
        target.BindFramebuffer();
        
        
        shader.Bind();
        surface.Framebuffer.BindColour(0);
        shader.Uniform1i("ut_colour0", 0);
        
        surface.Framebuffer.BindDepth(1);
        shader.Uniform1i("ut_depth", 1);
        
        DrawQuad();
        
        surface.MarkLatest(target);
    }
}