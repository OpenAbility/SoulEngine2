using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace SoulEngine.Rendering;

/// <summary>
/// Provides the common vertex interface
/// </summary>
public interface IVertex
{
    /// <summary>
    /// Creates the vertex format
    /// </summary>
    /// <returns>The vertex format object</returns>
    public int CreateVertexArray();
}

/// <summary>
/// Default vertex type
/// </summary>
public struct Vertex : IVertex
{
    /// <summary>
    /// The vertex position
    /// </summary>
    public Vector3 Position;
    
    /// <summary>
    /// The UV coordinate
    /// </summary>
    public Vector2 UV;
    
    /// <summary>
    /// A second UV coordinate(just in case)
    /// </summary>
    public Vector2 UV2 = Vector2.Zero;

    /// <summary>
    /// The vertex normal
    /// </summary>
    public Vector3 Normal;

    /// <summary>
    /// The vertex colour
    /// </summary>
    public Colour Colour = Colour.White;

    public JointIndices Indices;

    public Vector4 Weights;


    public Vertex(Vector3 position, Vector2 uv, Vector3 normal)
    {
        Position = position;
        UV = uv;
        Normal = normal;
    }
    
    public Vertex(Vector3 position, Vector2 uv, Vector3 normal, Colour colour)
    {
        Position = position;
        UV = uv;
        Normal = normal;
        Colour = colour;
    }

    /// <inheritdoc />
    public int CreateVertexArray()
    {
        int vao = GL.CreateVertexArray();

        GL.EnableVertexArrayAttrib(vao, 0);
        GL.EnableVertexArrayAttrib(vao, 1);
        GL.EnableVertexArrayAttrib(vao, 2);
        GL.EnableVertexArrayAttrib(vao, 3);
        GL.EnableVertexArrayAttrib(vao, 4);
        GL.EnableVertexArrayAttrib(vao, 5);
        GL.EnableVertexArrayAttrib(vao, 6);
        
        GL.VertexArrayAttribFormat(vao, 0, 3, VertexAttribType.Float, false, 0);
        GL.VertexArrayAttribFormat(vao, 1, 2, VertexAttribType.Float, false, 3 * sizeof(float));
        GL.VertexArrayAttribFormat(vao, 2, 2, VertexAttribType.Float, false, 5 * sizeof(float));
        GL.VertexArrayAttribFormat(vao, 3, 3, VertexAttribType.Float, false, 7 * sizeof(float));
        GL.VertexArrayAttribFormat(vao, 4, 4, VertexAttribType.Float, false, 10 * sizeof(float));
        GL.VertexArrayAttribIFormat(vao, 5, 4, VertexAttribIType.UnsignedInt, 14 * sizeof(float));
        GL.VertexArrayAttribFormat(vao, 6, 4, VertexAttribType.Float, false, 18 * sizeof(float));

        GL.VertexArrayAttribBinding(vao, 0, 0);
        GL.VertexArrayAttribBinding(vao, 1, 0);
        GL.VertexArrayAttribBinding(vao, 2, 0);
        GL.VertexArrayAttribBinding(vao, 3, 0);
        GL.VertexArrayAttribBinding(vao, 4, 0);
        GL.VertexArrayAttribBinding(vao, 5, 0);
        GL.VertexArrayAttribBinding(vao, 6, 0);

        return vao;
    }
}