﻿// ReSharper disable InconsistentNaming

namespace Dades
{
    /// <summary> Defines the various types of surface formats. </summary>
    /// <remarks> Values taken from http://msdn.microsoft.com/en-us/library/windows/desktop/bb172558%28v=vs.85%29.aspx </remarks>
    public enum D3DFormat
    {
        Unknown = 0,
        R8G8B8 = 20,
        A8R8G8B8 = 21,
        X8R8G8B8 = 22,
        R5G6B5 = 23,
        X1R5G5B5 = 24,
        A1R5G5B5 = 25,
        A4R4G4B4 = 26,
        R3G3B2 = 27,
        A8 = 28,
        A8R3G3B2 = 29,
        X4R4G4B4 = 30,
        A2B10G10R10 = 31,
        A8B8G8R8 = 32,
        X8B8G8R8 = 33,
        G16R16 = 34,
        A2R10G10B10 = 35,
        A16B16G16R16 = 36,

        A8P8 = 40,
        P8 = 41,

        L8 = 50,
        A8L8 = 51,
        A4L4 = 52,

        V8U8 = 60,
        L6V5U5 = 61,
        X8L8V8U8 = 62,
        DQ8W8V8U8 = 63,
        V16U16 = 64,
        A2W10V10U10 = 67,

        UYVY = (int) FOURCC.D3DFMT_UYVY,
        YUY2 = (int) FOURCC.D3DFMT_YUY2,
        R8G8_B8G8 = (int) FOURCC.D3DFMT_R8G8_B8G8,
        G8R8_G8B8 = (int) FOURCC.D3DFMT_G8R8_G8B8,
        RGBG = (int) FOURCC.FMT_RGBG,
        GRGB = (int) FOURCC.FMT_GRGB,
        Multi2_ARGB8 = (int) FOURCC.D3DFMT_MULTI2_ARGB8,

        DXT1 = (int) FOURCC.D3DFMT_DXT1,
        DXT2 = (int) FOURCC.D3DFMT_DXT2,
        DXT3 = (int) FOURCC.D3DFMT_DXT3,
        DXT4 = (int) FOURCC.D3DFMT_DXT4,
        DXT5 = (int) FOURCC.D3DFMT_DXT5,
        BC4U = (int) FOURCC.FMT_BC4U,
        BC4S = (int) FOURCC.FMT_BC4S,
        BC5U = (int) FOURCC.FMT_BC5U,
        BC5S = (int) FOURCC.FMT_BC5S,
        ATI1 = (int) FOURCC.FMT_ATI1,
        ATI2 = (int) FOURCC.FMT_ATI2,

        D16_Lockable = 70,
        D32 = 71,
        D15S1 = 73,
        D24S8 = 75,
        D24X8 = 77,
        D24X4S4 = 79,
        D16 = 80,
        L16 = 81,
        D32F_Lockable = 82,
        D24FS8 = 83,
        D32_Lockable = 84,
        S8_Lockable = 85,

        VertexData = 100,
        Index16 = 101,
        Index32 = 102,

        Q16W16V16U16 = 110,
        R16F = 111,
        G16R16F = 112,
        A16B16G16R16F = 113,
        R32F = 114,
        G32R32F = 115,
        A32B32G32R32F = 116,
        CxV8U8 = 117,
        A1 = 118,
        A2B10G10R10_XR_Bias = 119,
        BinaryBuffer = 199,

        ForceDword = 0x7fffffff // Useless, but perhaps someone would expect to see it.
    }
}
