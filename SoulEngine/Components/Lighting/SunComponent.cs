using OpenTK.Mathematics;
using SoulEngine.Entities;
using SoulEngine.Renderer;
using SoulEngine.Rendering;

namespace SoulEngine.Components.Lighting;

[Component("light_sun")]
[Serializable]
public class SunComponent : LightComponent
{
    private readonly Shader SunShader;
    private readonly Mesh SunMesh;

    [SerializedProperty("colour")] public Colour LightColour = Colour.White;
    [SerializedProperty("strength")] public float LightStrength = 1;
    
    [SerializedProperty("ambient_colour")] public Colour AmbientColour = Colour.White;
    [SerializedProperty("ambient_strength")] public float AmbientStrength = 0.2f;
    
    public SunComponent(Entity entity) : base(entity)
    {
        SunShader = Game.ResourceManager.Load<Shader>("shader/lights/sun.program");
        SunMesh = new Mesh(Game);
        
        SunMesh.Update([
            new Vertex(new Vector3(-1, -1, 0), Vector2.Zero, Vector3.Zero),
            new Vertex(new Vector3( 3, -1, 0), Vector2.Zero, Vector3.Zero),
            new Vertex(new Vector3(-1,  3, 0), Vector2.Zero, Vector3.Zero)
        ], [0, 1, 2]);
        
    }

    private void ShaderBind(in LightSubmitInformation information)
    {
        SunShader.Uniform4f("uc_colour", new Vector4(LightColour.R, LightColour.G, LightColour.B, LightColour.A * LightStrength));
        SunShader.Uniform4f("uc_colourAmbient", new Vector4(AmbientColour.R, AmbientColour.G, AmbientColour.B, AmbientColour.A * AmbientStrength));
    }

    public override void Render(IRenderPipeline renderPipeline, float deltaTime)
    {
        LightSubmitInformation lightSubmit = new LightSubmitInformation();
        lightSubmit.Component = this;
        lightSubmit.LightMesh = SunMesh;
        lightSubmit.LightShader = SunShader;
        lightSubmit.Direction = -Entity.Forward;

        lightSubmit.ShaderBind = ShaderBind;
        
        
        renderPipeline.SubmitLightDraw(lightSubmit);
        
    }
}