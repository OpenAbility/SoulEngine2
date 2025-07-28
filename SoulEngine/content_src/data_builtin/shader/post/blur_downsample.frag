uniform sampler2D srcTexture;
in vec2 v_uv;
out vec4 f_colour;

vec4 samplePixel(vec2 uv) {
    vec4 data = texture(srcTexture, uv);
    return data;
}

void main() {

    vec2 texelSize = vec2(1) / max(textureSize(srcTexture, 0), vec2(1.0));

    f_colour = (samplePixel(v_uv)
    + samplePixel(v_uv + vec2(texelSize.x, 0.0))
    + samplePixel(v_uv + vec2(-texelSize.x, 0.0))
    + samplePixel(v_uv + vec2(0.0,  texelSize.y))
    + samplePixel(v_uv + vec2(0.0, -texelSize.y))) / 5.0;
    f_colour.a = 1;
}

/*

// This shader performs downsampling on a texture,
// as taken from Call Of Duty method, presented at ACM Siggraph 2014.
// This particular method was customly designed to eliminate
// "pulsating artifacts and temporal stability issues".

// Remember to add bilinear minification filter for this texture!
// Remember to use a floating-point texture format (for HDR)!
// Remember to use edge clamping for this texture!
uniform sampler2D srcTexture;
uniform vec2 srcResolution;

layout (location = 0) in vec2 v_uv;
layout (location = 0) out vec4 output_colour;

void main()
{
    vec3 downsample = vec3(0);
    vec2 srcTexelSize = 1.0 / srcResolution;
    float x = srcTexelSize.x;
    float y = srcTexelSize.y;

    // Take 13 samples around current texel:
    // a - b - c
    // - j - k -
    // d - e - f
    // - l - m -
    // g - h - i
    // === ('e' is the current texel) ===
    vec3 a = texture(srcTexture, vec2(v_uv.x - 2*x, v_uv.y + 2*y)).rgb;
    vec3 b = texture(srcTexture, vec2(v_uv.x,       v_uv.y + 2*y)).rgb;
    vec3 c = texture(srcTexture, vec2(v_uv.x + 2*x, v_uv.y + 2*y)).rgb;

    vec3 d = texture(srcTexture, vec2(v_uv.x - 2*x, v_uv.y)).rgb;
    vec3 e = texture(srcTexture, vec2(v_uv.x,       v_uv.y)).rgb;
    vec3 f = texture(srcTexture, vec2(v_uv.x + 2*x, v_uv.y)).rgb;

    vec3 g = texture(srcTexture, vec2(v_uv.x - 2*x, v_uv.y - 2*y)).rgb;
    vec3 h = texture(srcTexture, vec2(v_uv.x,       v_uv.y - 2*y)).rgb;
    vec3 i = texture(srcTexture, vec2(v_uv.x + 2*x, v_uv.y - 2*y)).rgb;

    vec3 j = texture(srcTexture, vec2(v_uv.x - x, v_uv.y + y)).rgb;
    vec3 k = texture(srcTexture, vec2(v_uv.x + x, v_uv.y + y)).rgb;
    vec3 l = texture(srcTexture, vec2(v_uv.x - x, v_uv.y - y)).rgb;
    vec3 m = texture(srcTexture, vec2(v_uv.x + x, v_uv.y - y)).rgb;

    // Apply weighted distribution:
    // 0.5 + 0.125 + 0.125 + 0.125 + 0.125 = 1
    // a,b,d,e * 0.125
    // b,c,e,f * 0.125
    // d,e,g,h * 0.125
    // e,f,h,i * 0.125
    // j,k,l,m * 0.5
    // This shows 5 square areas that are being sampled. But some of them overlap,
    // so to have an energy preserving downsample we need to make some adjustments.
    // The weights are the distributed, so that the sum of j,k,l,m (e.g.)
    // contribute 0.5 to the final color output. The code below is written
    // to effectively yield this sum. We get:
    // 0.125*5 + 0.03125*4 + 0.0625*4 = 1
    downsample = e*0.125f;
    downsample += (a+c+g+i)*0.03125f;
    downsample += (b+d+f+h)*0.0625f;
    downsample += (j+k+l+m)*0.125f;

    output_colour = vec4(downsample, 1);
}
*/