using OpenTK.Mathematics;
using SoulEngine.Core;

namespace SoulEngine.Rendering;

public class Primitives
{
    private readonly Game game;
    
    public Primitives(Game game)
    {
        this.game = game;
    }

    private static readonly Vertex[] CubeVertices =
    [
        new Vertex(new Vector3(0, 0, 0), new Vector2(0, 0), new Vector3(0, 0, -1)),
        new Vertex(new Vector3(0, 1, 0), new Vector2(0, 1), new Vector3(0, 0, -1)),
        new Vertex(new Vector3(1, 1, 0), new Vector2(1, 1), new Vector3(0, 0, -1)),
        new Vertex(new Vector3(1, 0, 0), new Vector2(1, 0), new Vector3(0, 0, -1)),
        
        new Vertex(new Vector3(0, 0, 1), new Vector2(1, 0), new Vector3(0, 0, 1)),
        new Vertex(new Vector3(0, 1, 1), new Vector2(1, 1), new Vector3(0, 0, 1)),
        new Vertex(new Vector3(1, 1, 1), new Vector2(0, 1), new Vector3(0, 0, 1)),
        new Vertex(new Vector3(1, 0, 1), new Vector2(0, 0), new Vector3(0, 0, 1)),
        
        new Vertex(new Vector3(0, 0, 1), new Vector2(0, 0), new Vector3(-1, 0, 0)),
        new Vertex(new Vector3(0, 1, 1), new Vector2(0, 1), new Vector3(-1, 0, 0)),
        new Vertex(new Vector3(0, 1, 0), new Vector2(1, 1), new Vector3(-1, 0, 0)),
        new Vertex(new Vector3(0, 0, 0), new Vector2(1, 0), new Vector3(-1, 0, 0)),
        
        new Vertex(new Vector3(1, 0, 1), new Vector2(0, 0), new Vector3(1, 0, 0)),
        new Vertex(new Vector3(1, 1, 1), new Vector2(0, 1), new Vector3(1, 0, 0)),
        new Vertex(new Vector3(1, 1, 0), new Vector2(1, 1), new Vector3(1, 0, 0)),
        new Vertex(new Vector3(1, 0, 0), new Vector2(1, 0), new Vector3(1, 0, 0)),
        
        new Vertex(new Vector3(0, 1, 0), new Vector2(0, 0), new Vector3(0, 1, 0)),
        new Vertex(new Vector3(0, 1, 1), new Vector2(0, 1), new Vector3(0, 1, 0)),
        new Vertex(new Vector3(1, 1, 1), new Vector2(1, 1), new Vector3(0, 1, 0)),
        new Vertex(new Vector3(1, 1, 0), new Vector2(1, 0), new Vector3(0, 1, 0)),
        
        new Vertex(new Vector3(0, 0, 0), new Vector2(0, 1), new Vector3(0, -1, 0)),
        new Vertex(new Vector3(0, 0, 1), new Vector2(0, 0), new Vector3(0, -1, 0)),
        new Vertex(new Vector3(1, 0, 1), new Vector2(1, 0), new Vector3(0, -1, 0)),
        new Vertex(new Vector3(1, 0, 0), new Vector2(1, 1), new Vector3(0, -1, 0)),
    ];

    private static readonly uint[] CubeIndices =
    [
        0, 1, 2, 0, 2, 3,
        
        6, 5, 4, 7, 6, 4,
        
        8, 9, 10, 8, 10, 11,
        
        14, 13, 12, 15, 14, 12,
        
        16, 17, 18, 16, 18, 19,
        
        22, 21, 20, 23, 22, 20
    ];

    public Mesh GenerateCube(Vector3 size, Vector3 offset)
    {
        Mesh mesh = new Mesh(game);
        
        MeshBuildData buildData = mesh.BeginUpdate(CubeVertices.Length, CubeIndices.Length);

        for (int i = 0; i < CubeVertices.Length; i++)
        {
            Vertex v = CubeVertices[i];
            v.Position += offset;
            v.Position *= size;

            buildData.Vertices[i] = v;
        }

        for (int i = 0; i < CubeIndices.Length; i++)
        {
            buildData.Indices[i] = CubeIndices[i];
        }
        
        mesh.EndUpdate(buildData);

        return mesh;
    }
}