using SoulEngine.Core;
using SoulEngine.Rendering;
using SoulEngine.Util;

namespace SoulEngine.Props;

[Prop("quad")]
[Serializable]
public class QuadProp : Prop
{

    private readonly Mesh<Vertex> Mesh;
    private readonly Shader Shader;
    public readonly BoolProperty Visible;
    
    public QuadProp(Scene scene, string type, string name) : base(scene, type, name)
    {
        Mesh = new Mesh<Vertex>(scene.Game);
        Shader = scene.Game.ResourceManager.Load<Shader>("shader/simple.program");

        scene.Game.ThreadSafety.EnsureMain(() =>
        {
            Mesh.Update([
                new Vertex(0, 0, 0, new Colour(0, 0, 0)),
                new Vertex(0, 1, 0, new Colour(0, 1, 0)),
                new Vertex(1, 1, 0, new Colour(1, 1, 0)),
                new Vertex(1, 0, 0, new Colour(1, 0, 0))
            ], [
                0, 1, 2,
                0, 2, 3
            ]);
        });
        
        Visible = Register(new BoolProperty("visible", true));
    }

    public override void Update(float deltaTime)
    {
    }

    public override void Render(float deltaTime)
    {
        if(!Visible.Value)
            return;
        
        Shader.Bind();
        Mesh.Draw();
    }
}