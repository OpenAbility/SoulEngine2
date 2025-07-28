using OpenTK.Mathematics;

namespace SoulEngine.Props;

public interface ITransformable
{
    public Matrix4 LocalMatrix { get; }
    public Matrix4 GlobalMatrix { get; }
}