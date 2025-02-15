using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Rendering;

namespace SoulEngine.UI.Rendering;

public struct UIVertex : IVertex
{
    public Vector2 Position;
    public Vector2 UV;
    public Colour Colour;

    public UIVertex(Vector2 position, Vector2 uv, Colour colour)
    {
        Position = position;
        UV = uv;
        Colour = colour;
    }

    public int CreateVertexArray()
    {
        int vao = GL.CreateVertexArray();

        GL.EnableVertexArrayAttrib(vao, 0);
        GL.EnableVertexArrayAttrib(vao, 1);
        GL.EnableVertexArrayAttrib(vao, 2);
        
        GL.VertexArrayAttribFormat(vao, 0, 2, VertexAttribType.Float, false, 0);
        GL.VertexArrayAttribFormat(vao, 1, 2, VertexAttribType.Float, false, 2 * sizeof(float));
        GL.VertexArrayAttribFormat(vao, 2, 4, VertexAttribType.Float, false, 4 * sizeof(float));
            
        GL.VertexArrayAttribBinding(vao, 0, 0);
        GL.VertexArrayAttribBinding(vao, 1, 0);
        GL.VertexArrayAttribBinding(vao, 2, 0);

        return vao;
    }
}