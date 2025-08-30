uniform sampler2D ut_colour0;
uniform sampler2D ut_depth;

uniform float uf_depthMin;
uniform float uf_depthMax;
uniform float uf_knee;

in vec2 v_uv;

out vec4 f_color;

void main() {
    float depth = texture(ut_depth, v_uv).r;
    f_color = texture(ut_colour0, v_uv);
    
    // Cut off non-bright things
    if(depth < uf_depthMin)
        f_color.a = 0;
    if(depth > uf_depthMax)
        f_color.a = 0;
}
