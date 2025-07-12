in vec3 v_position;
in vec2 v_uv;
in vec3 v_normal;
in vec4 v_colour;

in vec4 v_shadow_positon;

out vec4 f_colour;

uniform sampler2D ut_albedoTexture;
uniform vec4 uc_albedoColour = vec4(1);
uniform vec3 um_camera_direction;

uniform sampler2D ut_shadow_buffer;

uniform bool ub_rimlit = true;

float sampleShadows() {

    vec3 coords = v_shadow_positon.xyz / v_shadow_positon.z;
    coords = coords * 0.5f + 0.5f;

    float closestDepth = texture(ut_shadow_buffer, coords.xy).r;
    float currentDepth = coords.z;

    float shadow = currentDepth > closestDepth  ? 1.0 : 0.0;

    return currentDepth;
}

void main() {

    float shadows = sampleShadows();
    //f_colour = (texture(ut_albedoTexture, v_uv) * v_colour * uc_albedoColour);
    f_colour = vec4(vec3(shadows), 1);
    
    /*
    vec3 sunDir = normalize(vec3(0, 1, 0));
    
    vec3 lighting = vec3(0.5f);
    
    vec3 normal = v_normal;
    if(!gl_FrontFacing)
        normal *= -1;
    
    // clamp(dot(v_normal, sunDir), 0, 1) - 
    float illumination = sampleShadows();
    
    lighting += illumination < 0.5 ? 0.2 : 1;
    
    
    f_colour = texture(ut_albedoTexture, v_uv) * v_colour * uc_albedoColour * vec4(lighting, 1);
    
    if(ub_rimlit)
        f_colour.rgb += smoothstep(0.5, 1.0, 1 - abs(dot(normal, um_camera_direction))) * 0.5f;
    
    if(f_colour.a <= 0.0f)
            discard;
    */
    
}