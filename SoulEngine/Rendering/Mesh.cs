using OpenTK.Graphics.OpenGL;
using SoulEngine.Core;
using SoulEngine.Util;

namespace SoulEngine.Rendering;

/// <summary>
/// Stores mesh data
/// </summary>
public unsafe class Mesh : EngineObject
{

    private static readonly int VertexArray;

    static Mesh()
    {
        VertexArray = new Vertex().CreateVertexArray();
    }

    private GpuBuffer<Vertex>? bakedVertexBuffer;
    private GpuBuffer<VertexSkinning>? skinningBuffer;
    private GpuBuffer<uint>? bakedIndexBuffer;
    
    private int indexCount;
    private int vertexCount;
    private readonly int vertexArray;
    private readonly Game game;

    private readonly Lock UpdateLock = new Lock();

    public Mesh(Game game)
    {
        this.game = game;
    }

    /// <summary>
    /// Updates this mesh (thread safe)
    /// </summary>
    /// <param name="vertices">The vertex data</param>
    /// <param name="indices">The index data</param>
    public void Update(ReadOnlySpan<Vertex> vertices, ReadOnlySpan<uint> indices)
    {
        int vCount = vertices.Length;
        int iCount = indices.Length;
        var d = game.ThreadSafety.EnsureMain(() => BeginUpdate(vCount, iCount));
        
        vertices.CopyTo(new Span<Vertex>(d.VertexData, vertices.Length));
        indices.CopyTo(new Span<uint>(d.IndexData, indices.Length));
        
        game.ThreadSafety.EnsureMain(() => EndUpdate(d));
    }
    
    public MeshBuildData BeginUpdate(int vertices, int indices)
    {
        // We don't bother with buffer resizing, just delete 'em
        int vertexBuffer = GL.CreateBuffer();
        int indexBuffer = GL.CreateBuffer();
        int skinningBuffer = GL.CreateBuffer();
        
        GL.NamedBufferStorage(vertexBuffer, vertices * sizeof(Vertex), null, BufferStorageMask.DynamicStorageBit | BufferStorageMask.MapWriteBit);
        GL.NamedBufferStorage(indexBuffer, indices * sizeof(uint), null, BufferStorageMask.DynamicStorageBit | BufferStorageMask.MapWriteBit);

        GL.NamedBufferStorage(skinningBuffer, vertices * sizeof(VertexSkinning), null, BufferStorageMask.DynamicStorageBit | BufferStorageMask.MapWriteBit);

        
        void* vtx = GL.MapNamedBuffer(vertexBuffer, BufferAccess.WriteOnly);
        void* idx = GL.MapNamedBuffer(indexBuffer, BufferAccess.WriteOnly);
        void* skn = GL.MapNamedBuffer(skinningBuffer, BufferAccess.WriteOnly);

        return new MeshBuildData
        {
            IndexData = (uint*)idx,
            VertexData = (Vertex*)vtx,
            SkinningData = (VertexSkinning*)skn,
            VertexBuffer = vertexBuffer,
            IndexBuffer = indexBuffer,
            SkinningBuffer = skinningBuffer,
            
            VertexCount = vertices,
            IndexCount = indices
        };
    }

    public void EndUpdate(MeshBuildData buildData)
    {
        lock (UpdateLock)
        {
            bakedIndexBuffer?.Dispose();
            bakedVertexBuffer?.Dispose();
            skinningBuffer?.Dispose();
            
            
            GL.UnmapNamedBuffer(buildData.VertexBuffer);
            GL.UnmapNamedBuffer(buildData.IndexBuffer);
            GL.UnmapNamedBuffer(buildData.SkinningBuffer);
            
            vertexCount = buildData.VertexCount;
            indexCount = buildData.IndexCount;

            bakedVertexBuffer = GpuBuffer<Vertex>.WrapExisting(buildData.VertexBuffer, buildData.VertexCount);
            bakedIndexBuffer = GpuBuffer<uint>.WrapExisting(buildData.IndexBuffer, buildData.IndexCount);
            skinningBuffer = GpuBuffer<VertexSkinning>.WrapExisting(buildData.SkinningBuffer, buildData.VertexCount);
        }
    }

    public static void Draw(GpuBuffer<Vertex> vertexBuffer, GpuBuffer<uint> indexBuffer, int indexCount)
    {

        GL.VertexArrayVertexBuffer(VertexArray, 0, vertexBuffer.Handle, 0, sizeof(Vertex));
        GL.VertexArrayElementBuffer(VertexArray, indexBuffer.Handle);

        GL.BindVertexArray(VertexArray);

        GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, 0);

        GL.BindVertexArray(0);
    }

    public void LockUpdates() => UpdateLock.Enter();
    public void UnlockUpdates() => UpdateLock.Exit();
    

    public int GetVertexArray() => vertexArray;
    public GpuBuffer<Vertex>? GetVertexBuffer() => bakedVertexBuffer;
    public GpuBuffer<uint>? GetIndexBuffer() => bakedIndexBuffer;
    public GpuBuffer<VertexSkinning>? GetSkinningBuffer() => skinningBuffer;

    ~Mesh()
    {
        game.ThreadSafety.EnsureMain(() =>
        {
            bakedVertexBuffer?.Dispose();
            bakedIndexBuffer?.Dispose();
        });
    }
}

public unsafe struct MeshBuildData
{
    public Vertex* VertexData;
    public uint* IndexData;
    public VertexSkinning* SkinningData;

    public Span<Vertex> Vertices => new Span<Vertex>(VertexData, VertexCount);
    public Span<uint> Indices => new Span<uint>(IndexData, IndexCount);
    public Span<VertexSkinning> Skinning => new Span<VertexSkinning>(SkinningData, VertexCount);

    internal int VertexCount;
    internal int IndexCount;

    internal int VertexBuffer;
    internal int IndexBuffer;
    internal int SkinningBuffer;
}
