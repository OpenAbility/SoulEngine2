using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Util;

namespace SoulEngine.Rendering;

/// <summary>
/// A default framebuffer
/// </summary>
public class Depthbuffer : EngineObject, IRenderSurface
{
    public readonly int Handle;
    
    public readonly int DepthBuffer;
    
    public Vector2i FramebufferSize { get; private set; }
    
    public Depthbuffer(Vector2i size)
    {
        if (size.X < 1)
            size.X = 1;
        if (size.Y < 1)
            size.Y = 1;
        
 
        Handle = GL.CreateFramebuffer();
        FramebufferSize = size;
        
        DepthBuffer = GL.CreateTexture(TextureTarget.Texture2d);
        
        GL.TextureStorage2D(DepthBuffer, 1, SizedInternalFormat.DepthComponent32f, size.X, size.Y);
        
        GL.TextureParameteri(DepthBuffer, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToBorder);
        GL.TextureParameteri(DepthBuffer, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        GL.TextureParameteri(DepthBuffer, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
        
        GL.TextureParameterf(DepthBuffer, TextureParameterName.TextureBorderColor, [1.0f, 1.0f, 1.0f, 1.0f]);
        
        GL.NamedFramebufferTexture(Handle, FramebufferAttachment.DepthAttachment, DepthBuffer, 0);

        GL.NamedFramebufferDrawBuffer(Handle, ColorBuffer.None);
        GL.NamedFramebufferReadBuffer(Handle, ColorBuffer.None);
        
        var status = GL.CheckNamedFramebufferStatus(Handle, FramebufferTarget.Framebuffer);
        if (status != FramebufferStatus.FramebufferComplete)
            throw new Exception("FBO Error: " + status);
    }
    
    
    ~Depthbuffer()
    {
        ThreadSafety.Instance.EnsureMainNonBlocking(() =>
        {
            if(Handle != -1)
                GL.DeleteFramebuffer(Handle);
            GL.DeleteTexture(DepthBuffer);
        });

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