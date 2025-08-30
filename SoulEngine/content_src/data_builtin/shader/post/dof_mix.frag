uniform sampler2D ut_colour0;

uniform sampler2D ut_near;
uniform sampler2D ut_focus;
uniform sampler2D ut_far;

in vec2 v_uv;

out vec4 f_color;


void main() {
    
    vec4 near = texture(ut_near, v_uv);
    vec4 focus = texture(ut_focus, v_uv);
    vec4 far = texture(ut_far, v_uv);
    
    f_color = far;
    if(focus.r + focus.g + focus.b > 0)
        f_color = focus;
    f_color = mix(f_color, near, near.a);
}