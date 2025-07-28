using System.Runtime.InteropServices;
using System.Text;
using Dades;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Content;
using SoulEngine.Core;
using SoulEngine.Data;
using SoulEngine.Resources;
using SoulEngine.Util;
using StbImageSharp;

namespace SoulEngine.Rendering;

[Resource("e.tex", typeof(Loader))]
[ExpectedExtensions(".dds", ".png", ".tga", ".jpg", ".bmp")]
public class Texture : Resource
{

    private readonly Game game;
    public int Handle { get; private set; }
    public readonly int Width;
    public readonly int Height;
    public readonly int Depth;
    
    public Texture(Game game, int handle, Vector3i size)
    {
        this.game = game;
        Handle = handle;
        Width = size.X;
        Height = size.Y;
        Depth = size.Z;
    }

    public void Bind(uint slot)
    {
        GL.BindTextureUnit(slot, Handle);
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
            if(Handle != -1)
                GL.DeleteTexture(Handle);
            Handle = -1;
        });

    }
    
    public class Loader : IResourceLoader<Texture>
    {
        public Texture LoadResource(ResourceData data)
        {
            (int handle, Vector3i size) = Load(data);

            return new Texture(data.ResourceManager.Game, handle, size);
        }

        private (int, Vector3i) LoadStandardFormat(ResourceManager resourceManager, string id, ContentContext context)
        {
            Game game = resourceManager.Game;
            
            StbImage.stbi_set_flip_vertically_on_load(1);
            ImageResult loaded = ImageResult.FromStream(context.Load(id)!);

            int handle = -1;
            
            game.ThreadSafety.EnsureMain(() =>
            {
                handle = GL.CreateTexture(TextureTarget.Texture2d);

                SizedInternalFormat format = loaded.Comp switch
                {
                    ColorComponents.Grey => SizedInternalFormat.R8,
                    ColorComponents.GreyAlpha => SizedInternalFormat.Rg8,
                    ColorComponents.RedGreenBlue => SizedInternalFormat.Rgb8,
                    ColorComponents.RedGreenBlueAlpha => SizedInternalFormat.Rgba8,
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                GL.TextureStorage2D(handle, 1, format, loaded.Width, loaded.Height);

                PixelFormat pixelFormat = loaded.Comp switch
                {
                    ColorComponents.Grey => PixelFormat.Red,
                    ColorComponents.GreyAlpha => PixelFormat.Rg,
                    ColorComponents.RedGreenBlue => PixelFormat.Rgb,
                    ColorComponents.RedGreenBlueAlpha => PixelFormat.Rgba,
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                GL.TextureSubImage2D(handle, 0, 0, 0, loaded.Width, loaded.Height, pixelFormat, PixelType.UnsignedByte, loaded.Data);
                GL.GenerateTextureMipmap(handle);

                GL.TextureParameteri(handle, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.LinearMipmapLinear);
                GL.TextureParameteri(handle, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
                GL.TextureParameteri(handle, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
                GL.TextureParameteri(handle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TextureParameteri(handle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                
            });

            return (handle, new Vector3i(loaded.Width, loaded.Height, 1));
        }

        private (int, Vector3i) Load(ResourceData data)
        {
            ResourceManager resourceManager = data.ResourceManager;
            string id = data.ResourcePath;
            ContentContext content = data.Content;
            
            Game game = resourceManager.Game;

            int handle = -1;
            Vector3i size = new Vector3i();
            if (id == "null" || id == "__TEXTURE_AUTOGEN/null")
            {
                float[] textureData = new float[16 * 16 * 4];
                bool coloured = true;
                int i = 0;
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        Colour colour = coloured ? Colour.DeepPink : Colour.Black;
                        coloured = !coloured;

                        textureData[i * 4 + 0] = colour.R;
                        textureData[i * 4 + 1] = colour.G;
                        textureData[i * 4 + 2] = colour.B;
                        textureData[i * 4 + 3] = colour.A;
                        i++;
                    }

                    coloured = !coloured;
                }

                game.ThreadSafety.EnsureMain(() =>
                {
                    handle = GL.CreateTexture(TextureTarget.Texture2d);
                    GL.TextureStorage2D(handle, 1, SizedInternalFormat.Rgba8, 16, 16);

                    GL.TextureSubImage2D(handle, 0, 0, 0, 16, 16, PixelFormat.Rgba, PixelType.Float, textureData);
                    GL.GenerateTextureMipmap(handle);

                    GL.TextureParameteri(handle, TextureParameterName.TextureMinFilter,
                        (int)TextureMinFilter.LinearMipmapLinear);
                    GL.TextureParameteri(handle, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
                    GL.TextureParameteri(handle, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
                    GL.TextureParameteri(handle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TextureParameteri(handle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                });

                return (handle, new Vector3i(16, 16, 1));
            }

            if (id == "__TEXTURE_AUTOGEN/white")
            {
                game.ThreadSafety.EnsureMain(() =>
                {
                    handle = GL.CreateTexture(TextureTarget.Texture2d);
                    GL.TextureStorage2D(handle, 1, SizedInternalFormat.Rgba8, 1, 1);

                    byte[] textureData = [255, 255, 255, 255];

                    GL.TextureSubImage2D(handle, 0, 0, 0, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, textureData);
                    GL.GenerateTextureMipmap(handle);

                    GL.TextureParameteri(handle, TextureParameterName.TextureMinFilter,
                        (int)TextureMinFilter.LinearMipmapLinear);
                    GL.TextureParameteri(handle, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
                    GL.TextureParameteri(handle, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
                    GL.TextureParameteri(handle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TextureParameteri(handle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                });

                return (handle, new Vector3i(1, 1, 1));
            }

            if (id.EndsWith(".tga") || id.EndsWith(".png") || id.EndsWith(".bmp") || id.EndsWith(".jpeg") ||
                id.EndsWith(".jpg"))
                return LoadStandardFormat(resourceManager, id, content);


            DdsFileData fileData = new DdsFileData(content.Load(id)!);
            PixelFormatInfo format = new PixelFormatInfo(fileData.FormatDxgi);


            ThreadSafety ts = game.ThreadSafety;

            handle = ts.EnsureMain(() =>
            {
                int handle = GL.CreateTexture(GetTextureType(fileData));

                if (fileData.IsVolumeTexture)
                {
                    GL.TextureStorage3D(handle, fileData.MipMapCount, format.SizedInternalFormat,
                        fileData.Width, fileData.Height, fileData.Textures[0].Surfaces.Length);

                    size = new Vector3i(fileData.Width, fileData.Height, fileData.Textures[0].Surfaces.Length);

                    foreach (var slice in fileData.Textures[0].Surfaces)
                    {
                        if (fileData.IsBlockCompressed)
                        {
                            GL.CompressedTextureSubImage3D(handle, slice.Level, 0, 0, 0,
                                fileData.Width, fileData.Height, fileData.Depth, format.UnsizedInternalFormat,
                                slice.Data.Length, slice.Data);
                        }
                        else
                        {
                            GL.TextureSubImage3D(handle, slice.Level, 0, 0, 0, fileData.Width,
                                fileData.Height, fileData.Depth, format.Format, format.Type, slice.Data);
                        }
                    }

                }
                else if (fileData.IsCubemap)
                {

                    size = new Vector3i(fileData.Width, fileData.Height, 1);

                    GL.TextureStorage2D(handle, fileData.MipMapCount, format.SizedInternalFormat,
                        fileData.Width, fileData.Height);
                    foreach (var slice in fileData.Textures[0].Surfaces)
                    {
                        if (fileData.IsBlockCompressed)
                        {

                        }
                    }
                }
                else
                {
                    size = new Vector3i(fileData.Width, fileData.Height, 1);


                    int baseMip = Math.Min(EngineVarContext.Global.GetInt("e_basemip"),
                        fileData.Textures[0].Surfaces.Length - 1);

                    var baseSurface = fileData.Textures[0].Surfaces[baseMip];

                    GL.TextureStorage2D(handle, Math.Max(1, fileData.MipMapCount - baseMip),
                        format.SizedInternalFormat,
                        baseSurface.Width, baseSurface.Height);


                    for (var i = baseMip; i < fileData.Textures[0].Surfaces.Length; i++)
                    {
                        var surface = fileData.Textures[0].Surfaces[i];

                        // We must be larger than 0x0
                        if (surface.Width <= 0 || surface.Height <= 0)
                            continue;

                        if (fileData.IsBlockCompressed)
                        {
                            GL.CompressedTextureSubImage2D(handle, surface.Level - baseMip, 0, 0,
                                surface.Width, surface.Height, format.UnsizedInternalFormat, surface.Data.Length,
                                surface.Data);
                        }
                        else
                        {
                            GL.TextureSubImage2D(handle, surface.Level, 0, 0,
                                surface.Width, surface.Height, format.Format, format.Type, surface.Data);
                        }
                    }
                }

                GL.TextureParameteri(handle, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.LinearMipmapLinear);
                GL.TextureParameteri(handle, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
                GL.TextureParameteri(handle, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
                GL.TextureParameteri(handle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TextureParameteri(handle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                GL.TextureParameterf(handle, TextureParameterName.TextureMaxAnisotropy,
                    EngineVarContext.Global.GetFloat("e_anisotropy", 16));

                return handle;
            });

            return (handle, size);
        }
    }
}

[StructLayout(LayoutKind.Explicit, Pack = 0, Size = 18)]
file struct TargaHeader
{
    [FieldOffset(0)] public byte idLength;
    [FieldOffset(1)] public byte colorMapType;
    [FieldOffset(2)] public byte dataTypeCode;
    [FieldOffset(3)] public ushort colorMapOrigin;
    [FieldOffset(5)] public ushort colorMapLength;
    [FieldOffset(7)] public byte colorMapEntrySize;
    [FieldOffset(8)] public ushort x_origin;
    [FieldOffset(10)] public ushort y_origin;
    [FieldOffset(12)] public ushort height;
    [FieldOffset(14)] public ushort width;
    [FieldOffset(16)] public byte imagePixelSize;
    [FieldOffset(17)] public byte imageDescriptor;
}
