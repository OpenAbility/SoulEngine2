using SoulEngine.Core;
using SoulEngine.Data.NBT;
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
    public readonly EnumProperty<QuadMode> Mode;
    public readonly ResourceProperty<Texture> Texture;
    
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
        Mode = Register(new EnumProperty<QuadMode>("mode", QuadMode.Visible));
        Texture = Register(new ResourceProperty<Texture>("texture", "tex/proto_wall_dark01.dds", () => new Texture(scene.Game), scene.Game));
    }

    public override void OnLoad(CompoundTag tag)
    {
        Console.WriteLine(Mode.Value);
    }

    public override void Update(float deltaTime)
    {
    }

    public override void Render(SceneRenderData data, float deltaTime)
    {
        if(!Visible.Value)
            return;
        
        if(Texture.Value != null)
            Texture.Value.Bind(0);
        Shader.Bind();
        Shader.Matrix("um_projection", data.CameraProjectionMatrix, false);
        Shader.Matrix("um_view", data.CameraViewMatrix, false);
        Mesh.Draw();
    }
    
    public enum QuadMode
    {
        Visible = 1,
        Hidden = 2,
    }
}