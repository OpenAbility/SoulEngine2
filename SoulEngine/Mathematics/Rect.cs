using OpenTK.Mathematics;

namespace SoulEngine.Mathematics;

public struct Rect
{
    public Vector2 Origin;
    public Vector2 Extents;

    public Vector2 Min
    {
        get => Origin;
        set => Origin = value;
    }

    public Vector2 Max
    {
        get => Origin + Extents;
        set => Extents = value - Origin;
    }


    public Rect(Vector2 origin, Vector2 extents)
    {
        Origin = origin;
        Extents = extents;
    }

    public Rect(float x, float y, float w, float h)
    {
        Origin = new Vector2(x, y);
        Extents = new Vector2(w, h);
    }

    public readonly bool Inside(Vector2 point)
    {
        return point.X >= Min.X && point.X <= Max.X && point.Y >= Min.Y && point.Y <= Max.Y;
    }

    public readonly Rect Inset(float left, float top, float right, float bottom)
    {
        return Inset(new Vector2(left, top), new Vector2(right, bottom));
    }

    public readonly Rect Inset(Vector2 min, Vector2 max)
    {
        return new Rect(Origin + min, Extents - min - max);
    }
}