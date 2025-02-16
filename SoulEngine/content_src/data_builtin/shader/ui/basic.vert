uniform mat4 um_projection;

layout(location = 0) in vec2 a_position;
layout(location = 1) in vec2 a_uv;
layout(location = 2) in vec4 a_color;

out vec4 v_color;
out vec2 v_uv;

void main() {
    gl_Position = um_projection * vec4(a_position, 0, 1);
    v_color = a_color;
    v_uv = a_uv;
}