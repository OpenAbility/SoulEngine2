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
    /// The vertex colour
    /// </summary>
    public Colour Colour = Colour.White;

    public Vertex(Vector3 pos)
    {
        Position = pos;
    }

    public Vertex(float x, float y, float z, Colour colour)
    {
        Position = new Vector3(x, y, z);
        Colour = colour;
    }


    /// <inheritdoc />
    public int CreateVertexArray()
    {
        int vao = GL.CreateVertexArray();

        GL.EnableVertexArrayAttrib(vao, 0);
        GL.EnableVertexArrayAttrib(vao, 1);
        
        GL.VertexArrayAttribFormat(vao, 0, 3, VertexAttribType.Float, false, 0);
        GL.VertexArrayAttribFormat(vao, 1, 4, VertexAttribType.Float, false, 3 * sizeof(float));

        GL.VertexArrayAttribBinding(vao, 0, 0);
        GL.VertexArrayAttribBinding(vao, 1, 0);

        return vao;
    }
}