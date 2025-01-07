in vec3 a_position;
in vec4 a_colour;

out vec4 v_colour;

uniform mat4 um_projection;
uniform mat4 um_view;

void main() {
    gl_Position = um_projection * um_view * vec4(a_position - 0.5f, 1);
    v_colour = a_colour;
}