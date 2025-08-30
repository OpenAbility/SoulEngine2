using OpenTK.Mathematics;
using SoulEngine.Entities;
using SoulEngine.Mathematics;
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
    
    [SerializedProperty("shadows")] public bool Shadows = false;
    [SerializedProperty("shadow_size")] public float ShadowSize = 10;
    [SerializedProperty("shadow_levels")] public int ShadowLevels = 3;
    [SerializedProperty("shadow_resolution")] public int ShadowResolution = 1024;
    
    [SerializedProperty("shadow_near")] public float ShadowNear = -50.0f;
    [SerializedProperty("shadow_far")] public float ShadowFar = 50.0f;
    
    public SunComponent(Entity entity) : base(entity)
    {
        SunShader = Game.ResourceManager.Load<Shader>("shader/lights/sun.program");
        SunMesh = new Mesh(Game);
        
        SunMesh.Update([
            new Vertex(new Vector3(-1, -1, 0), Vector2.Zero, Vector3.Zero),
            new Vertex(new Vector3( 3, -1, 0), Vector2.Zero, Vector3.Zero),
            new Vertex(new Vector3(-1,  3, 0), Vector2.Zero, Vector3.Zero)
        ], [2, 1, 0]);
        
    }

    private void ShaderBind(in LightSubmitInformation information)
    {
        SunShader.Uniform4f("uc_colour", new Vector4(LightColour.R, LightColour.G, LightColour.B, LightColour.A * LightStrength));
        SunShader.Uniform4f("uc_colourAmbient", new Vector4(AmbientColour.R, AmbientColour.G, AmbientColour.B, AmbientColour.A * AmbientStrength));
    }

    private CSMShadowBuffer? shadowBuffer;
    
    public override void Render(IRenderPipeline renderPipeline, float deltaTime)
    {
        LightSubmitInformation lightSubmit = new LightSubmitInformation();
        lightSubmit.Component = this;
        lightSubmit.LightMesh = SunMesh;
        lightSubmit.LightShader = SunShader;
        lightSubmit.Direction = -Entity.Forward;
        lightSubmit.ShadowMaps = [];
        
        if(Shadows && ShadowLevels > 0) {
            int levels = Math.Clamp(ShadowLevels, 0, 3);

            int res = ShadowResolution;

            if (shadowBuffer == null || shadowBuffer.Levels != levels || shadowBuffer.Resolution != res)
            {
                shadowBuffer = new CSMShadowBuffer(res, levels);
            }
            
            Vector3 origin = Entity.Position;
            if (Scene.Camera != null)
                origin += Scene.Camera.Entity.Position;

            lightSubmit.ShadowMaps = new ShadowLevelInformation[levels];
            lightSubmit.ShadowBuffer = shadowBuffer;
            
            float size = ShadowSize;
            for (int i = 0; i < levels; i++)
            {
                
                
                ShadowLevelInformation shadowLevelInformation = new ShadowLevelInformation();
                shadowLevelInformation.ViewMatrix =
                    Matrix4.LookAt(origin, origin + Entity.Forward, Vector3.UnitY);
                shadowLevelInformation.ProjectionMatrix = Matrix4.CreateOrthographic(size, size, ShadowNear, ShadowFar);

                lightSubmit.ShadowMaps[i] = shadowLevelInformation;
                
                size *= 2;
            }

        }

        lightSubmit.ShaderBind = ShaderBind;
        
        
        renderPipeline.SubmitLightDraw(lightSubmit);
        
    }
}