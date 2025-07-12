in vec3 a_position;
in vec2 a_uv;
in vec2 a_uv2;
in vec3 a_normal;
in vec4 a_colour;

out vec3 v_position;
out vec2 v_uv;
out vec2 v_uv2;
out vec3 v_normal;
out vec4 v_colour;

uniform mat4 um_projection;
uniform mat4 um_view;
uniform mat4 um_model;

void main() {
    // My IDE yells at me if I don't do the cast.
    // Dumb but oh well.
    
    vec4 p = vec4(um_projection * um_view * um_model * vec4(a_position, 1));
    gl_Position = p;

    v_position = a_position;
    v_uv = a_uv;
    v_uv2 = a_uv2;
    // We need to make sure all normals are applied world-space :D
    v_normal = normalize(mat3(transpose(inverse(um_model))) * a_normal);
    v_colour = a_colour;
}