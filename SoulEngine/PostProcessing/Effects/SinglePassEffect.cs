using OpenTK.Graphics.OpenGL;
using SoulEngine.Rendering;
using SoulEngine.Resources;
using SoulEngine.UI.Rendering;

namespace SoulEngine.PostProcessing.Effects;

public abstract class SinglePassEffect : PostEffect
{
    private readonly Shader effectShader;

    private Framebuffer target = null!;
    
    public SinglePassEffect(int priority, string shaderPath) : base(0)
    {
        effectShader = ResourceManager.Global.Load<Shader>(shaderPath);
    }

    protected virtual void BindUniforms(Shader shader)
    {
        
    }

    public override void PerformEffect(PostProcessedSurface surface)
    {

        if (surface.RebuildFrameResources)
        {
            target = new Framebuffer(surface.Game, surface.FramebufferSize);
        }
        
        target.BindFramebuffer();

        BindUniforms(effectShader);
        
        effectShader.Bind();
        surface.Framebuffer.BindColour(0);
        effectShader.Uniform1i("ut_colour0", 0);
        
        surface.Framebuffer.BindDepth(1);
        effectShader.Uniform1i("ut_depth", 1);
        
        DrawQuad();
        
        surface.MarkLatest(target);
    }
}