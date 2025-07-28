using OpenTK.Mathematics;

namespace SoulEngine.Rendering;

/// <summary>
/// A collection of the data needed to apply skinning to a vertex
/// </summary>
public struct VertexSkinning
{
    public JointIndices Indices;
    public Vector4 Weights;
}