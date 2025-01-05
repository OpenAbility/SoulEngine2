using OpenTK.Graphics.OpenGL;
using SoulEngine.Core;

namespace SoulEngine.Rendering;

/// <summary>
/// Stores mesh data
/// </summary>
public unsafe class Mesh<T> where T : unmanaged, IVertex
{

    private static readonly Dictionary<Type, int> VertexArrays = new Dictionary<Type, int>();

    private int vertexBuffer = -1;
    private int indexBuffer = -1;
    private int indexCount;
    private readonly int vertexArray;
    private readonly Game game;

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
        vertexArray = basicInstance.CreateVertexArray();
        VertexArrays[typeof(T)] = vertexArray;
    }
    
    /// <summary>
    /// Updates this mesh 
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="indices"></param>
    public void Update(ReadOnlySpan<T> vertices, ReadOnlySpan<uint> indices)
    {
        // We don't bother with buffer resizing, just delete 'em
        if(vertexBuffer != -1) 
            GL.DeleteBuffer(vertexBuffer);
        if(indexBuffer != -1) 
            GL.DeleteBuffer(indexBuffer);

        vertexBuffer = GL.CreateBuffer();
        indexBuffer = GL.CreateBuffer();
        
        GL.NamedBufferData(vertexBuffer, vertices.Length * sizeof(T), vertices, VertexBufferObjectUsage.StaticDraw);
        GL.NamedBufferData(indexBuffer, indices.Length * sizeof(uint), indices, VertexBufferObjectUsage.StaticDraw);

        indexCount = indices.Length;
    }

    public void Draw()
    {
        GL.VertexArrayVertexBuffer(vertexArray, 0, vertexBuffer, 0, sizeof(T));
        GL.VertexArrayElementBuffer(vertexArray, indexBuffer);
        
        GL.BindVertexArray(vertexArray);
        
        GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, 0);
        
        GL.BindVertexArray(0);
    }

    ~Mesh()
    {
        game.ThreadSafety.EnsureMain(() =>
        {
            if(vertexBuffer != -1) 
                GL.DeleteBuffer(vertexBuffer);
            if(indexBuffer != -1) 
                GL.DeleteBuffer(indexBuffer);
        });
    }
}