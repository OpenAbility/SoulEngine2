using System.Text;

namespace SoulEngine.Util;

public static unsafe class Unsafe
{
    public static T ReadStruct<T>(this BinaryReader reader) where T : unmanaged
    {
        byte[] raw = reader.ReadBytes(sizeof(T));

        fixed (byte* ptr = raw)
            return *(T*)ptr;
    }
    
    public static void WriteStruct<T>(this BinaryWriter writer, T value) where T : unmanaged
    {
        T valueCopy = value;
        T* ptr = &valueCopy;
        
        writer.Write(new ReadOnlySpan<byte>(ptr, sizeof(T)));
    }

    public static TOut CastStruct<TOut, TIn>(this Span<TIn> span)   where TIn : unmanaged 
                                                                    where TOut : unmanaged
    {
        if (span.Length * sizeof(TIn) < sizeof(TOut))
            throw new Exception("Input span is smaller in size than required!");
        
        fixed (TIn* ptr = span)
            return *(TOut*)ptr;
        
    }
    
    public static Span<TOut> CastStructs<TOut, TIn>(this Span<TIn> span)   where TIn : unmanaged 
        where TOut : unmanaged
    {
        if (span.Length * sizeof(TIn) < sizeof(TOut))
            throw new Exception("Input span is smaller in size than required!");

        fixed (TIn* ptr = span)
            return new Span<TOut>(ptr, (span.Length * sizeof(TIn)) / sizeof(TOut));

    }
    
    public static unsafe string GetString(this IntPtr ptr)
    {
        return GetString((byte*)ptr);
    }

    public static unsafe string GetString(byte* ptr)
    {
        StringBuilder builder = new StringBuilder();
        int i = 0;
        while (ptr[i] != 0)
        {
            builder.Append((char)ptr[i]);
            i++;
        }
        return builder.ToString();
    }
}