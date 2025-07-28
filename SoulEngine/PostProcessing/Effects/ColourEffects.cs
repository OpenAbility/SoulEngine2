using SoulEngine.Data;
using SoulEngine.Rendering;

namespace SoulEngine.PostProcessing.Effects;

public class ColourEffects() : SinglePassEffect(0, "shader/post/colour_pass.program")
{
    protected override void BindUniforms(Shader shader)
    {
        shader.Uniform1f("uf_gamma", EngineVarContext.Global.GetFloat("e_gamma", 1));
        shader.Uniform1f("uf_brightness", EngineVarContext.Global.GetFloat("e_brightness", 1));
    }
}