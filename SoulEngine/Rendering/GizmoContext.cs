using System.Buffers;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Mathematics;

namespace SoulEngine.Rendering;

public class GizmoContext : EngineObject
{
    public bool Selected { get; internal set; }

    public Matrix4 ModelMatrix { get; internal set; }
    public Matrix4 ViewMatrix { get; internal set; }
    public Matrix4 ProjectionMatrix { get; internal set; }
    
    public Frustum CameraFrustum { get; internal set; }
    public float CurrentAspectRatio { get; internal set; }

    public readonly RenderContext RenderContext;

    private PrimitiveType primitiveType;

    private Vertex[] vertices;
    private int currentVertex = 0;

    private Shader shader;
    private readonly int vertexArray;
    private readonly int vertexBuffer;
    
    private readonly Shader defaultShader;
    private readonly Shader billboardShader;

    private uint textureBindPoint;

    public GizmoContext(Game game)
    {
        RenderContext = game.RenderContext;
        defaultShader = game.ResourceManager.Load<Shader>("shader/dd_simple_coloured.program");
        billboardShader = game.ResourceManager.Load<Shader>("shader/dd_billboarded_flat.program");
        
        
        vertexArray = new Vertex().CreateVertexArray();
        vertexBuffer = GL.CreateBuffer();

        vertices = ArrayPool<Vertex>.Shared.Rent(256);
    }

    public Shader Billboard => billboardShader;

    public void Begin(PrimitiveType primitiveType)
    {
        Begin(primitiveType, defaultShader);
    }
    
    public void Begin(PrimitiveType primitiveType, Shader shader)
    {
        currentVertex = 0;
        
        this.primitiveType = primitiveType;
        this.shader = shader;
        shader.Bind();
        textureBindPoint = 0;

        shader.Matrix("um_model", ModelMatrix, false);
        shader.Matrix("um_projection", ProjectionMatrix, false);
        shader.Matrix("um_view", ViewMatrix, false);
    }
    
    public Shader Shader => shader;

    public void Texture(string name, Texture texture)
    {
        uint binding = textureBindPoint++;
        texture.Bind(binding);
        Shader.Uniform1i(name, (int)binding);
    }
    
    public void Vertex(Vertex vertex)
    {

        if (vertices.Length <= currentVertex)
        {
            Vertex[] newVertices = ArrayPool<Vertex>.Shared.Rent(vertices.Length << 1);
            vertices.CopyTo(newVertices, 0);
            
            ArrayPool<Vertex>.Shared.Return(vertices);
            
            vertices = newVertices;
        }

        vertices[currentVertex++] = vertex;
    }
    
    public void Vertex(Vector3 vertex)
    {
        Vertex(new Vertex(vertex, Vector2.Zero, Vector3.Zero, Colour.White));
    }
    
    public void Vertex(Vector3 vertex, Vector2 uv)
    {
        Vertex(new Vertex(vertex, uv, Vector3.Zero, Colour.White));
    }
    
    public void Vertex(Vector3 vertex, Colour colour)
    {
        Vertex(new Vertex(vertex, Vector2.Zero, Vector3.Zero, colour));
    }

    public unsafe void End()
    {
        GL.NamedBufferData(vertexBuffer, currentVertex * sizeof(Vertex), vertices,
            VertexBufferObjectUsage.StreamDraw);
        
        GL.VertexArrayVertexBuffer(vertexArray, 0, vertexBuffer, 0, sizeof(Vertex));
        
        GL.BindVertexArray(vertexArray);
        GL.DrawArrays(primitiveType, 0, currentVertex);

        currentVertex = 0;
    }

    public void BillboardedSprite(Texture texture, float size = 1)
    {
        Begin(PrimitiveType.TriangleFan, billboardShader);
        
        this.Texture("ut_albedoTexture", texture);

        float half = size / 2;
        
        Vertex(new Vector3(-half, -half, 0), new Vector2(0, 0));
        Vertex(new Vector3(-half,  half, 0), new Vector2(0, 1));
        Vertex(new Vector3( half,  half, 0), new Vector2(1, 1));
        Vertex(new Vector3( half, -half, 0), new Vector2(1, 0));
        
        End();
    }
}