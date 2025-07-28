using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Rendering;
using SoulEngine.Resources;
using SoulEngine.UI.Rendering;

namespace SoulEngine.PostProcessing.Effects;

public class BlurPass
{
    private readonly Shader downsampler;
    private readonly Shader upsampler;

    private Vector2i size = new Vector2i(-1, -1);
    private Framebuffer[] framebuffers = null!;
    private int depth;
    
    public BlurPass(int depth)
    {
        downsampler = ResourceManager.Global.Load<Shader>("shader/post/blur_downsample.program");
        upsampler = ResourceManager.Global.Load<Shader>("shader/post/blur_upsample.program");
        this.depth = depth + 1;
    }

    private void RebuildChain(Vector2i newSize, Game game)
    {
        size = newSize;

        Vector2i mipSize = newSize;

        List<Framebuffer> buffers = new List<Framebuffer>();

        while (true)
        {
            if(mipSize.X <= 1 || mipSize.Y <= 1)
                break;
            if(buffers.Count >= depth)
                break;
            
            buffers.Add(new Framebuffer(game, mipSize));
            mipSize /= 2;
        }

        framebuffers = buffers.ToArray();
    }

    public Framebuffer Perform(Framebuffer framebuffer, Game game)
    {
        if (framebuffer.FramebufferSize != size)
        {
            RebuildChain(framebuffer.FramebufferSize, game);
        }
        
        DrawList drawList = new DrawList(PrimitiveType.TriangleFan);

        void DrawQuad()
        {
            drawList.Vertex(0, 0, 0, 0, Colour.Blank);
            drawList.Vertex(1, 0, 0, 0, Colour.Blank);
            drawList.Vertex(1, 1, 0, 0, Colour.Blank);
            drawList.Vertex(0, 1, 0, 0, Colour.Blank);
            
            drawList.Submit();
        }

        downsampler.Bind();
        framebuffer.BindColour(0);
        downsampler.Uniform1i("srcTexture", 0);
        downsampler.Uniform2f("srcResolution", framebuffer.FramebufferSize);

        for (int i = 1; i < framebuffers.Length; i++)
        {
            framebuffers[i].BindFramebuffer();
            DrawQuad();
            
            framebuffers[i].BindColour(0);
            downsampler.Uniform2f("srcResolution", framebuffers[i].FramebufferSize);
        }
        
        upsampler.Bind();
        upsampler.Uniform1i("srcTexture", 0);
        upsampler.Uniform1f("filterRadius", 0f);
        for (int i = framebuffers.Length - 2; i >= 0; i--)
        {
            framebuffers[i].BindFramebuffer();
            framebuffers[i + 1].BindColour(0);
            DrawQuad();
     
        }

        return framebuffers[0];

    }
    
}