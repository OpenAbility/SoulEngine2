// Sun shader
// provides a good basis for how light shader code ought to be written
// it's intentionally very simple

layout(location=0) out vec4 f_albedo;
layout(location=1) out vec4 f_normal;
layout(location=2) out vec4 f_light;

uniform sampler2D ut_normal;
uniform sampler2D ut_depth;
uniform sampler2D ut_colour;

// Our general direction
uniform vec3 uv_direction;

// For deconstructing fragment positions
uniform mat4 um_inv_cam;

// For shadow sampling 
uniform mat4 um_shadow_projections[3];
uniform mat4 um_shadow_views[3];
uniform bool ub_shadows;
uniform sampler2DArray ut_shadow_buffer;
uniform int ut_shadow_buffer_count;

// Light parameters
uniform vec4 uc_colour;
uniform vec4 uc_colourAmbient;


float sampleShadow(vec4 shadowPosition, vec3 surfaceNormal) {
    if(!ub_shadows || ut_shadow_buffer_count <= 0)
        return 0.0f;
    
    vec3 coordsNDC = shadowPosition.xyz / shadowPosition.w;

    // The most extreme extent of our NDC coordinates will tell the cascade level to use
    float maxExtent = max(abs(coordsNDC.x), abs(coordsNDC.y));

    // Calculate the buffer index - if we're inside buffer 0 it'll not return properly so we override it
    // We add a small epsilon to prevent sampling outside the shadow buffer (as that does happen)
    int bufferIndex = int(floor(log2(maxExtent + 0.01f))) + 1;
    if(maxExtent < 1.0f)
        bufferIndex = 0;
    // We only have 3 buffers.
    if(bufferIndex >= 3)
        return 0.0f;

    // Cast down to proper buffer scale (z-axis should remain as-is tho)
    coordsNDC.xy /= pow(2, bufferIndex);

    vec3 coordsUV = coordsNDC * 0.5f + 0.5f;
    
    float currentDepth = coordsUV.z;
    float shadowBias = 0.0f; //mix(clamp(dot(uv_direction, surfaceNormal), 0, 1), 0.0005f, 0.00005f);
    
    float sampleDepth = texture(ut_shadow_buffer, vec3(coordsUV.xy, bufferIndex)).r;
    
    return currentDepth - shadowBias > sampleDepth ? 1.0f : 0.0;
}


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
    
    // Project from the shadow POV
    vec4 shadowProject = um_shadow_projections[0] * um_shadow_views[0] * vec4(position, 1);
    
    
    float shadowStrength = sampleShadow(shadowProject, normal);
    
    float strength = clamp(dot(normal, uv_direction) - shadowStrength, 0, 1) * uc_colour.a;

    vec3 light = uc_colour.rgb * strength;
    light += uc_colourAmbient.rgb * uc_colourAmbient.a;
    
    f_light = vec4(light, 1);
}