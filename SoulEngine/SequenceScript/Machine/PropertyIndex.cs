using System.Runtime.InteropServices;

namespace SoulEngine.SequenceScript.Machine;

/// <summary>
/// Encodes the index of symbols as 2 numerics packed into 32 bits.
/// Supports up to 254 libraries(libraries 0x00 and 0xFF are reserved).
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 4, Pack = 0)]
public struct PropertyIndex
{
    [FieldOffset(0)] private uint valueIndex;
    [FieldOffset(3)] public byte LibraryIndex;

    public uint ValueIndex
    {
        get => valueIndex & 0x00FFFFFF;
        set => valueIndex = (value & 0x00FFFFFF) | (valueIndex & 0xFF000000);
    }

    public bool IsScopeLocal => LibraryIndex == 0xFF;
    public bool IsPackageLocal => LibraryIndex == 0x00;
}