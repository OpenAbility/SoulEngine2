using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace SoulEngine.Rendering;

/// <summary>
/// Default vertex type
/// </summary>
public struct Vertex
{
    /// <summary>
    /// The vertex position
    /// </summary>
    public Vector3 Position;
    
    private float _pad0;
    
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

    private float _pad1;

    public Vector3 Tangent;

    private float _pad2;

    /// <summary>
    /// The vertex colour
    /// </summary>
    public Colour Colour = Colour.White;
    


    public Vertex(Vector3 position, Vector2 uv, Vector3 normal)
    {
        Position = position;
        UV = uv;
        Normal = normal;
        _pad0 = 0;
    }
    
    public Vertex(Vector3 position, Vector2 uv, Vector3 normal, Colour colour)
    {
        Position = position;
        UV = uv;
        Normal = normal;
        Colour = colour;
        _pad0 = 0;
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
        
        GL.VertexArrayAttribFormat(vao, 0, 3, VertexAttribType.Float, false, 0);
        GL.VertexArrayAttribFormat(vao, 1, 2, VertexAttribType.Float, false, 4 * sizeof(float));
        GL.VertexArrayAttribFormat(vao, 2, 2, VertexAttribType.Float, false, 6 * sizeof(float));
        GL.VertexArrayAttribFormat(vao, 3, 3, VertexAttribType.Float, false, 8 * sizeof(float));
        GL.VertexArrayAttribFormat(vao, 4, 4, VertexAttribType.Float, false, 12 * sizeof(float));

        GL.VertexArrayAttribBinding(vao, 0, 0);
        GL.VertexArrayAttribBinding(vao, 1, 0);
        GL.VertexArrayAttribBinding(vao, 2, 0);
        GL.VertexArrayAttribBinding(vao, 3, 0);
        GL.VertexArrayAttribBinding(vao, 4, 0);

        return vao;
    }
}