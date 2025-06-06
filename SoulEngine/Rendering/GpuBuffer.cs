using OpenTK.Graphics.OpenGL;
using SoulEngine.Core;

namespace SoulEngine.Rendering;

/// <summary>
/// Wrapper for memory stored on the GPU
/// </summary>
/// <typeparam name="T">The type to store in the buffer</typeparam>
public unsafe class GpuBuffer<T> : EngineObject, IDisposable where T : unmanaged
{
    /// <summary>
    /// The underlying object handle
    /// </summary>
    public readonly int Handle;

    public readonly int Length;
    
    
    public GpuBuffer(int count, BufferStorageMask storageMask)
    {
        GL.CreateBuffer(out Handle);
        GL.NamedBufferStorage(Handle, count * sizeof(T), null, storageMask);
        Length = count;
    }

    private GpuBuffer(int handle, int length)
    {
        Handle = handle;
        Length = length;
    }

    public static GpuBuffer<T> WrapExisting(int handle, int length)
    {
        return new GpuBuffer<T>(handle, length);
    }

    private bool disposed;
    public void Dispose()
    {
        if (mapped)
            throw new Exception("Cannot dispose of a mapped buffer!");
        
        if(disposed)
            return;
        disposed = true;
        GC.SuppressFinalize(this);
        
        GL.DeleteBuffer(Handle);
    }



    private bool mapped;

    internal void InvalidateMapping()
    {
        if (!mapped)
            throw new Exception("Buffer was not mapped!");
        GL.UnmapNamedBuffer(Handle);
        mapped = false;
    }
    
    public BufferMapping<T> Map(IntPtr offset, IntPtr size, MapBufferAccessMask bufferAccessMask)
    {
        if (mapped)
            throw new Exception("Buffer is already mapped!");

        void* ptr = GL.MapNamedBufferRange(Handle, offset * sizeof(T), size * sizeof(T), bufferAccessMask);

        mapped = true;

        return new BufferMapping<T>(this, new Span<T>(ptr, (int)size));
    }

    ~GpuBuffer()
    {
        Dispose();
    }
}

public readonly ref struct BufferMapping<T> : IDisposable where T : unmanaged
{
    private readonly GpuBuffer<T> buffer;

    public readonly Span<T> Span;

    public BufferMapping(GpuBuffer<T> buffer, Span<T> span)
    {
        this.buffer = buffer;
        Span = span;
    }

    public void Dispose()
    {
        buffer.InvalidateMapping();
    }
}