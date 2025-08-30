using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Core;

namespace SoulEngine.Rendering;

// Slot-in replacement for Framebuffer, but with only colour
public class ColourFrameBuffer : EngineObject, IColourBufferProvider
{
    private readonly Game game;
    public readonly int Handle;

    public readonly int ColourBuffer;
    
    public Vector2i FramebufferSize { get; private set; }
    
    public ColourFrameBuffer(Game game, Vector2i size)
    {
        if (size.X < 1)
            size.X = 1;
        if (size.Y < 1)
            size.Y = 1;
        
        this.game = game;

        Handle = GL.CreateFramebuffer();
        FramebufferSize = size;

        ColourBuffer = GL.CreateTexture(TextureTarget.Texture2d);
        
        GL.TextureStorage2D(ColourBuffer, 1, SizedInternalFormat.Rgba16f, size.X, size.Y);

        GL.TextureParameteri(ColourBuffer, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        GL.TextureParameteri(ColourBuffer, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TextureParameteri(ColourBuffer, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        
        GL.NamedFramebufferTexture(Handle, FramebufferAttachment.ColorAttachment0, ColourBuffer, 0);
        
        ColorBuffer[] drawBuffers = [
            ColorBuffer.ColorAttachment0,
            ColorBuffer.ColorAttachment1,
            ColorBuffer.ColorAttachment2
        ];
        
        GL.NamedFramebufferDrawBuffers(Handle, drawBuffers.Length, drawBuffers);

        GL.NamedFramebufferReadBuffer(Handle, ColorBuffer.ColorAttachment0);
        
        var status = GL.CheckNamedFramebufferStatus(Handle, FramebufferTarget.Framebuffer);
        if (status != FramebufferStatus.FramebufferComplete)
            throw new Exception("FBO Error: " + status);
    }
    
    
    ~ColourFrameBuffer()
    {
        game?.ThreadSafety.EnsureMainNonBlocking(() =>
        {
            if(Handle != -1)
                GL.DeleteFramebuffer(Handle);
            GL.DeleteTexture(ColourBuffer);
        });

    }

    public void BindColour(uint index)
    {
        GL.BindTextureUnit(index, ColourBuffer);
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

public interface IColourBufferProvider : IRenderSurface
{
    public void BindColour(uint index);
}