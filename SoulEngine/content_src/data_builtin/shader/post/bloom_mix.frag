uniform sampler2D ut_colour0;
uniform sampler2D ut_blurred;

in vec2 v_uv;

out vec4 f_color;


void main() {
    f_color = texture(ut_colour0, v_uv);
    f_color.rgb += texture(ut_blurred, v_uv).rgb * 0.05f;
}