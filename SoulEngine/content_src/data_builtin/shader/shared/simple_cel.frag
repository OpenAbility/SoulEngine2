in vec3 v_position;
in vec2 v_uv;
in vec3 v_normal;
in vec4 v_colour;

out vec4 f_colour;

uniform sampler2D ut_albedoTexture;
uniform vec4 uc_albedoColour = vec4(1);
uniform vec3 um_camera_direction;

uniform bool ub_rimlit = true;


void main() {
    
    vec3 sunDir = normalize(vec3(0, 1, 0));
    
    vec3 lighting = vec3(0.5f);
    
    vec3 normal = v_normal;
    if(!gl_FrontFacing)
        normal *= -1;
    
    lighting += clamp(dot(v_normal, sunDir), 0, 1) < 0.5 ? 0.2 : 1;
    
    f_colour = texture(ut_albedoTexture, v_uv) * v_colour * uc_albedoColour * vec4(lighting, 1);
    
    if(ub_rimlit)
        f_colour += smoothstep(0.5, 1.0, 1 - abs(dot(normal, um_camera_direction))) * 0.5f;
    
    if(f_colour.a <= 0.0f)
            discard;
    
}