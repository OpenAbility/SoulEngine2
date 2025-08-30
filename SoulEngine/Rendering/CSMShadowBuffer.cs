using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Util;

namespace SoulEngine.Rendering;

public class CSMShadowBuffer : EngineObject
{
    public readonly int TextureHandle;
    public readonly int[] Framebuffers;
    public readonly ShadowLevel[] ShadowLevels;

    public readonly int Resolution;
    public readonly int Levels;

    public CSMShadowBuffer(int resolution, int levels)
    {
        Resolution = resolution;
        Levels = levels;

        TextureHandle = GL.CreateTexture(TextureTarget.Texture2dArray);
        GL.TextureStorage3D(TextureHandle, 1, SizedInternalFormat.DepthComponent24, resolution, resolution, levels);
        
        GL.TextureParameteri(TextureHandle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TextureParameteri(TextureHandle, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
        
        GL.TextureParameteri(TextureHandle, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToBorder);
        GL.TextureParameteri(TextureHandle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        GL.TextureParameteri(TextureHandle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
        GL.TextureParameterf(TextureHandle, TextureParameterName.TextureBorderColor, [1.0f, 1.0f, 1.0f, 1.0f]);

        Framebuffers = new int[levels];
        ShadowLevels = new ShadowLevel[levels];
        for (int i = 0; i < levels; i++)
        {
            int handle = GL.CreateFramebuffer();

            GL.NamedFramebufferTextureLayer(handle, FramebufferAttachment.DepthAttachment, TextureHandle, 0, i);
            
            GL.NamedFramebufferDrawBuffer(handle, ColorBuffer.None);
            GL.NamedFramebufferReadBuffer(handle, ColorBuffer.None);
        
            var status = GL.CheckNamedFramebufferStatus(handle, FramebufferTarget.Framebuffer);
            if (status != FramebufferStatus.FramebufferComplete)
                throw new Exception("FBO Error: " + status);

            Framebuffers[i] = handle;
            ShadowLevels[i] = new ShadowLevel(this, i);
        }

    }
    
    
    ~CSMShadowBuffer()
    {
        ThreadSafety.Instance.EnsureMainNonBlocking(() =>
        {
            if(TextureHandle == -1)
                return;

            for (int i = 0; i < Framebuffers.Length; i++)
            {
                GL.DeleteFramebuffer(Framebuffers[i]);
            }
            
            GL.DeleteTexture(TextureHandle);
        });

    }
    
    
    public class ShadowLevel : IRenderSurface
    {
        public readonly CSMShadowBuffer ShadowBuffer;
        public readonly int Level;
        
        public ShadowLevel(CSMShadowBuffer shadowBuffer, int level)
        {
            ShadowBuffer = shadowBuffer;
            Level = level;
        }
        
        public void BindFramebuffer()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, ShadowBuffer.Framebuffers[Level]);
            GL.Viewport(0, 0, ShadowBuffer.Resolution, ShadowBuffer.Resolution);
        }

        public Vector2i FramebufferSize => new(ShadowBuffer.Resolution);
        public int GetSurfaceHandle()
        {
            return ShadowBuffer.Framebuffers[Level];
        }
    }
    
}