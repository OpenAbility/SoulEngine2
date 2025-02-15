using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Mathematics;
using SoulEngine.Rendering;
using SoulEngine.UI.Rendering;
using EnableCap = OpenTK.Graphics.OpenGL.EnableCap;

namespace SoulEngine.UI;

/// <summary>
/// Handles UI rendering
/// </summary>
public class UIContext
{
    public readonly Game Game;

    private readonly Shader defaultShader;

    public Vector2i Size { get; private set; }


    private Matrix4 projection;
    private RenderContext renderContext;

    public UIContext(Game game)
    {
        Game = game;
        defaultShader = game.ResourceManager.Load<Shader>("shader/ui.program");
        currentShader = defaultShader;
    }

    public void OnBegin(RenderContext renderContext, Vector2i size)
    {
        this.renderContext = renderContext;
        Size = size;
        projection = Matrix4.CreateOrthographicOffCenter(0, size.X, 0, size.Y, -10, 10);

    }

    private Texture? currentTexture;
    private DrawList drawList = null!;
    private PrimitiveType currentType;
    private Shader currentShader;

    private void EnsureDraw(Texture? texture, PrimitiveType primitiveType, Shader shader)
    {
        if (currentTexture == texture && currentType == primitiveType && currentShader == shader) 
            return;
        
        EnsureEnded();
            
        drawList = new DrawList(primitiveType);
        currentTexture = texture;
        currentType = primitiveType;
        currentShader = shader;
    }

    public void ResetShader()
    {
        EnsureDraw(currentTexture, currentType, defaultShader);
    }
    
    public void BindShader(Shader shader)
    {
        EnsureDraw(currentTexture, currentType, shader);
    }

    public void EnsureEnded()
    {
        if (drawList == null!) 
            return;
        
        renderContext.Disable(EnableCap.DepthTest);
        
        currentShader.Bind();
        currentShader.Matrix("um_projection", projection, false);
        currentTexture?.Bind(0);
        drawList.Submit();
    }
    

    public void DrawSprite(Sprite sprite, float x, float y)
    { 
        EnsureDraw(sprite.Texture, PrimitiveType.Triangles, currentShader);

        Vector2 uv0 = new Vector2(sprite.Position.X / sprite.Texture.Width, sprite.Position.Y / sprite.Texture.Height);
        Vector2 uv1 = new Vector2((sprite.Position.X + sprite.Size.X) / sprite.Texture.Width, (sprite.Position.Y + sprite.Size.Y) / sprite.Texture.Height);

        drawList.Vertex(x, y, uv0.X, uv0.Y, Colour.White);
        drawList.Vertex(x + sprite.Size.X, y, uv1.X, uv0.Y, Colour.White);
        drawList.Vertex(x + sprite.Size.X, y + sprite.Size.Y, uv1.X, uv1.Y, Colour.White);
        
        drawList.Vertex(x, y, uv0.X, uv0.Y, Colour.White);
        drawList.Vertex(x + sprite.Size.X, y + sprite.Size.Y, uv1.X, uv1.Y, Colour.White);
        drawList.Vertex(x, y + sprite.Size.Y, uv0.X, uv1.Y, Colour.White);
    }
    
    public void DrawSprite(Sprite sprite, Vector2 pos, Vector2 origin, Vector2 size, float rotation)
    {
        // Well, this is bound to be fun.
        // Just a lot... like... a LOT of maths to do.

        rotation *= Mathf.Deg2Rad;
        
        Vector2 right = new Vector2(MathF.Cos(rotation), MathF.Sin(rotation));
        Vector2 up    = new Vector2(MathF.Cos(rotation - MathF.PI / 2), MathF.Sin(rotation - MathF.PI / 2));
        //Vector2 up = new Vector2(0, 1);
        
        // Origin minus one
        // So, if origin is 0.3, 0.3 this should be 0.7, 0.7
        Vector2 originMO = new Vector2(1 - origin.X, 1 - origin.Y) * size;
        origin *= size;

        
        
        Vector2 tl = (-origin.X * right)   + (-origin.Y * up) + pos;
        Vector2 tr = ( originMO.X * right) + (-origin.Y * up) + pos;
        Vector2 bl = (-origin.X * right)   + (originMO.Y * up) + pos;
        Vector2 br = ( originMO.X * right) + (originMO.Y * up) + pos;
        
        EnsureDraw(sprite.Texture, PrimitiveType.Triangles, currentShader);

        Vector2 uv0 = new Vector2(sprite.Position.X / sprite.Texture.Width, sprite.Position.Y / sprite.Texture.Height);
        Vector2 uv1 = new Vector2((sprite.Position.X + sprite.Size.X) / sprite.Texture.Width, (sprite.Position.Y + sprite.Size.Y) / sprite.Texture.Height);

        drawList.Vertex(tl.X, tl.Y, uv0.X, uv0.Y, Colour.White);
        drawList.Vertex(tr.X, tr.Y, uv1.X, uv0.Y, Colour.White);
        drawList.Vertex(br.X, br.Y, uv1.X, uv1.Y, Colour.White);

        drawList.Vertex(tl.X, tl.Y, uv0.X, uv0.Y, Colour.White);
        drawList.Vertex(br.X, br.Y, uv1.X, uv1.Y, Colour.White);
        drawList.Vertex(bl.X, bl.Y, uv0.X, uv1.Y, Colour.White);
        
    }
}