using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Core;

namespace SoulEngine.Rendering;

/// <summary>
/// A default framebuffer
/// </summary>
public class Framebuffer : EngineObject, IRenderSurface
{
    private readonly Game game;
    public readonly int Handle;

    public readonly int ColourBuffer;
    public readonly int NormalBuffer;
    public readonly int DepthBuffer;
    
    public Vector2i FramebufferSize { get; private set; }
    
    public Framebuffer(Game game, Vector2i size)
    {
        if (size.X < 1)
            size.X = 1;
        if (size.Y < 1)
            size.Y = 1;
        
        this.game = game;

        Handle = GL.CreateFramebuffer();
        FramebufferSize = size;

        ColourBuffer = GL.CreateTexture(TextureTarget.Texture2d);
        DepthBuffer = GL.CreateTexture(TextureTarget.Texture2d);
        NormalBuffer = GL.CreateTexture(TextureTarget.Texture2d);
        
        GL.TextureStorage2D(ColourBuffer, 1, SizedInternalFormat.Rgb16f, size.X, size.Y);
        GL.TextureStorage2D(NormalBuffer, 1, SizedInternalFormat.Rgb8, size.X, size.Y);
        GL.TextureStorage2D(DepthBuffer, 1, SizedInternalFormat.Depth24Stencil8, size.X, size.Y);

        GL.TextureParameteri(ColourBuffer, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        GL.TextureParameteri(ColourBuffer, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TextureParameteri(ColourBuffer, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        
        GL.TextureParameteri(NormalBuffer, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        GL.TextureParameteri(NormalBuffer, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TextureParameteri(NormalBuffer, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        
        GL.NamedFramebufferTexture(Handle, FramebufferAttachment.ColorAttachment0, ColourBuffer, 0);
        GL.NamedFramebufferTexture(Handle, FramebufferAttachment.ColorAttachment1, NormalBuffer, 0);
        GL.NamedFramebufferTexture(Handle, FramebufferAttachment.DepthAttachment, DepthBuffer, 0);
        //GL.NamedFramebufferRenderbuffer(Handle, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);

        ColorBuffer[] drawBuffers = [
            ColorBuffer.ColorAttachment0,
            ColorBuffer.ColorAttachment1
        ];
        
        GL.NamedFramebufferDrawBuffers(Handle, drawBuffers.Length, drawBuffers);

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
            GL.DeleteTexture(ColourBuffer);
            GL.DeleteTexture(DepthBuffer);
        });

    }

    public void BindColour(uint index)
    {
        GL.BindTextureUnit(index, ColourBuffer);
    }
    
    public void BindDepth(uint index)
    {
        GL.BindTextureUnit(index, DepthBuffer);
    }

    public void BindFramebuffer()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
        GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);
    }


    public int GetSurfaceHandle()
    {
        return Handle;
    }
}