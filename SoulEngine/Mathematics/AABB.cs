using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Rendering;

namespace SoulEngine.Mathematics;

/// <summary>
/// Axis-aligned bounding box
/// </summary>
public struct AABB : IEquatable<AABB>
{
    /// <summary>
    /// The lowest coordinates of the AABB
    /// </summary>
    public Vector3 Min
    {
        get => min;
        set
        {
            min = value;
            RecalculateBounds();
        }
    }

    /// <summary>
    /// The highest coordinates of the AABB
    /// </summary>
    public Vector3 Max
    {
        get => max;
        set
        {
            max = value;
            RecalculateBounds();
        }
    }

    public Vector3 Origin
    {
        get => Min;
        set
        {
            Vector3 size = Size;
            Min = value;
            Max = value + size;
        }
    }

    public Vector3 Size
    {
        get => Max - Min;
        set => Max = Min + value;
    }

    public bool Invalid => Vector3.ComponentMin(min, max) != min;

    private Vector3 min;
    private Vector3 max;

    public AABB(Vector3 min, Vector3 max)
    {
        this.min = min;
        this.max = max;
        RecalculateBounds();
    }

    public AABB RecalculateBounds()
    {
        Vector3 a = min;
        Vector3 b = max;

        min = Vector3.ComponentMin(a, b);
        max = Vector3.ComponentMax(a, b);

        return this;
    }
    
    public readonly AABB Translated(Matrix4 matrix)
    {
        Vector4 a = new Vector4(min, 1) * matrix;
        Vector4 b = new Vector4(max, 1) * matrix;

        return new AABB(a.Xyz, b.Xyz);
    }

    public readonly AABB Translated(Vector3 delta)
    {
        return new AABB()
        {
            min = this.min + delta,
            max = this.max + delta
        };
    }
    
    public readonly AABB Scaled(Vector3 scale)
    {
        return new AABB()
        {
            min = this.min * scale,
            max = this.max * scale
        }.RecalculateBounds();
    }

    public AABB SetUnchecked(Vector3 min, Vector3 max)
    {
        this.min = min;
        this.max = max;
        return this;
    }

    public AABB PushPoint(Vector3 point)
    {
        min = Vector3.ComponentMin(min, point);
        max = Vector3.ComponentMax(max, point);
        return this;
    }

    public readonly AABB Copy() => new AABB().SetUnchecked(min, max);

    public readonly bool IsInside(Vector3 point)
    {
        return point.X >= min.X && point.X <= max.X && point.Y >= min.Y && point.Y >= max.Y && point.Z >= min.Z &&
               point.Z >= max.Z;
    }

    /// <summary>
    /// An inside-out infinite AABB, useful for constructing AABBs from a series of points.
    /// Should NEVER be used without at least one <see cref="PushPoint"/> call!
    /// </summary>
    public static readonly AABB InvertedInfinity = new AABB().SetUnchecked(Vector3.PositiveInfinity, Vector3.NegativeInfinity);

    public bool Equals(AABB other)
    {
        return min.Equals(other.min) && max.Equals(other.max);
    }

    public override bool Equals(object? obj)
    {
        return obj is AABB other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(min, max);
    }

    public static bool operator ==(AABB a, AABB b)
    {
        return a.Equals(b);
    }
    
    public static bool operator !=(AABB a, AABB b)
    {
        return !a.Equals(b);
    }
    
    public void Draw(GizmoContext gizmoContext, Colour colour)
    {
        gizmoContext.Begin(PrimitiveType.Lines);
        
        gizmoContext.Vertex(Mathf.Swizzle(min, min, min), colour);
        gizmoContext.Vertex(Mathf.Swizzle(min, min, max), colour);
        
        gizmoContext.Vertex(Mathf.Swizzle(min, min, max), colour);
        gizmoContext.Vertex(Mathf.Swizzle(max, min, max), colour);
        
        gizmoContext.Vertex(Mathf.Swizzle(max, min, max), colour);
        gizmoContext.Vertex(Mathf.Swizzle(max, min, min), colour);
        
        gizmoContext.Vertex(Mathf.Swizzle(max, min, min), colour);
        gizmoContext.Vertex(Mathf.Swizzle(min, min, min), colour);
        
        gizmoContext.Vertex(Mathf.Swizzle(min, max, min), colour);
        gizmoContext.Vertex(Mathf.Swizzle(min, max, max), colour);
        
        gizmoContext.Vertex(Mathf.Swizzle(min, max, max), colour);
        gizmoContext.Vertex(Mathf.Swizzle(max, max, max), colour);
        
        gizmoContext.Vertex(Mathf.Swizzle(max, max, max), colour);
        gizmoContext.Vertex(Mathf.Swizzle(max, max, min), colour);
        
        gizmoContext.Vertex(Mathf.Swizzle(max, max, min), colour);
        gizmoContext.Vertex(Mathf.Swizzle(min, max, min), colour);
        
        gizmoContext.Vertex(Mathf.Swizzle(min, min, min), colour);
        gizmoContext.Vertex(Mathf.Swizzle(min, max, min), colour);
        
        gizmoContext.Vertex(Mathf.Swizzle(min, min, max), colour);
        gizmoContext.Vertex(Mathf.Swizzle(min, max, max), colour);
        
        gizmoContext.Vertex(Mathf.Swizzle(max, min, max), colour);
        gizmoContext.Vertex(Mathf.Swizzle(max, max, max), colour);
        
        gizmoContext.Vertex(Mathf.Swizzle(max, min, min), colour);
        gizmoContext.Vertex(Mathf.Swizzle(max, max, min), colour);
        
        gizmoContext.End();
    }
}