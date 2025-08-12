using System.Buffers;
using OpenTK.Mathematics;
using SoulEngine.Rendering;
using SoulEngine.Resources;

namespace SoulEngine.Renderer;

public struct MeshRenderProperties()
{
    public Mesh Mesh = null!;

    public Material Material = null!;
    public Matrix4 ModelMatrix = Matrix4.Identity;

    public Matrix4[]? SkeletonBuffer;
    public ArrayPool<Matrix4> SkeletonBufferPool = null!;
    public int SkeletonBufferSize;

    public GpuBuffer<Vertex>? DeformationCache;

    public bool PerformSkeletonDeformation = false;
}