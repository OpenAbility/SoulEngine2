using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Core;

namespace SoulEngine.Rendering;

/// <summary>
/// A default framebuffer
/// </summary>
public class Framebuffer
{
    private readonly Game game;
    public readonly int Handle;

    public readonly int ColourBuffer;
    private readonly int depthBuffer;
    
    public readonly Vector2i Size;
    
    public Framebuffer(Game game, Vector2i size)
    {
        if (size.X < 1)
            size.X = 1;
        if (size.Y < 1)
            size.Y = 1;
        
        this.game = game;

        Handle = GL.CreateFramebuffer();
        Size = size;

        ColourBuffer = GL.CreateTexture(TextureTarget.Texture2d);
        depthBuffer = GL.CreateRenderbuffer();
        
        GL.TextureStorage2D(ColourBuffer, 1, SizedInternalFormat.Rgb8, size.X, size.Y);
        GL.NamedRenderbufferStorage(depthBuffer, InternalFormat.Depth24Stencil8, size.X, size.Y);

        GL.NamedFramebufferTexture(Handle, FramebufferAttachment.ColorAttachment0, ColourBuffer, 0);
        GL.NamedFramebufferRenderbuffer(Handle, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);

        GL.NamedFramebufferDrawBuffer(Handle, ColorBuffer.ColorAttachment0);
        GL.NamedFramebufferReadBuffer(Handle, ColorBuffer.ColorAttachment0);
        
        var status = GL.CheckNamedFramebufferStatus(Handle, FramebufferTarget.Framebuffer);
        if (status != FramebufferStatus.FramebufferComplete)
            throw new Exception("FBO Error: " + status);
    }
    
    
    ~Framebuffer()
    {
        game?.ThreadSafety.EnsureMain(() =>
        {
            if(Handle != -1)
                GL.DeleteFramebuffer(Handle);
        });

    }
}