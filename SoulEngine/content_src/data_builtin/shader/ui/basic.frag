uniform sampler2D ut_texture0;

in vec4 v_color;
in vec2 v_uv;

out vec4 f_color;

void main() {
    f_color = v_color * texture(ut_texture0, v_uv);
}