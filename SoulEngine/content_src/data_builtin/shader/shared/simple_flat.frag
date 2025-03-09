in vec3 v_position;
in vec2 v_uv;
in vec3 v_normal;
in vec4 v_colour;

out vec4 f_colour;

uniform sampler2D ut_albedoTexture;
uniform vec4 uc_albedoColour = vec4(1);



void main() {
    
    vec3 sunDir = normalize(vec3(0, 1, 0));
    
    vec3 lighting = vec3(0.5f);
    
    vec3 normal = v_normal;
    
    f_colour = texture(ut_albedoTexture, v_uv) * v_colour * uc_albedoColour;
    
    if(f_colour.a < 0.1f)
            discard;
}