using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Core;

namespace SoulEngine.Rendering;

public class GizmoContext
{
    public bool Selected { get; internal set; }

    public Matrix4 ModelMatrix { get; internal set; }
    public Matrix4 ViewMatrix { get; internal set; }
    public Matrix4 ProjectionMatrix { get; internal set; }

    public readonly RenderContext RenderContext;

    private PrimitiveType primitiveType;
    public SceneRenderData SceneRenderData { get; internal set; }

    private List<Vertex> vertices = new List<Vertex>();

    private Shader shader;
    private readonly int vertexArray;
    private readonly int vertexBuffer;
    
    private readonly Shader defaultShader;
    private readonly Shader billboardShader;

    private uint textureBindPoint;

    public GizmoContext(Game game)
    {
        RenderContext = game.RenderContext;
        defaultShader = game.ResourceManager.Load<Shader>("shader/simple_coloured.program");
        billboardShader = game.ResourceManager.Load<Shader>("shader/billboarded_flat.program");
        
        
        vertexArray = new Vertex().CreateVertexArray();
        vertexBuffer = GL.CreateBuffer();
    }

    public Shader Billboard => billboardShader;

    public void Begin(PrimitiveType primitiveType)
    {
        Begin(primitiveType, defaultShader);
    }
    
    public void Begin(PrimitiveType primitiveType, Shader shader)
    {
        vertices.Clear();
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
        vertices.Add(vertex);
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
        GL.NamedBufferData(vertexBuffer, vertices.Count * sizeof(Vertex), vertices.ToArray(),
            VertexBufferObjectUsage.StreamDraw);
        
        GL.VertexArrayVertexBuffer(vertexArray, 0, vertexBuffer, 0, sizeof(Vertex));
        
        GL.BindVertexArray(vertexArray);
        GL.DrawArrays(primitiveType, 0, vertices.Count);
        
        vertices.Clear();
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