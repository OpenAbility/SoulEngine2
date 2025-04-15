using OpenTK.Graphics.OpenGL;
using SoulEngine.Core;
using SoulEngine.Util;

namespace SoulEngine.Rendering;

/// <summary>
/// Stores mesh data
/// </summary>
public unsafe class Mesh<T> : IDrawableMesh
        where T : unmanaged, IVertex
{

    private static readonly Dictionary<Type, int> VertexArrays = new Dictionary<Type, int>();

    private int bakedVertexBuffer = -1;
    private int bakedIndexBuffer = -1;
    private int indexCount;
    private readonly int vertexArray;
    private readonly Game game;

    private readonly Lock UpdateLock = new Lock();

    public Mesh(Game game)
    {
        this.game = game;
        if (VertexArrays.TryGetValue(typeof(T), out int existingVAO))
        {
            vertexArray = existingVAO;
            return;
        }

        // Since it's a struct type it doesn't need a constructor
        T basicInstance = default(T);
        vertexArray = game.ThreadSafety.EnsureMain(basicInstance.CreateVertexArray);
        VertexArrays[typeof(T)] = vertexArray;
    }

    /// <summary>
    /// Updates this mesh (thread safe)
    /// </summary>
    /// <param name="vertices">The vertex data</param>
    /// <param name="indices">The index data</param>
    public void Update(ReadOnlySpan<T> vertices, ReadOnlySpan<uint> indices)
    {
        int vCount = vertices.Length;
        int iCount = indices.Length;
        var d = game.ThreadSafety.EnsureMain(() => BeginUpdate(vCount, iCount));
        
        vertices.CopyTo(new Span<T>(d.VertexData, vertices.Length));
        indices.CopyTo(new Span<uint>(d.IndexData, indices.Length));
        
        game.ThreadSafety.EnsureMain(() => EndUpdate(d));
    }
    
    public MeshBuildData<T> BeginUpdate(int vertices, int indices)
    {
        // We don't bother with buffer resizing, just delete 'em
        int vertexBuffer = GL.CreateBuffer();
        int indexBuffer = GL.CreateBuffer();
        
        GL.NamedBufferStorage(vertexBuffer, vertices * sizeof(T), null, BufferStorageMask.DynamicStorageBit | BufferStorageMask.MapWriteBit);
        GL.NamedBufferStorage(indexBuffer, indices * sizeof(uint), null, BufferStorageMask.DynamicStorageBit | BufferStorageMask.MapWriteBit);

        void* vtx = GL.MapNamedBuffer(vertexBuffer, BufferAccess.WriteOnly);
        void* idx = GL.MapNamedBuffer(indexBuffer, BufferAccess.WriteOnly);

        return new MeshBuildData<T>
        {
            IndexData = (uint*)idx,
            VertexData = (T*)vtx,
            VertexBuffer = vertexBuffer,
            IndexBuffer = indexBuffer,
            
            VertexCount = vertices,
            IndexCount = indices
        };
    }

    public void EndUpdate(MeshBuildData<T> buildData)
    {
        lock (UpdateLock)
        {
            
            if(bakedIndexBuffer != -1)
                GL.DeleteBuffer(bakedIndexBuffer);
            if(bakedVertexBuffer != -1)
                GL.DeleteBuffer(bakedVertexBuffer);
            
            GL.UnmapNamedBuffer(buildData.VertexBuffer);
            GL.UnmapNamedBuffer(buildData.IndexBuffer);

            indexCount = buildData.IndexCount;

            bakedVertexBuffer = buildData.VertexBuffer;
            bakedIndexBuffer = buildData.IndexBuffer;
        }
    }

    public void Draw()
    {
        lock (UpdateLock)
        {
            if (bakedVertexBuffer == -1 || bakedIndexBuffer == -1)
                return;

            GL.VertexArrayVertexBuffer(vertexArray, 0, bakedVertexBuffer, 0, sizeof(T));
            GL.VertexArrayElementBuffer(vertexArray, bakedIndexBuffer);

            GL.BindVertexArray(vertexArray);

            GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
        }
    }

    ~Mesh()
    {
        game.ThreadSafety.EnsureMain(() =>
        {
            if(bakedVertexBuffer != -1) 
                GL.DeleteBuffer(bakedVertexBuffer);
            if(bakedIndexBuffer != -1) 
                GL.DeleteBuffer(bakedIndexBuffer);
        });
    }
}

public unsafe struct MeshBuildData<T> where T : unmanaged, IVertex
{
    public T* VertexData;
    public uint* IndexData;

    public Span<T> Vertices => new Span<T>(VertexData, VertexCount);
    public Span<uint> Indices => new Span<uint>(IndexData, IndexCount);

    internal int VertexCount;
    internal int IndexCount;

    internal int VertexBuffer;
    internal int IndexBuffer;
}

/// <summary>
/// Should only ever be implemented by <see cref="Mesh{T}"/>, unless you're feeling really daring.
/// Makes it possible to work around the fact that <see cref="Mesh{T}"/> is generic
/// </summary>
public interface IDrawableMesh
{
    public void Draw();
}