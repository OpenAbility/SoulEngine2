using System.Buffers;
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


    private UIVertex[] vertexBuffer;
    private int vertexIndex;
    
    private readonly PrimitiveType primitiveType;
    
    public DrawList(PrimitiveType primitiveType)
    {
        this.primitiveType = primitiveType;

        vertexBuffer = ArrayPool<UIVertex>.Shared.Rent(512);
    }

    private void Add(UIVertex vertex)
    {
        if (vertexIndex >= vertexBuffer.Length)
        {
            UIVertex[] newBuffer =  ArrayPool<UIVertex>.Shared.Rent(vertexBuffer.Length * 2);
            vertexBuffer.CopyTo(newBuffer, 0);
            
            ArrayPool<UIVertex>.Shared.Return(vertexBuffer);
            vertexBuffer = newBuffer;
        }

        vertexBuffer[vertexIndex++] = vertex;
    }

    public DrawList Vertex(float x, float y, float u, float v, Colour colour)
    {
        Add(new UIVertex(new Vector2(x, y), new Vector2(u, v), colour));
        return this;
    }
    
    public DrawList Vertex(Matrix4 model, float x, float y, float u, float v, Colour colour)
    {
        Add(new UIVertex((model * new Vector4(x, y, 0, 0)).Xy, new Vector2(u, v), colour));
        return this;
    }


    public void Submit()
    {
        Draw();
        vertexIndex = 0;
    }


    public unsafe void Draw()
    {
        GL.NamedBufferData(VertexBuffer, vertexIndex * sizeof(UIVertex), vertexBuffer,
            VertexBufferObjectUsage.StreamDraw);
        
        GL.VertexArrayVertexBuffer(VertexArray, 0, VertexBuffer, 0, sizeof(UIVertex));
        
        GL.BindVertexArray(VertexArray);
        GL.DrawArrays(primitiveType, 0, vertexIndex);
    }
}