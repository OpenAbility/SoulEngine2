in vec3 v_position;
in vec2 v_uv;
in vec2 v_uv2;
in vec3 v_normal;
in vec3 v_tangent;
in vec4 v_colour;

out vec4 f_colour;
uniform vec4 uc_albedoColour = vec4(1);

void main() {
    
    f_colour = v_colour; // vec4(1, 0, 1, 1);
    /*
    
    f_colour = v_colour * uc_albedoColour;

    if(f_colour.a <= 0.0f)
        discard;
        */
}