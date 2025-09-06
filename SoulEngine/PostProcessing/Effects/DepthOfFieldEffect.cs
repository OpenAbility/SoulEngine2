using OpenTK.Mathematics;
using SoulEngine.Data;
using SoulEngine.Rendering;
using SoulEngine.Resources;

namespace SoulEngine.PostProcessing.Effects;

public class DepthOfFieldEffect : PostEffect
{
    private readonly BlurPass farBlur = new BlurPass(2);
    private readonly BlurPass nearBlur = new BlurPass(2);
    
    private ColourFrameBuffer farPlane;
    private ColourFrameBuffer nearPlane;
    private ColourFrameBuffer focusPlane;
    
    private readonly Shader separateDepth;
    private readonly Shader mix;
    private readonly Shader voxagon;
    
    private Framebuffer target = null!;
    
    public DepthOfFieldEffect(int priority) : base(priority)
    {
        separateDepth = ResourceManager.Global.Load<Shader>("shader/post/dof_separate.program");
        mix = ResourceManager.Global.Load<Shader>("shader/post/dof_mix.program");
        voxagon = ResourceManager.Global.Load<Shader>("shader/post/dof_voxagon.program");
    }



    private void VoxagonEffect(PostProcessedSurface surface)
    {
        surface.LastUsedBuffer.BindColour(0);
        surface.Framebuffer.BindDepth(1); // Bind original framebuffer depth (out of safety)
        
        voxagon.Uniform1i("ut_colour0", 0);
        voxagon.Uniform1i("ut_depth", 1);
        
        voxagon.Uniform1f("uf_camNear", surface.CameraSettings.NearPlane);
        voxagon.Uniform1f("uf_camFar", surface.CameraSettings.FarPlane);
        
        voxagon.Uniform1f("uf_focus", EngineVarContext.Global.GetFloat("dof_focus", 0.90f) / surface.CameraSettings.FarPlane);
        voxagon.Uniform1f("uf_scale", EngineVarContext.Global.GetFloat("dof_scale", 1.0f));
        voxagon.Uniform2f("uf_pixelSize", new Vector2(1f / surface.FramebufferSize.X, 1f / surface.FramebufferSize.Y));

        
        voxagon.Bind();
        
        target.BindFramebuffer();
        
        DrawQuad();

        surface.MarkLatest(target);

    }

    public override void PerformEffect(PostProcessedSurface surface)
    {
        if (surface.RebuildFrameResources)
        {
            farPlane = new ColourFrameBuffer(surface.Game, surface.FramebufferSize);
            nearPlane = new ColourFrameBuffer(surface.Game, surface.FramebufferSize);
            focusPlane = new ColourFrameBuffer(surface.Game, surface.FramebufferSize);

            target = new Framebuffer(surface.Game, surface.FramebufferSize);
        }
        
        if (EngineVarContext.Global.GetBool("dof_voxagon", true))
        {
            VoxagonEffect(surface);
            return;
        }
        
        

        

        surface.LastUsedBuffer.BindColour(0);
        surface.Framebuffer.BindDepth(1); // Bind original framebuffer depth (out of safety)
        
        separateDepth.Uniform1i("ut_colour0", 0);
        separateDepth.Uniform1i("ut_depth", 1);
        
        separateDepth.Uniform1f("uf_knee", EngineVarContext.Global.GetFloat("dof_knee", 0.01f));
        
        separateDepth.Bind();
        
        // Three draws: far, near and middle.
        
        // First is far
        farPlane.BindFramebuffer();
        separateDepth.Uniform1f("uf_depthMin", EngineVarContext.Global.GetFloat("dof_far", 0.99f));
        separateDepth.Uniform1f("uf_depthMax", 2.0f); // 2 because that'll NEVER hit
        DrawQuad();
        
        // Then near
        nearPlane.BindFramebuffer();
        separateDepth.Uniform1f("uf_depthMin", -1); // -1 because, once again, that will never hit
        separateDepth.Uniform1f("uf_depthMax", EngineVarContext.Global.GetFloat("dof_near", 0.90f));
        DrawQuad();
        
        // Finally middle (this one is from 0-far plane, just works better that way)
        focusPlane.BindFramebuffer();
        separateDepth.Uniform1f("uf_depthMin", -1);
        separateDepth.Uniform1f("uf_depthMax", EngineVarContext.Global.GetFloat("dof_far"));
        DrawQuad();
        
        // then we'll blur the different "planes"
        Framebuffer farField = farBlur.Perform(farPlane, surface.Game);
        Framebuffer nearField = nearBlur.Perform(nearPlane, surface.Game);
        
        // finally, we'll do the mixing
        
        target.BindFramebuffer();
        
        mix.Bind();
        farField.BindColour(0);
        nearField.BindColour(1);
        focusPlane.BindColour(2);

        mix.Uniform1i("ut_far", 0);
        mix.Uniform1i("ut_near", 1);
        mix.Uniform1i("ut_focus", 2);
        
        DrawQuad();
        
        surface.MarkLatest(target);
        
    }
}