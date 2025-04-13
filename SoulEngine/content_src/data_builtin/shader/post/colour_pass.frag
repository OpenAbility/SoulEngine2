uniform sampler2D ut_colour0;
uniform sampler2D ut_depth;

in vec2 v_uv;

out vec4 f_color;

float linearize_depth(float d,float zNear, float zFar)
{
    return zNear * zFar / (zFar + d * (zNear - zFar));
}


void main() {
    
    float depth = linearize_depth(texture(ut_depth, v_uv).r, 0.2f, 200.0f);
    
    
    f_color = texture(ut_colour0, v_uv);

    vec2 texelSize = vec2(1.0f) / textureSize(ut_colour0, 0);
    
    vec3 avg = vec3(0);
    int kernel_size = 9;
    
    for (int x = -kernel_size; x <= kernel_size; x++) {
        for (int y = -kernel_size; y <= kernel_size; y++) {
            avg += texture(ut_colour0, v_uv + vec2(x, y) * texelSize).rgb / (kernel_size * kernel_size * 4);
        }
    }
    
    f_color.rgb = mix(f_color.rgb, avg, smoothstep(5, 20, depth));
    
    vec2 ndc = v_uv * 2 - 1;
    
    /*
   

    vec3 strengths = vec3(0.0, 0.01, 0.015);
    for (int i = 0; i < 3; i++) {

        if(strengths[i] == 0) continue;

        vec2 slightly_offset = mix(ndc, ndc * ndc, strengths[i]);
        vec2 offset_uv = (slightly_offset + 1) / 2;
        f_color[i] = texture(ut_texture0, offset_uv)[i];
    }
    */
    
    //f_color.rgb -= length(ndc) / length(vec2(1)) * 0.02f;


}