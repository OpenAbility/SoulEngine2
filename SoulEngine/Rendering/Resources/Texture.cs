using Dades;
using OpenTK.Graphics.OpenGL;
using SoulEngine.Content;
using SoulEngine.Core;
using SoulEngine.Resources;
using SoulEngine.Util;

namespace SoulEngine.Rendering;

[Resource(typeof(Loader))]
public class Texture : Resource
{
    private int handle;
    private readonly Game game;
    
    public Texture(Game game)
    {
        this.game = game;
    }
    
    private void Load(ResourceManager resourceManager, string id, ContentContext content)
    {
        if (id == "null")
        {
            game.ThreadSafety.EnsureMain(() =>
            {
                handle = GL.CreateTexture(TextureTarget.Texture2d);
                GL.TextureStorage2D(handle, 1, SizedInternalFormat.Rgba8, 16, 16);

                float[] textureData = new float[16 * 16 * 4];
                bool coloured = true;
                int i = 0;
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        Colour colour = coloured ? Colour.Pink : Colour.Black;
                        coloured = !coloured;

                        textureData[i * 4 + 0] = colour.R;
                        textureData[i * 4 + 1] = colour.G;
                        textureData[i * 4 + 2] = colour.B;
                        textureData[i * 4 + 3] = colour.A;
                        i++;
                    }

                    coloured = !coloured;
                }
                
                GL.TextureSubImage2D(handle, 0, 0, 0, 16, 16, PixelFormat.Rgba, PixelType.Float, textureData);
                GL.GenerateTextureMipmap(handle);
                
                GL.TextureParameteri(handle, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.LinearMipmapNearest);
                GL.TextureParameteri(handle, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
                GL.TextureParameteri(handle, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
                GL.TextureParameteri(handle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TextureParameteri(handle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                
            });
            
            return;
        }
        
        
        DdsFileData fileData = new DdsFileData(content.Load(id)!);
        PixelFormatInfo format = new PixelFormatInfo(fileData.FormatDxgi);


        ThreadSafety ts = game.ThreadSafety;

        handle = ts.EnsureMain(() => GL.CreateTexture(GetTextureType(fileData)));

        if (fileData.IsVolumeTexture)
        {
            ts.EnsureMain(() => GL.TextureStorage3D(handle, fileData.MipMapCount, format.SizedInternalFormat,
                fileData.Width, fileData.Height, fileData.Textures[0].Surfaces.Length));

            foreach (var slice in fileData.Textures[0].Surfaces)
            {
                if (fileData.IsBlockCompressed)
                {
                    ts.EnsureMain(() => GL.CompressedTextureSubImage3D(handle, slice.Level, 0, 0, 0,
                        fileData.Width, fileData.Height, fileData.Depth, format.UnsizedInternalFormat,
                        slice.Data.Length, slice.Data));
                }
                else
                {
                    ts.EnsureMain(() => GL.TextureSubImage3D(handle, slice.Level, 0, 0, 0, fileData.Width,
                        fileData.Height, fileData.Depth, format.Format, format.Type, slice.Data));
                }
            }
            
        } else if (fileData.IsCubemap)
        {;
            ts.EnsureMain(() => GL.TextureStorage2D(handle, fileData.MipMapCount, format.SizedInternalFormat,
                fileData.Width, fileData.Height));
            foreach (var slice in fileData.Textures[0].Surfaces)
            {
                if (fileData.IsBlockCompressed)
                {
                   
                }
            }
        } else
        {
            ts.EnsureMain(() => GL.TextureStorage2D(handle, Math.Max(1, fileData.MipMapCount), format.SizedInternalFormat,
                fileData.Width, fileData.Height));
            foreach (var surface in fileData.Textures[0].Surfaces)
            {
                if (fileData.IsBlockCompressed)
                {
                    ts.EnsureMain(() => GL.CompressedTextureSubImage2D(handle, surface.Level, 0, 0,
                        surface.Width, surface.Height, format.UnsizedInternalFormat, surface.Data.Length,
                        surface.Data));
                }
                else
                {
                    ts.EnsureMain(() => GL.TextureSubImage2D(handle, surface.Level, 0, 0,
                        surface.Width, surface.Height, format.Format, format.Type, surface.Data));
                }
            }
        }

        ts.EnsureMain(() =>
        {
            GL.TextureParameteri(handle, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.LinearMipmapNearest);
            GL.TextureParameteri(handle, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TextureParameteri(handle, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
            GL.TextureParameteri(handle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TextureParameteri(handle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        });
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
    
    public class Loader : IResourceLoader<Texture>
    {
        public Texture LoadResource(ResourceManager resourceManager, string id, ContentContext content)
        {
            Texture texture = new Texture(resourceManager.Game);
            texture.Load(resourceManager, id, content);
            return texture;
        }
    }
}
