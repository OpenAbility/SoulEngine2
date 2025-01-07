using Dades;
using OpenTK.Graphics.OpenGL;
using SoulEngine.Content;
using SoulEngine.Core;
using SoulEngine.Resources;
using SoulEngine.Util;

namespace SoulEngine.Rendering;

public class Texture : Resource
{
    private int handle;
    private readonly Game game;
    
    public Texture(Game game)
    {
        this.game = game;
    }
    
    public override async Task Load(ResourceManager resourceManager, string id, ContentContext content)
    {
        DdsFileData fileData = new DdsFileData(content.Load(id)!);
        PixelFormatInfo format = new PixelFormatInfo(fileData.FormatDxgi);


        ThreadSafety ts = game.ThreadSafety;

        handle = await ts.EnsureMainAsync(() => GL.CreateTexture(GetTextureType(fileData)));

        if (fileData.IsVolumeTexture)
        {
            await ts.EnsureMainAsync(() => GL.TextureStorage3D(handle, fileData.MipMapCount, format.SizedInternalFormat,
                fileData.Width, fileData.Height, fileData.Textures[0].Surfaces.Length));

            foreach (var slice in fileData.Textures[0].Surfaces)
            {
                if (fileData.IsBlockCompressed)
                {
                    await ts.EnsureMainAsync(() => GL.CompressedTextureSubImage3D(handle, slice.Level, 0, 0, 0,
                        fileData.Width, fileData.Height, fileData.Depth, format.UnsizedInternalFormat,
                        slice.Data.Length, slice.Data));
                }
                else
                {
                    await ts.EnsureMainAsync(() => GL.TextureSubImage3D(handle, slice.Level, 0, 0, 0, fileData.Width,
                        fileData.Height, fileData.Depth, format.Format, format.Type, slice.Data));
                }
            }
            
        } else if (fileData.IsCubemap)
        {;
            await ts.EnsureMainAsync(() => GL.TextureStorage2D(handle, fileData.MipMapCount, format.SizedInternalFormat,
                fileData.Width, fileData.Height));
            foreach (var slice in fileData.Textures[0].Surfaces)
            {
                if (fileData.IsBlockCompressed)
                {
                   
                }
            }
        } else
        {
            await ts.EnsureMainAsync(() => GL.TextureStorage2D(handle, fileData.MipMapCount, format.SizedInternalFormat,
                fileData.Width, fileData.Height));
            foreach (var surface in fileData.Textures[0].Surfaces)
            {
                if (fileData.IsBlockCompressed)
                {
                    await ts.EnsureMainAsync(() => GL.CompressedTextureSubImage2D(handle, surface.Level, 0, 0,
                        surface.Width, surface.Height, format.UnsizedInternalFormat, surface.Data.Length,
                        surface.Data));
                }
                else
                {
                    await ts.EnsureMainAsync(() => GL.TextureSubImage2D(handle, surface.Level, 0, 0,
                        fileData.Width, fileData.Height, format.Format, format.Type, surface.Data));
                }
            }
        }

        await ts.EnsureMainAsync(() => GL.TextureParameteri(handle, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.LinearMipmapNearest));
        await ts.EnsureMainAsync(() =>
            GL.TextureParameteri(handle, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear));

        await ts.EnsureMainAsync(() =>
            GL.TextureParameteri(handle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge));
        await ts.EnsureMainAsync(() =>
            GL.TextureParameteri(handle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge));
    }

    public void Bind(uint slot)
    {
        GL.BindTextureUnit(slot, handle);
    }

    private static TextureTarget GetTextureType(DdsFileData fileData)
    {
        if (fileData.IsVolumeTexture)
            return TextureTarget.Texture3d;

        if (fileData.IsCubemap)
            return TextureTarget.TextureCubeMap;

        return TextureTarget.Texture2d;
    }
    
    ~Texture()
    {
        game?.ThreadSafety.EnsureMain(() =>
        {
            if(handle != -1)
                GL.DeleteTexture(handle);
            handle = -1;
        });

    }
}
