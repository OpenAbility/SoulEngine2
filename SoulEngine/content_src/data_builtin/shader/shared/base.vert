struct VertexData {
    vec3 position;
    vec4 uv01;
    vec3 normal;
    vec3 tangent;
    vec4 colour;
};

layout(std430, binding=20) restrict readonly buffer ib_vertex_buffer {
    VertexData vertices[];
} ib_vertices;

out vec3 v_position;
out vec2 v_uv;
out vec2 v_uv2;
out vec3 v_normal;
out vec4 v_colour;

out vec4 v_shadow_positon;

uniform mat4 um_projection;
uniform mat4 um_view;
uniform mat4 um_model;

uniform mat4 um_shadow_projection;
uniform mat4 um_shadow_view;



void main() {
    // My IDE yells at me if I don't do the cast.
    // Dumb but oh well.

    VertexData v = ib_vertices.vertices[gl_VertexID];
    
    vec3 p = (um_model * vec4(v.position, 1)).xyz;
    gl_Position = um_projection * um_view * vec4(p, 1.0);
    
    v_shadow_positon = um_shadow_projection * um_shadow_view * vec4(p, 1);
    v_position = p;
    v_uv = v.uv01.xy;
    v_uv2 = v.uv01.zw;
    // We need to make sure all normals are applied world-space :D
    v_normal = normalize(mat3(transpose(inverse(um_model))) * v.normal);
    
    v_colour = v.colour;
}