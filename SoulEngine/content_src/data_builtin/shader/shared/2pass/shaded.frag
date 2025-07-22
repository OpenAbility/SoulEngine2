in vec3 v_position;
in vec2 v_uv;
in vec3 v_normal;
in vec4 v_colour;

in vec4 v_shadow_positon;

layout(location=0) out vec4 f_colour;
layout(location=1) out vec3 f_normal;

uniform sampler2D ut_albedoTexture;
uniform vec4 uc_albedoColour = vec4(1);

uniform sampler2D ut_shadow_buffers[3];
uniform vec3 um_shadow_direction;
uniform bool ub_shadows;

uniform bool ub_shaded;

float sampleShadows() {
    
    if(!ub_shadows)
        return 0.0f;

    vec3 coords_ndc = v_shadow_positon.xyz / v_shadow_positon.w;

    
    float maxExtent = max(abs(coords_ndc.x), abs(coords_ndc.y));

    int buffer_index = int(floor(log2(maxExtent))) + 1;
    if(maxExtent < 1.0f)
        buffer_index = 0;
    
    if(buffer_index >= 3)
        return 0.0f;
    
    coords_ndc /= pow(2, buffer_index);

    vec3 coords_uv = coords_ndc * 0.5f + 0.5f;
    
    #define shadowSampler ut_shadow_buffers[buffer_index]
    
    vec2 texelSize = vec2(1.0) / textureSize(shadowSampler, 0);
   
    float currentDepth = coords_uv.z;

    float bias = max(0.0005 * (1.0 - dot(v_normal, um_shadow_direction)), 0.0005);

    
    
    float shadow = 0.0f;

    float sampleDepth = texture(shadowSampler, coords_uv.xy).r;
    shadow += currentDepth - bias > sampleDepth  ? 1.0f : 0.0;
    
    /*
    for (int x = -1; x <= 1; x++) {
        for(int y = -1; y <= 1; y++) {
            float sampleDepth = texture(shadowSampler, coords_uv.xy + texelSize * vec2(x, y)).r;
            shadow += currentDepth - bias > sampleDepth  ? (1.0f / 9.0f) : 0.0;
        }
    }
    */
    
    #undef shadowSampler

    return shadow;
    
    
}

void shade() {
    vec3 sunDir = normalize(vec3(0, 1, 0));

    vec3 lighting = vec3(0.5f);

    vec3 normal = v_normal;
    if(!gl_FrontFacing)
    normal *= -1;

    float shadows = sampleShadows();

    float illum = clamp(dot(v_normal, sunDir) - shadows, 0, 1);

    lighting += illum;

    f_colour = texture(ut_albedoTexture, v_uv) * v_colour * uc_albedoColour * vec4(lighting, 1);

    if(f_colour.a <= 0.0f)
    discard;
}

void gbuffer() {

    vec3 normal = v_normal;
    if(!gl_FrontFacing)
        normal *= -1;

    f_normal = normal;
}


void main() {
    
    if(ub_shaded)
        shade();
    else
        gbuffer();
    
}
