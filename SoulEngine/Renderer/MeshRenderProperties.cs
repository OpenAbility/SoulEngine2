using System.Buffers;
using OpenTK.Mathematics;
using SoulEngine.Rendering;
using SoulEngine.Resources;

namespace SoulEngine.Renderer;

public struct MeshRenderProperties()
{
    public Mesh Mesh;

    public Material Material;
    public Matrix4 ModelMatrix = Matrix4.Identity;

    public Matrix4[]? SkeletonBuffer;
    public ArrayPool<Matrix4> SkeletonBufferPool;
    public int SkeletonBufferSize;
}