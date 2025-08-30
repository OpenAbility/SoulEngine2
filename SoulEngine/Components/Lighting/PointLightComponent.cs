using OpenTK.Mathematics;
using SoulEngine.Entities;
using SoulEngine.Models;
using SoulEngine.Renderer;
using SoulEngine.Rendering;

namespace SoulEngine.Components.Lighting;

[Component("light_point")]
public class PointLightComponent : LightComponent
{
    [SerializedProperty("colour")] public Colour LightColour = Colour.White;
    [SerializedProperty("strength")] public float LightStrength = 1;

    [SerializedProperty("distance")] public float Distance = 1.0f;
    [SerializedProperty("decay")] public float Decay = 1.0f;
    
    private readonly Shader PointShader;
    private readonly Model PointModel;
    
    public PointLightComponent(Entity entity) : base(entity)
    {
        PointShader = Game.ResourceManager.Load<Shader>("shader/lights/point.program");
        PointModel = Game.ResourceManager.Load<Model>("mod/sphere_light.mdl");

        
        
        
    }
    
    private void ShaderBind(in LightSubmitInformation information)
    {
        PointShader.Uniform4f("uc_colour", new Vector4(LightColour.R, LightColour.G, LightColour.B, LightColour.A * LightStrength));
        PointShader.Uniform1f("uf_distance", Distance);
        PointShader.Uniform1f("uf_decay", Decay);
    }

    public override void Render(IRenderPipeline renderPipeline, float deltaTime)
    {
        LightSubmitInformation lightSubmit = new LightSubmitInformation();
        lightSubmit.Component = this;
        lightSubmit.LightMesh = PointModel.Meshes[0].ActualMesh;
        lightSubmit.LightShader = PointShader;
        lightSubmit.Direction = -Entity.Forward;
        lightSubmit.Position = Entity.Position;
        lightSubmit.ModelMatrix = Matrix4.CreateScale(Distance) * Entity.GlobalMatrix.ClearScale();
        lightSubmit.ShadowMaps = [];
        
        lightSubmit.ShaderBind = ShaderBind;
        
        
        renderPipeline.SubmitLightDraw(lightSubmit);
        
    }
}