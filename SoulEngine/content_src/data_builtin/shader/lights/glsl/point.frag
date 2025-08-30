// Sun shader
// provides a good basis for how light shader code ought to be written
// it's intentionally very simple

layout(location=0) out vec4 f_albedo;
layout(location=1) out vec4 f_normal;
layout(location=2) out vec4 f_light;

uniform sampler2D ut_normal;
uniform sampler2D ut_depth;
uniform sampler2D ut_colour;

// Our general direction & position
uniform vec3 uv_direction;
uniform vec3 uv_position;

// For deconstructing fragment positions
uniform mat4 um_inv_cam;

// For shadow sampling 
uniform mat4 um_shadow_projection;
uniform mat4 um_shadow_view;
uniform bool ub_shadows;
uniform sampler2D ut_shadow_buffers[3];

// Light parameters
uniform vec4 uc_colour;
uniform float uf_distance;
uniform float uf_decay;

void main() {
    // Our current location in screen space
    ivec2 screenTexel = ivec2(gl_FragCoord.xy);
    vec2 screenUV = gl_FragCoord.xy / textureSize(ut_colour, 0);
    
    // Texture data samples
    float depth = texelFetch(ut_depth, screenTexel, 0).r;
    vec3 normal = normalize(texelFetch(ut_normal, screenTexel, 0).xyz);
    
    // Calculate world coordinates of pixel
    vec4 positionClipped = um_inv_cam * vec4(screenUV * 2.0f - 1.0f, depth * 2.0f - 1.0f, 1);
    vec3 position = positionClipped.xyz / positionClipped.w;
    
    float dist = distance(position, uv_position);

    if (dist > uf_distance)
        discard;
    
    // Attenuation function from https://lisyarus.github.io/blog/posts/point-light-attenuation.html
    float percentDist = dist / uf_distance;
    float baseStrength = pow(1 - pow(percentDist, 2), 2) / (1 + uf_decay * percentDist);

    float faceStrength = clamp(dot(normalize(uv_position - position), normal), 0, 1) * baseStrength;
    
    vec3 light = uc_colour.rgb * faceStrength * uc_colour.a;
    
    f_light = vec4(light, 1);
}