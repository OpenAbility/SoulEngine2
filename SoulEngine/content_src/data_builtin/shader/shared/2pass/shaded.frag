in vec3 v_position;
in vec2 v_uv;
in vec3 v_normal;
in vec4 v_colour;

in vec4 v_shadow_positon;

layout(location=0) out vec4 f_colour;
layout(location=1) out vec3 f_normal;

uniform sampler2D ut_lightTexture;

uniform sampler2D ut_albedoTexture;
uniform vec4 uc_albedoColour = vec4(1);

uniform sampler2D ut_normalTexture;

uniform bool ub_shaded;

void shade() {
    vec3 lighting = vec3(1); // = texelFetch(ut_lightTexture, ivec2(gl_FragCoord.xy), 0).rgb;
    
    
    f_colour = texture(ut_albedoTexture, v_uv) * v_colour * uc_albedoColour * vec4(lighting, 1);

    if(f_colour.a <= 0.0f)
        discard;
}

void gbuffer() {

    vec3 normal = normalize(v_normal);
    
    f_normal = normal;
    f_colour = texture(ut_albedoTexture, v_uv);
    if(f_colour.a <= 0.0f)
        discard;
}


void main() {
    
    if(ub_shaded)
        shade();
    else
        gbuffer();
    
}
