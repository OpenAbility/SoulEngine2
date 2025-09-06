using OpenTK.Graphics.OpenGL;
using SoulEngine.Core;
using SoulEngine.UI.Rendering;

namespace SoulEngine.PostProcessing;

public abstract class PostEffect : EngineObject, IComparable<PostEffect>
{
    public readonly int Priority;

    protected PostEffect(int priority)
    {
        Priority = priority;
    }


    public abstract void PerformEffect(PostProcessedSurface surface);

    public int CompareTo(PostEffect? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Priority.CompareTo(other.Priority);
    }

    private static DrawList drawList = new DrawList(PrimitiveType.Triangles);
    
    protected void DrawQuad()
    {
        drawList.Vertex(0, 0, 0, 0, Colour.Blank);
        drawList.Vertex(3, 0, 0, 0, Colour.Blank);
        drawList.Vertex(0, 3, 0, 0, Colour.Blank);
            
        drawList.Submit();
    }
}