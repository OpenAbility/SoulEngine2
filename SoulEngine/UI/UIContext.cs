using System.Diagnostics.Contracts;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Mathematics;
using SoulEngine.Rendering;
using SoulEngine.UI.Rendering;
using SoulEngine.UI.Text;
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

    private readonly Queue<UIDraw> draws = new Queue<UIDraw>();

    private Sprite noneSprite;

    public UIContext(Game game)
    {
        Game = game;
        defaultShader = game.ResourceManager.Load<Shader>("shader/ui.program");
        currentShader = defaultShader;

        noneSprite = new Sprite(game.ResourceManager.Load<Texture>("__TEXTURE_AUTOGEN/white"), new Vector2i(0, 0),
            new Vector2i(1, 1));
    }

    public void OnBegin(Vector2i size)
    {
        Size = size;
        projection = Matrix4.CreateOrthographicOffCenter(0, size.X, size.Y, 0, -10, 10);

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
        
        UIDraw uiDraw = new UIDraw();
        uiDraw.Shader = currentShader;
        uiDraw.Texture = currentTexture;
        uiDraw.DrawList = drawList;
        
        draws.Enqueue(uiDraw);
    }
    

    public void DrawSprite(Sprite? sprite, float x, float y, Colour tint)
    {
        sprite ??= noneSprite;
        
        EnsureDraw(sprite.Texture, PrimitiveType.Triangles, currentShader);

        Vector2 uv0 = new Vector2(sprite.Position.X / sprite.Texture.Width, (sprite.Position.Y + sprite.Size.Y) / sprite.Texture.Height);
        Vector2 uv1 = new Vector2((sprite.Position.X + sprite.Size.X) / sprite.Texture.Width, sprite.Position.Y / sprite.Texture.Height);

        drawList.Vertex(x, y, uv0.X, uv0.Y, tint);
        drawList.Vertex(x + sprite.Size.X, y, uv1.X, uv0.Y, tint);
        drawList.Vertex(x + sprite.Size.X, y + sprite.Size.Y, uv1.X, uv1.Y, tint);
        
        drawList.Vertex(x, y, uv0.X, uv0.Y, tint);
        drawList.Vertex(x + sprite.Size.X, y + sprite.Size.Y, uv1.X, uv1.Y, tint);
        drawList.Vertex(x, y + sprite.Size.Y, uv0.X, uv1.Y, tint);
    }
    
    public void DrawSprite(Sprite? sprite, float x, float y, float w, float h, Colour tint)
    {
        sprite ??= noneSprite;
        
        EnsureDraw(sprite.Texture, PrimitiveType.Triangles, currentShader);

        Vector2 uv0 = new Vector2(sprite.Position.X / sprite.Texture.Width, (sprite.Position.Y + sprite.Size.Y) / sprite.Texture.Height);
        Vector2 uv1 = new Vector2((sprite.Position.X + sprite.Size.X) / sprite.Texture.Width, sprite.Position.Y / sprite.Texture.Height);

        drawList.Vertex(x, y, uv0.X, uv0.Y, tint);
        drawList.Vertex(x + w, y, uv1.X, uv0.Y, tint);
        drawList.Vertex(x + w, y + h, uv1.X, uv1.Y, tint);
        
        drawList.Vertex(x, y, uv0.X, uv0.Y, tint);
        drawList.Vertex(x + w, y + h, uv1.X, uv1.Y, tint);
        drawList.Vertex(x, y + h, uv0.X, uv1.Y, tint);
    }
    
    public void DrawSprite(Sprite? sprite, Vector2 pos, Vector2 origin, Vector2 size, float rotation, Colour tint)
    {
        sprite ??= noneSprite;
        
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

        Vector2 uv0 = new Vector2(sprite.Position.X / sprite.Texture.Width, (sprite.Position.Y + sprite.Size.Y) / sprite.Texture.Height);
        Vector2 uv1 = new Vector2((sprite.Position.X + sprite.Size.X) / sprite.Texture.Width, sprite.Position.Y / sprite.Texture.Height);

        drawList.Vertex(tl.X, tl.Y, uv0.X, uv0.Y, tint);
        drawList.Vertex(tr.X, tr.Y, uv1.X, uv0.Y, tint);
        drawList.Vertex(br.X, br.Y, uv1.X, uv1.Y, tint);

        drawList.Vertex(tl.X, tl.Y, uv0.X, uv0.Y, tint);
        drawList.Vertex(br.X, br.Y, uv1.X, uv1.Y, tint);
        drawList.Vertex(bl.X, bl.Y, uv0.X, uv1.Y, tint);
    }
    

    public void DrawText(Font font, float x, float y, string text, Colour tint)
    {
        float startX = x;
        
        foreach (var character in text)
        {
            if (character == '\n')
            {
                y += font.LineHeight;
                x = startX;
                continue;
            }
            
            Glyph? glyph = font[character];
            if(glyph == null)
                continue;
            
            DrawSprite(glyph.Value.Sprite, x + glyph.Value.Offset.X, glyph.Value.Offset.Y + y, tint);

            x += glyph.Value.Advance;
        }
    }
    
    public void DrawTextContinuable(Font font, float startX, ref float x, ref float y, string text, Colour tint)
    {
        foreach (var character in text)
        {
            if (character == '\n')
            {
                y += font.LineHeight;
                x = startX;
                continue;
            }
            
            Glyph? glyph = font[character];
            if(glyph == null)
                continue;
            
            DrawSprite(glyph.Value.Sprite, x + glyph.Value.Offset.X, glyph.Value.Offset.Y + y, tint);

            x += glyph.Value.Advance;
        }
    }
    
    [Pure]
    public Vector2 MeasureText(Font font, string text)
    { 
        float x = 0;
        float y = 0;

        float maxX = 0;
        
        float startX = x;
        
        foreach (var character in text)
        {
            if (character == '\n')
            {
                y += font.LineHeight;
                x = startX;
                continue;
            }
            
            Glyph? glyph = font[character];
            if(glyph == null)
                continue;
            
            x += glyph.Value.Advance;

            if (x >= maxX)
                maxX = x;
        }

        return new Vector2(maxX, y);
    }
    
    private struct UIDraw
    {
        public Texture? Texture;
        public DrawList DrawList;
        public Shader Shader;
    }

    public void DrawAll()
    {
        while (draws.Count > 0)
        {
            UIDraw draw = draws.Dequeue();
            
            draw.Shader.Bind();
            draw.Shader.Matrix("um_projection", projection, false);
            draw.Texture?.Bind(0);
            draw.DrawList.Submit();
        }
    }
}