in vec3 a_position;
in vec4 a_colour;


out vec4 v_colour;

void main() {
    gl_Position = vec4(a_position, 1);
    v_colour = a_colour;
}