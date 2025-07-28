uniform sampler2D ut_colour0;
uniform sampler2D ut_depth;

uniform float uf_brightness = 1.0;
uniform float uf_gamma = 1.0;

in vec2 v_uv;

out vec4 f_color;

float linearize_depth(float d,float zNear, float zFar)
{
    return zNear * zFar / (zFar + d * (zNear - zFar));
}

vec3 reinhard_extended(vec3 v, float max_white)
{
    vec3 numerator = v * (1.0f + (v / vec3(max_white * max_white)));
    return numerator / (1.0f + v);
}

float luminance(vec3 v)
{
    return dot(v, vec3(0.2126f, 0.7152f, 0.0722f));
}

void main() {
    
    // Initial sampling
    float depth = linearize_depth(texture(ut_depth, v_uv).r, 0.2f, 200.0f);
    f_color = texture(ut_colour0, v_uv);

    // Tone mapping
    f_color.rgb = reinhard_extended(f_color.rgb, uf_brightness);
    
    // Gamma correction
    f_color.rgb = pow(f_color.rgb, vec3(1.0/uf_gamma));
}