namespace SoulEngine.Util;

public static unsafe class Unsafe
{
    public static T ReadStruct<T>(this BinaryReader reader) where T : unmanaged
    {
        byte[] raw = reader.ReadBytes(sizeof(T));

        fixed (byte* ptr = raw)
            return *(T*)ptr;
    }
}