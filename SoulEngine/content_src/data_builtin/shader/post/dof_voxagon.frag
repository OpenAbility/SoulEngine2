in vec2 v_uv;
out vec4 f_color;

uniform sampler2D ut_colour0;
uniform sampler2D ut_depth;

uniform float uf_focus;
uniform float uf_scale;
uniform vec2  uf_pixelSize;

uniform float uf_camNear;
uniform float uf_camFar;

const float GOLDEN_ANGLE = 2.39996323;
const float MAX_BLUR_SIZE = 20.0;
const float RAD_SCALE = 0.5; // Smaller = nicer blur, larger = faster

float getBlurSize(float depth, float focusPoint, float focusScale) {
    
    float coc = clamp((1.0 / focusPoint - 1.0 / depth) * focusScale, -1.0, 1.0);
    return abs(coc) * MAX_BLUR_SIZE;
}

float linearizeDepth(float d, float zNear, float zFar)
{
    float z_n = 2.0 * d - 1.0;
    return 2.0 * zNear * zFar / (zFar + zNear - z_n * (zFar - zNear));
}



void main() {

    vec2 pixelSize = uf_pixelSize;
    
    float centerDepth = linearizeDepth(texture(ut_depth, v_uv).r, uf_camNear, uf_camFar) / uf_camFar;
    float centerSize = getBlurSize(centerDepth, uf_focus, uf_scale);
    
    vec3 color = texture(ut_colour0, v_uv).rgb;
    float tot = 1.0f;
    float radius = RAD_SCALE;
    
    for (float angle = 0.0; radius < MAX_BLUR_SIZE; angle += GOLDEN_ANGLE) {
        
        vec2 tc = v_uv + vec2(cos(angle), sin(angle)) * pixelSize * radius;
        vec3 sampleColor = texture(ut_colour0, tc).rgb;
        float sampleDepth = linearizeDepth(texture(ut_depth, tc).r, uf_camNear, uf_camFar) / uf_camFar;
        
        float sampleSize = getBlurSize(sampleDepth, uf_focus, uf_scale);
        if(sampleDepth > centerDepth)
            sampleSize = clamp(sampleSize, 0.0, centerSize * 2.0);
        float m = smoothstep(radius - 0.5, radius + 0.5, sampleSize);
        
        color += mix(color / tot, sampleColor, m);
        tot += 1.0f;
        
        radius += RAD_SCALE / radius;
    }
    
    f_color.rgb = color / tot;
    f_color.a = 1.0f;
}
