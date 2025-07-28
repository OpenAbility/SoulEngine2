using OpenTK.Graphics.OpenGL;
using SoulEngine.Rendering;

namespace SoulEngine.Models;

public class DeformationCache : IDisposable
{
    private readonly GpuBuffer<Vertex>?[] buffers;
    
    public DeformationCache(int size)
    {
        buffers = new GpuBuffer<Vertex>[size];
    }

    public void AllocateBuffer(int buffer, int size, BufferStorageMask mask)
    {
        buffers[buffer] = new GpuBuffer<Vertex>(size, mask);
    }

    public bool IsAllocated(int buffer) => buffers[buffer] != null;

    public void FreeBuffer(int buffer)
    {
        buffers[buffer]?.Dispose();
        buffers[buffer] = null;
    }

    public GpuBuffer<Vertex> GetBuffer(int buffer) => buffers[buffer]!;
    
    
    public void Dispose()
    {
        for (int i = 0; i < buffers.Length; i++)
        {
            FreeBuffer(i);
        }
    }
}