using Dades;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace SoulEngine.Rendering;

/// <summary> Holder for OpenGL pixel formats used in texture creation.</summary>
internal struct PixelFormatInfo
{
    /// <summary> Initializes a new instance of the <see cref="PixelFormatInfo" /> struct. </summary>
    /// <param name="format">DXGI surface format.</param>
    /// <exception cref="NotSupportedException">Unsupported pixel format.</exception>
    public PixelFormatInfo(DxgiFormat format)
        : this()
    {
        FillFromDxgiFormat(format);
    }

    /// <summary>
    ///     Gets the internal pixel format. This is the real format of the image as OpenGL stores it.
    /// </summary>
    public SizedInternalFormat SizedInternalFormat { get; set; }

    public InternalFormat UnsizedInternalFormat { get; set; }

    /// <summary>
    ///     Gets the pixel format. Describes part of the format of the pixel data you are providing to OpenGL.
    /// </summary>
    public PixelFormat Format { get; set; }

    /// <summary> Gets the pixel type. </summary>
    public PixelType Type { get; set; }

    /// <summary> Gets amount of bits per pixel. </summary>
    public int BitsPerPixel { get; set; }

    /// <summary> Build OpenGL pixel format data from DXGI format.</summary>
    /// <param name="format">DXGI format.</param>
    /// <exception cref="System.NotSupportedException">Unsupported pixel format.</exception>
    private void FillFromDxgiFormat(DxgiFormat format)
    {
        BitsPerPixel = DdsTools.GetBitsPerPixel(format);

        switch (format)
        {
            case DxgiFormat.BC1_UNorm:
                SizedInternalFormat = SizedInternalFormat.CompressedRgbS3tcDxt1Ext;
                UnsizedInternalFormat = InternalFormat.CompressedRgbS3tcDxt1Ext;
                break;
            case DxgiFormat.BC1_UNorm_SRGB:
                SizedInternalFormat = SizedInternalFormat.CompressedSrgbS3tcDxt1Ext;
                UnsizedInternalFormat = InternalFormat.CompressedSrgbS3tcDxt1Ext;
                break;
            case DxgiFormat.BC2_UNorm:
                SizedInternalFormat = SizedInternalFormat.CompressedRgbaS3tcDxt3Ext;
                UnsizedInternalFormat = InternalFormat.CompressedRgbaS3tcDxt3Ext;
                break;
            case DxgiFormat.BC2_UNorm_SRGB:
                SizedInternalFormat = SizedInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext;
                UnsizedInternalFormat = InternalFormat.CompressedSrgbAlphaS3tcDxt3Ext;
                break;
            case DxgiFormat.BC3_UNorm:
                SizedInternalFormat = SizedInternalFormat.CompressedRgbaS3tcDxt5Ext;
                UnsizedInternalFormat = InternalFormat.CompressedRgbaS3tcDxt5Ext;
                break;
            case DxgiFormat.BC3_UNorm_SRGB:
                SizedInternalFormat = SizedInternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
                UnsizedInternalFormat = InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
                break;
            case DxgiFormat.BC4_UNorm:
                SizedInternalFormat = SizedInternalFormat.CompressedRedRgtc1;
                UnsizedInternalFormat = InternalFormat.CompressedRedRgtc1;
                break;
            case DxgiFormat.BC4_SNorm:
                SizedInternalFormat = SizedInternalFormat.CompressedSignedRedRgtc1;
                UnsizedInternalFormat = InternalFormat.CompressedSignedRedRgtc1;
                break;
            case DxgiFormat.BC5_UNorm:
                SizedInternalFormat = SizedInternalFormat.CompressedRgRgtc2;
                UnsizedInternalFormat = InternalFormat.CompressedRgRgtc2;
                break;
            case DxgiFormat.BC5_SNorm:
                SizedInternalFormat = SizedInternalFormat.CompressedSignedRgRgtc2;
                UnsizedInternalFormat = InternalFormat.CompressedSignedRgRgtc2;
                break;

            case DxgiFormat.A8_UNorm:
                SizedInternalFormat = SizedInternalFormat.Alpha8Ext;
                UnsizedInternalFormat = InternalFormat.Alpha8Ext;
                Format = PixelFormat.Alpha;
                Type = PixelType.UnsignedByte;
                break;

            case DxgiFormat.B5G5R5A1_UNorm:
                SizedInternalFormat = SizedInternalFormat.Rgb5A1;
                UnsizedInternalFormat = InternalFormat.Rgb5A1;
                Format = PixelFormat.Bgra;
                Type = PixelType.UnsignedShort1555Rev;
                break;

            case DxgiFormat.B5G6R5_UNorm:
                SizedInternalFormat = SizedInternalFormat.Rgb565;
                UnsizedInternalFormat = InternalFormat.Rgb565;
                Format = PixelFormat.Rgb;
                Type = PixelType.UnsignedShort565;
                break;

            case DxgiFormat.B4G4R4A4_UNorm:
                SizedInternalFormat = SizedInternalFormat.Rgba4;
                UnsizedInternalFormat = InternalFormat.Rgba4;
                Format = PixelFormat.Bgra;
                Type = PixelType.UnsignedShort4444Rev;
                break;

            case DxgiFormat.B8G8R8A8_UNorm:
                SizedInternalFormat = SizedInternalFormat.Rgba8;
                UnsizedInternalFormat = InternalFormat.Rgba8;
                Format = PixelFormat.Bgra;
                Type = PixelType.UnsignedByte;
                break;

            case DxgiFormat.B8G8R8A8_UNorm_SRGB:
                SizedInternalFormat = SizedInternalFormat.Srgb8Alpha8;
                UnsizedInternalFormat = InternalFormat.Srgb8Alpha8;
                Format = PixelFormat.Bgra;
                Type = PixelType.UnsignedByte;
                break;

            case DxgiFormat.B8G8R8X8_UNorm:
                SizedInternalFormat = SizedInternalFormat.Rgba8;
                UnsizedInternalFormat = InternalFormat.Rgba8;
                Format = PixelFormat.Bgra;
                Type = PixelType.UnsignedByte;
                break;

            case DxgiFormat.B8G8R8X8_UNorm_SRGB:
                SizedInternalFormat = SizedInternalFormat.Srgb8Alpha8;
                UnsizedInternalFormat = InternalFormat.Srgb8Alpha8;
                Format = PixelFormat.Bgra;
                Type = PixelType.UnsignedByte;
                break;

            case DxgiFormat.R10G10B10A2_UNorm:
            case DxgiFormat.R10G10B10A2_UInt:
            case DxgiFormat.R10G10B10_XR_BIAS_A2_UNorm:
                SizedInternalFormat = SizedInternalFormat.Rgb10A2;
                UnsizedInternalFormat = InternalFormat.Rgb10A2;
                Format = PixelFormat.Rgba;
                Type = PixelType.UnsignedInt2101010Rev;
                break;

            case DxgiFormat.R11G11B10_Float:
                SizedInternalFormat = SizedInternalFormat.R11fG11fB10f;
                UnsizedInternalFormat = InternalFormat.R11fG11fB10f;
                Format = PixelFormat.Rgb;
                Type = PixelType.UnsignedInt10f11f11fRev;
                break;

            case DxgiFormat.R16_UInt:
            case DxgiFormat.R16_UNorm:
                SizedInternalFormat = SizedInternalFormat.R16;
                UnsizedInternalFormat = InternalFormat.R16;
                Format = PixelFormat.Red;
                Type = PixelType.UnsignedShort;
                break;

            case DxgiFormat.R16_Float:
                SizedInternalFormat = SizedInternalFormat.R16f;
                UnsizedInternalFormat = InternalFormat.R16f;
                Format = PixelFormat.Red;
                Type = PixelType.HalfFloat;
                break;

            case DxgiFormat.R16_SNorm:
            case DxgiFormat.R16_SInt:
                SizedInternalFormat = SizedInternalFormat.R16;
                UnsizedInternalFormat = InternalFormat.R16;
                Format = PixelFormat.Red;
                Type = PixelType.Short;
                break;

            case DxgiFormat.R16G16_Float:
                SizedInternalFormat = SizedInternalFormat.Rg16f;
                UnsizedInternalFormat = InternalFormat.Rg16f;
                Format = PixelFormat.Rg;
                Type = PixelType.HalfFloat;
                break;

            case DxgiFormat.R16G16_SNorm:
            case DxgiFormat.R16G16_SInt:
                SizedInternalFormat = SizedInternalFormat.Rg16;
                UnsizedInternalFormat = InternalFormat.Rg16;
                Format = PixelFormat.Rg;
                Type = PixelType.Short;
                break;

            case DxgiFormat.R16G16_UNorm:
            case DxgiFormat.R16G16_UInt:
                SizedInternalFormat = SizedInternalFormat.Rg16;
                UnsizedInternalFormat = InternalFormat.Rg16;
                Format = PixelFormat.Rg;
                Type = PixelType.UnsignedShort;
                break;

            case DxgiFormat.R16G16B16A16_Float:
                SizedInternalFormat = SizedInternalFormat.Rgba16f;
                UnsizedInternalFormat = InternalFormat.Rgba16f;
                Format = PixelFormat.Rgba;
                Type = PixelType.HalfFloat;
                break;

            case DxgiFormat.R16G16B16A16_UNorm:
            case DxgiFormat.R16G16B16A16_UInt:
                SizedInternalFormat = SizedInternalFormat.Rgba16;
                UnsizedInternalFormat = InternalFormat.Rgba16;
                Format = PixelFormat.Rgba;
                Type = PixelType.UnsignedShort;
                break;

            case DxgiFormat.R16G16B16A16_SNorm:
            case DxgiFormat.R16G16B16A16_SInt:
                SizedInternalFormat = SizedInternalFormat.Rgba16;
                UnsizedInternalFormat = InternalFormat.Rgba16;
                Format = PixelFormat.Rgba;
                Type = PixelType.Short;
                break;


            case DxgiFormat.R32_Float:
                SizedInternalFormat = SizedInternalFormat.R32f;
                UnsizedInternalFormat = InternalFormat.R32f;
                Format = PixelFormat.Red;
                Type = PixelType.Float;
                break;

            case DxgiFormat.R32_UInt:
                SizedInternalFormat = SizedInternalFormat.R32f;
                UnsizedInternalFormat = InternalFormat.R32f;
                Format = PixelFormat.Red;
                Type = PixelType.UnsignedInt;
                break;

            case DxgiFormat.R32_SInt:
                SizedInternalFormat = SizedInternalFormat.R32f;
                UnsizedInternalFormat = InternalFormat.R32f;
                Format = PixelFormat.Red;
                Type = PixelType.Int;
                break;

            case DxgiFormat.R32G32_Float:
                SizedInternalFormat = SizedInternalFormat.Rg32f;
                UnsizedInternalFormat = InternalFormat.Rg32f;
                Format = PixelFormat.Rg;
                Type = PixelType.Float;
                break;

            case DxgiFormat.R32G32_SInt:
                SizedInternalFormat = SizedInternalFormat.Rg32f;
                UnsizedInternalFormat = InternalFormat.Rg32f;
                Format = PixelFormat.Rg;
                Type = PixelType.Int;
                break;

            case DxgiFormat.R32G32_UInt:
                SizedInternalFormat = SizedInternalFormat.Rg32f;
                UnsizedInternalFormat = InternalFormat.Rg32f;
                Format = PixelFormat.Rg;
                Type = PixelType.UnsignedInt;
                break;

            case DxgiFormat.R32G32B32_Float:
                SizedInternalFormat = SizedInternalFormat.Rgb32f;
                UnsizedInternalFormat = InternalFormat.Rgb32f;
                Format = PixelFormat.Rgb;
                Type = PixelType.Float;
                break;

            case DxgiFormat.R32G32B32_SInt:
                SizedInternalFormat = SizedInternalFormat.Rgb32f;
                UnsizedInternalFormat = InternalFormat.Rgb32f;
                Format = PixelFormat.Rgb;
                Type = PixelType.Int;
                break;

            case DxgiFormat.R32G32B32_UInt:
                SizedInternalFormat = SizedInternalFormat.Rgb32f;
                UnsizedInternalFormat = InternalFormat.Rgb32f;
                Format = PixelFormat.Rgb;
                Type = PixelType.UnsignedInt;
                break;

            case DxgiFormat.R32G32B32A32_Float:
                SizedInternalFormat = SizedInternalFormat.Rgba32f;
                UnsizedInternalFormat = InternalFormat.Rgba32f;
                Format = PixelFormat.Rgba;
                Type = PixelType.Float;
                break;
            case DxgiFormat.R32G32B32A32_SInt:
                SizedInternalFormat = SizedInternalFormat.Rgba32f;
                UnsizedInternalFormat = InternalFormat.Rgba32f;
                Format = PixelFormat.Rgba;
                Type = PixelType.Int;
                break;

            case DxgiFormat.R32G32B32A32_UInt:
                SizedInternalFormat = SizedInternalFormat.Rgba32f;
                UnsizedInternalFormat = InternalFormat.Rgba32f;
                Format = PixelFormat.Rgba;
                Type = PixelType.UnsignedInt;
                break;

            case DxgiFormat.R8_SNorm:
            case DxgiFormat.R8_SInt:
                SizedInternalFormat = SizedInternalFormat.R8;
                UnsizedInternalFormat = InternalFormat.R8;
                Format = PixelFormat.Red;
                Type = PixelType.Byte;
                break;

            case DxgiFormat.R8_UNorm:
            case DxgiFormat.R8_UInt:
                SizedInternalFormat = SizedInternalFormat.R8;
                UnsizedInternalFormat = InternalFormat.R8;
                Format = PixelFormat.Red;
                Type = PixelType.UnsignedByte;
                break;

            case DxgiFormat.G8R8_G8B8_UNorm:
                SizedInternalFormat = SizedInternalFormat.Rgb8;
                UnsizedInternalFormat = InternalFormat.Rgb8;
                Format = PixelFormat.Bgra;
                Type = PixelType.UnsignedShort4444Rev;
                break;

            case DxgiFormat.R8G8_B8G8_UNorm:
                SizedInternalFormat = SizedInternalFormat.Rgb8;
                UnsizedInternalFormat = InternalFormat.Rgb8;
                Format = PixelFormat.Rgba;
                Type = PixelType.UnsignedShort4444;
                break;

            case DxgiFormat.R8G8_SNorm:
            case DxgiFormat.R8G8_SInt:
                SizedInternalFormat = SizedInternalFormat.Rg8;
                UnsizedInternalFormat = InternalFormat.Rg8;
                Format = PixelFormat.Rg;
                Type = PixelType.Byte;
                break;

            case DxgiFormat.R8G8_UInt:
            case DxgiFormat.R8G8_UNorm:
                SizedInternalFormat = SizedInternalFormat.Rg8;
                UnsizedInternalFormat = InternalFormat.Rg8;
                Format = PixelFormat.Rg;
                Type = PixelType.UnsignedByte;
                break;

            case DxgiFormat.R8G8B8A8_SNorm:
            case DxgiFormat.R8G8B8A8_SInt:
                SizedInternalFormat = SizedInternalFormat.Rgba8;
                UnsizedInternalFormat = InternalFormat.Rgba8;
                Format = PixelFormat.Rgba;
                Type = PixelType.Byte;
                break;

            case DxgiFormat.R8G8B8A8_UNorm:
            case DxgiFormat.R8G8B8A8_UInt:
            case DxgiFormat.R8G8B8A8_UNorm_SRGB:
                SizedInternalFormat = SizedInternalFormat.Srgb8Alpha8;
                UnsizedInternalFormat = InternalFormat.Srgb8Alpha8;
                Format = PixelFormat.Rgba;
                Type = PixelType.UnsignedByte;
                break;

            // Needs to be decoded like this: decoded.rgb = encoded.rgb * pow(2, encoded.a)
            case DxgiFormat.R9G9B9E5_SHAREDEXP:
                SizedInternalFormat = SizedInternalFormat.Rgb9E5;
                UnsizedInternalFormat = InternalFormat.Rgb9E5;
                Format = PixelFormat.Rgb;
                Type = PixelType.UnsignedInt5999Rev;
                break;
            default:
                throw new NotSupportedException($"DXGI format '{format}' is not supported.");
        }
    }
}