using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Renderer;
using SoulEngine.Rendering;

namespace SoulEngine.UI.Rendering;

/// <summary>
/// Helps manage immediate mode rendering
/// </summary>
public class DrawList
{
    private static readonly int VertexArray;
    private static readonly int VertexBuffer;
    
    static DrawList()
    {
        VertexArray = new UIVertex().CreateVertexArray();

        VertexBuffer = GL.CreateBuffer();
    }

    private readonly List<UIVertex> vertices = new List<UIVertex>(256);
    private readonly PrimitiveType primitiveType;
    
    public DrawList(PrimitiveType primitiveType)
    {
        this.primitiveType = primitiveType;
    }

    public DrawList Vertex(float x, float y, float u, float v, Colour colour)
    {
        vertices.Add(new UIVertex(new Vector2(x, y), new Vector2(u, v), colour));
        return this;
    }
    
    public DrawList Vertex(Matrix4 model, float x, float y, float u, float v, Colour colour)
    {
        vertices.Add(new UIVertex((model * new Vector4(x, y, 0, 0)).Xy, new Vector2(u, v), colour));
        return this;
    }


    public void Submit()
    {
        Draw();
        vertices.Clear();
    }

    public void Submit(IRenderPipeline renderPipeline)
    {
        
    }


    public unsafe void Draw()
    {
        GL.NamedBufferData(VertexBuffer, vertices.Count * sizeof(UIVertex), vertices.ToArray(),
            VertexBufferObjectUsage.StreamDraw);
        
        GL.VertexArrayVertexBuffer(VertexArray, 0, VertexBuffer, 0, sizeof(UIVertex));
        
        GL.BindVertexArray(VertexArray);
        GL.DrawArrays(primitiveType, 0, vertices.Count);
    }
}