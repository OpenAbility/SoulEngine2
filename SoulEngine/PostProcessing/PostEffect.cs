using OpenTK.Graphics.OpenGL;
using SoulEngine.UI.Rendering;

namespace SoulEngine.PostProcessing;

public abstract class PostEffect : IComparable<PostEffect>
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

    private static DrawList drawList = new DrawList(PrimitiveType.TriangleFan);
    
    protected void DrawQuad()
    {
        drawList.Vertex(0, 0, 0, 0, Colour.Blank);
        drawList.Vertex(1, 0, 0, 0, Colour.Blank);
        drawList.Vertex(1, 1, 0, 0, Colour.Blank);
        drawList.Vertex(0, 1, 0, 0, Colour.Blank);
            
        drawList.Submit();
    }
}