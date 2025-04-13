uniform sampler2D srcTexture;
in vec2 v_uv;
out vec4 f_colour;

void main() {

    vec2 texelSize = vec2(1) / textureSize(srcTexture, 0);
    f_colour  = (    texture(srcTexture, v_uv + vec2( 2.0*texelSize.x,            0.0))
    +      texture(srcTexture, v_uv + vec2(-2.0*texelSize.x,            0.0))
    +      texture(srcTexture, v_uv + vec2(            0.0, 2.0*texelSize.y))
    +      texture(srcTexture, v_uv + vec2(            0.0,-2.0*texelSize.y))
    +  2.0*texture(srcTexture, v_uv + vec2(     texelSize.x,     texelSize.y))
    +  2.0*texture(srcTexture, v_uv + vec2(    -texelSize.x,     texelSize.y))
    +  2.0*texture(srcTexture, v_uv + vec2(     texelSize.x,    -texelSize.y))
    +  2.0*texture(srcTexture, v_uv + vec2(    -texelSize.x,    -texelSize.y))) / 12.0;
}

/*
// This shader performs upsampling on a texture,
// as taken from Call Of Duty method, presented at ACM Siggraph 2014.

// Remember to add bilinear minification filter for this texture!
// Remember to use a floating-point texture format (for HDR)!
// Remember to use edge clamping for this texture!
uniform sampler2D srcTexture;
uniform float filterRadius = 0.005;

layout (location = 0) in vec2 v_uv;
layout (location = 0) out vec4 upsample_result;

void main()
{
    vec3 upsample = vec3(0);
    
    // The filter kernel is applied with a radius, specified in texture
    // coordinates, so that the radius will vary across mip resolutions.
    float x = filterRadius;
    float y = filterRadius;

    // Take 9 samples around current texel:
    // a - b - c
    // d - e - f
    // g - h - i
    // === ('e' is the current texel) ===
    vec3 a = texture(srcTexture, vec2(v_uv.x - x, v_uv.y + y)).rgb;
    vec3 b = texture(srcTexture, vec2(v_uv.x,     v_uv.y + y)).rgb;
    vec3 c = texture(srcTexture, vec2(v_uv.x + x, v_uv.y + y)).rgb;

    vec3 d = texture(srcTexture, vec2(v_uv.x - x, v_uv.y)).rgb;
    vec3 e = texture(srcTexture, vec2(v_uv.x,     v_uv.y)).rgb;
    vec3 f = texture(srcTexture, vec2(v_uv.x + x, v_uv.y)).rgb;

    vec3 g = texture(srcTexture, vec2(v_uv.x - x, v_uv.y - y)).rgb;
    vec3 h = texture(srcTexture, vec2(v_uv.x,     v_uv.y - y)).rgb;
    vec3 i = texture(srcTexture, vec2(v_uv.x + x, v_uv.y - y)).rgb;

    // Apply weighted distribution, by using a 3x3 tent filter:
    //  1   | 1 2 1 |
    // -- * | 2 4 2 |
    // 16   | 1 2 1 |
    upsample = e*4.0;
    upsample += (b+d+f+h)*2.0;
    upsample += (a+c+g+i);
    upsample *= 1.0 / 16.0;

    upsample_result = vec4(upsample, 1.0f);
}
*/