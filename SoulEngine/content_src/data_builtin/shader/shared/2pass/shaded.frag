in vec3 v_position;
in vec2 v_uv;
in vec3 v_normal;
in vec4 v_colour;

in vec4 v_shadow_positon;

layout(location=0) out vec4 f_colour;
layout(location=1) out vec3 f_normal;
layout(location=2) out vec4 f_light;

uniform sampler2D ut_lightBuffer;
uniform sampler2D ut_normalBuffer;

uniform sampler2D ut_albedoTexture;
uniform vec4 uc_albedoColour = vec4(1);

uniform sampler2D ut_normalTexture;
uniform bool ut_normalTexture_assigned;

uniform bool ub_shaded;

// Fetched somewhere online, dunno where tbh
mat3 ppTBN( vec3 normal, vec3 position, vec2 uv )
{
    // get edge vectors of the pixel triangle
    vec3 dp1 = dFdx( position );
    vec3 dp2 = dFdy( position );
    vec2 duv1 = dFdx( uv );
    vec2 duv2 = dFdy( uv );

    // solve the linear system
    vec3 dp2perp = cross( dp2, normal );
    vec3 dp1perp = cross( normal, dp1 );
    vec3 T = dp2perp * duv1.x + dp1perp * duv2.x;
    vec3 B = dp2perp * duv1.y + dp1perp * duv2.y;

    // construct a scale-invariant frame 
    float invmax = inversesqrt( max( dot(T,T), dot(B,B) ) );
    return mat3( T * invmax, B * invmax, normal );
}

void shade() {
    vec3 lighting = texelFetch(ut_lightBuffer, ivec2(gl_FragCoord.xy), 0).rgb;
    vec3 normal = texelFetch(ut_normalBuffer, ivec2(gl_FragCoord.xy), 0).rgb;
    vec4 albedo = texture(ut_albedoTexture, v_uv) * v_colour * uc_albedoColour;

    f_colour = albedo * vec4(lighting, 1.0f);

    if(f_colour.a <= 0.0f)
        discard;
}

void gbuffer() {

    
    vec3 normal = normalize(v_normal);
    
    if(!gl_FrontFacing)
        normal = -normal;
    
    if(ut_normalTexture_assigned) {
        mat3 tbn = ppTBN(normal, v_position, v_uv);
        vec3 normalTextureSample = normalize(texture(ut_normalTexture, v_uv).rgb * 2 - 1);
        
        normal = tbn * normalTextureSample;
    }
    
    
    f_normal = normal;
    f_colour = texture(ut_albedoTexture, v_uv);
    if(f_colour.a <= 0.0f)
        discard;
}


void main() {
    
    if(ub_shaded)
        shade();
    else
        gbuffer();
    
}
