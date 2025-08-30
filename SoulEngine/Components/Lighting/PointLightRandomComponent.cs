using OpenTK.Mathematics;
using SoulEngine.Entities;
using SoulEngine.Mathematics;
using SoulEngine.Models;
using SoulEngine.Renderer;
using SoulEngine.Rendering;

namespace SoulEngine.Components.Lighting;

[Component("light_point_r")]
public class PointLightRandomComponent : LightComponent
{
    [SerializedProperty("count")] public int Lights;
    [SerializedProperty("seed")] public int Seed;
    [SerializedProperty("min")] public Vector3 VolumeMin;
    [SerializedProperty("max")] public Vector3 VolumeMax;
    
    private readonly Shader PointShader;
    private readonly Model PointModel;

    private LightData[] lights = [];
    private int cachedSeed = 0;
    
    public PointLightRandomComponent(Entity entity) : base(entity)
    {
        PointShader = Game.ResourceManager.Load<Shader>("shader/lights/point.program");
        PointModel = Game.ResourceManager.Load<Model>("mod/sphere_light.mdl");
    }
    
    private void ShaderBind(in LightSubmitInformation information)
    {
        LightData data = lights[(int)information.UserData!];
        
        PointShader.Uniform4f("uc_colour", data.Colour);
        PointShader.Uniform1f("uf_distance", data.Distance);
        PointShader.Uniform1f("uf_decay", data.Decay);
    }

    public override void Render(IRenderPipeline renderPipeline, float deltaTime)
    {
        if (Lights != lights.Length || cachedSeed != Seed)
        {
            cachedSeed = Seed;
            
            Random rng = new Random(Seed);
            lights = new LightData[Lights];

            for (int i = 0; i < Lights; i++)
            {
                LightData lightData = new LightData();
                lightData.Distance = Mathx.Lerp(2, 10, rng.NextSingle());
                lightData.Decay = Mathx.Lerp(1, 10, rng.NextSingle());
                lightData.Colour = new Colour(rng.NextSingle(), rng.NextSingle(), rng.NextSingle(),
                    Mathx.Lerp(1, 10, rng.NextSingle()));
                lightData.Position = new Vector3(
                    Mathx.Lerp(VolumeMin.X, VolumeMax.X, rng.NextSingle()),
                    Mathx.Lerp(VolumeMin.Y, VolumeMax.Y, rng.NextSingle()),
                    Mathx.Lerp(VolumeMin.Z, VolumeMax.Z, rng.NextSingle())
                );

                lights[i] = lightData;
            }
            
        }
        
        for (int i = 0; i < lights.Length; i++)
        {
            LightSubmitInformation lightSubmit = new LightSubmitInformation();
            lightSubmit.Component = this;
            lightSubmit.LightMesh = PointModel.Meshes[0].ActualMesh;
            lightSubmit.LightShader = PointShader;
            lightSubmit.Direction = -Entity.Forward;
            lightSubmit.Position = Entity.Position + lights[i].Position;
            lightSubmit.ModelMatrix = Matrix4.CreateScale(lights[i].Distance) * Matrix4.CreateTranslation(lights[i].Position) * Entity.GlobalMatrix.ClearScale();
            lightSubmit.UserData = i;
            lightSubmit.ShadowMaps = [];

            lightSubmit.ShaderBind = ShaderBind;
        
        
            renderPipeline.SubmitLightDraw(lightSubmit);
        }
        
        
        
    }
    
    private struct LightData
    {
        public Colour Colour;
        public float Distance;
        public float Decay;
        public Vector3 Position;
    }
}