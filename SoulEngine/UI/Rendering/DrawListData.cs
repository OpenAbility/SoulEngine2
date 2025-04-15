using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Resources;

namespace SoulEngine.UI.Rendering;

public struct DrawListData
{
    public UIVertex[] Vertices;
    public PrimitiveType PrimitiveType;

    public Material Material;
    public Matrix4 ModelMatrix;
}